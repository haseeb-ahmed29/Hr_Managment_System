using HrMangmentSystem_Application.Config;
using HrMangmentSystem_Application.Interfaces.OpenAi;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace HrMangmentSystem_Application.Implementation.OpenAi
{
    public class OpenAiCvScoringClient : IOpenAiCvScoringClient
    {
        private readonly OpenAiOptions _openAiOptions;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OpenAiCvScoringClient> _logger;
        public OpenAiCvScoringClient(
             IOptions<OpenAiOptions> options,
            IHttpClientFactory httpClientFactory,
            ILogger<OpenAiCvScoringClient> logger)
        {
            _openAiOptions = options.Value;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<double> GetScoreAsync(string jobText, string pdfPhysicalPath)
        {
            if (string.IsNullOrWhiteSpace(_openAiOptions.ApiKey) ||
                string.IsNullOrWhiteSpace(_openAiOptions.Model) ||
                string.IsNullOrWhiteSpace(_openAiOptions.BaseUrl))
            {
                _logger.LogError("OpenAI CV Scoring: API key / model / baseUrl is not configured.");
                throw new InvalidOperationException("OpenAI Api is not Configured");
            }

            if (!File.Exists(pdfPhysicalPath))
            {
                _logger.LogError("OpenAI CV Scoring: PDF file not found at path {Path}", pdfPhysicalPath);
                throw new FileNotFoundException("CV PDF file not found.", pdfPhysicalPath);
            }

            // 1) Read PDF and encode Base64 with proper data URI prefix
            var pdfBytes = await File.ReadAllBytesAsync(pdfPhysicalPath);
            var base64Pdf = Convert.ToBase64String(pdfBytes);
            var fileData = $"data:application/pdf;base64,{base64Pdf}";

            _logger.LogInformation(
                "CV Ranking paths: FilePath={FilePath}, Physical={Physical}",
                pdfPhysicalPath, pdfPhysicalPath);

            // 2) Build the JSON Schema for structured outputs
            var textFormat = new
            {
                type = "json_schema",
                name = "cv_rank",
                schema = new
                {
                    type = "object",
                    additionalProperties = false,
                    properties = new
                    {
                        score = new
                        {
                            type = "number",
                            minimum = 0,
                            maximum = 100
                        }
                    },
                    required = new[] { "score" }
                },
                strict = true
            };

            // 3) Build the request body for /v1/responses
            var body = new
            {
                model = _openAiOptions.Model,
                input = new object[]
                {
                    new
                    {
                        role = "system",
                        content = new object[]
                        {
                            new
                            {
                                type = "input_text",
                                text = "You are an HR screening assistant. You must return ONLY JSON that matches the provided schema."
                            }
                        }
                    },
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new
                            {
                                type = "input_text",
                                text =
                                    "You will receive:\n" +
                                    "1) Job description text.\n" +
                                    "2) A candidate CV as a PDF file.\n\n" +
                                    "Task:\n" +
                                    "- Compare the CV to the job description.\n" +
                                    "- Return a single numeric field 'score' between 0 and 100.\n" +
                                    "- 0 means the CV is completely irrelevant.\n" +
                                    "- 100 means a perfect match.\n\n" +
                                    "Job description:\n" + jobText
                            },
                            new
                            {
                                type = "input_file",
                                filename = Path.GetFileName(pdfPhysicalPath),
                                file_data = fileData
                            }
                        }
                    }
                },
                text = new
                {
                    format = textFormat
                },
                max_output_tokens = 200
            };
            // 4) Send the HTTP request to OpenAI
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _openAiOptions.ApiKey); 

            var json = JsonSerializer.Serialize(body); // Serialize the request body to JSON
            var requestContent = new StringContent(json, Encoding.UTF8, "application/json");  // Set content type to application/json

            _logger.LogInformation("Sending OpenAI CV ranking request to {Url}", _openAiOptions.BaseUrl); 

            using var resp = await client.PostAsync(_openAiOptions.BaseUrl, requestContent); // Send POST request
            var respText = await resp.Content.ReadAsStringAsync(); // Read response content as string

            if (!resp.IsSuccessStatusCode) 
            {
                _logger.LogError("OpenAI scoring failed. Status={Status}. Body={Body}", resp.StatusCode, respText);
                throw new InvalidOperationException("OpenAI request failed.");
            }

            // 4) Extract score from response JSON
            var score = ExtractScoreFromOpenAiResponse(respText);
            _logger.LogInformation("OpenAI CV scoring succeeded. Score={Score}", score);

            return score;
        }

        private double ExtractScoreFromOpenAiResponse(string respText)
        {
            using var doc = JsonDocument.Parse(respText); // Parse response JSON
            var root = doc.RootElement; // Get root element

            // Try direct score extraction
            if (TryGetScoreFromElement(root, out var directScore))
            {
                return directScore;
            }

            // Try structured output extraction
            if (root.TryGetProperty("output_parsed", out var parsedElement))
            {     // Check if output_parsed is an object or array
                if (parsedElement.ValueKind == JsonValueKind.Object &&
                    TryGetScoreFromElement(parsedElement, out var parsedScore))// If it's an object, try to get score directly
                {
                    return parsedScore;
                }
                // If it's an array, try to get score from the first element
                if (parsedElement.ValueKind == JsonValueKind.Array &&
                    parsedElement.GetArrayLength() > 0 &&
                    TryGetScoreFromElement(parsedElement[0], out var firstParsedScore))
                {
                    return firstParsedScore;
                }
            }

            /// Try nested content extraction
            if (root.TryGetProperty("output", out var outputArray) &&
                outputArray.ValueKind == JsonValueKind.Array)
            {   // Check if output is an array
                foreach (var outputItem in outputArray.EnumerateArray())
                {
                    if (!outputItem.TryGetProperty("content", out var contentArray) ||
                        contentArray.ValueKind != JsonValueKind.Array)
                    {
                        continue;
                    }
                    // Iterate through content parts
                    foreach (var part in contentArray.EnumerateArray())
                    {
                        if (!part.TryGetProperty("type", out var typeProp))
                            continue;

                        var type = typeProp.GetString();
                        if (!string.Equals(type, "output_text", StringComparison.OrdinalIgnoreCase))
                            continue;

                        if (!part.TryGetProperty("text", out var textProp))
                            continue;

                        string? textString = null;

                        // Handle text as string or object with value property
                        if (textProp.ValueKind == JsonValueKind.String)
                        {
                            textString = textProp.GetString();
                        }
                        
                        else if (textProp.ValueKind == JsonValueKind.Object &&
                                 textProp.TryGetProperty("value", out var valueProp) &&
                                 valueProp.ValueKind == JsonValueKind.String)
                        {
                            textString = valueProp.GetString();
                        }

                        if (string.IsNullOrWhiteSpace(textString))
                            continue;

                        
                        try
                        {   // Try parsing the text content as JSON
                            using var inner = JsonDocument.Parse(textString);
                            if (TryGetScoreFromElement(inner.RootElement, out var innerScore))
                            {
                                return innerScore;
                            }
                        }
                        catch (JsonException ex)
                        {
                            
                            _logger.LogError("Failed to parse text content as JSON: {Message}", ex.Message);
                        }
                    }
                }
            }



            // Fallback: Use regex to find score in raw response text
            var match = Regex.Match(
                respText,
                "\"score\"\\s*:\\s*([0-9]+(\\.[0-9]+)?)",
                RegexOptions.IgnoreCase);

            if (match.Success &&
                double.TryParse(match.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var regexScore))
            {
                return regexScore;
            }

            _logger.LogError("OpenAI CV scoring: could not find numeric 'score' in response: {Body}", respText);
            throw new InvalidOperationException("OpenAI response did not contain a numeric 'score' field.");
        }

        private static bool TryGetScoreFromElement(JsonElement element, out double score)
        {
            score = 0;

            if (element.ValueKind != JsonValueKind.Object)
                return false;

            if (!element.TryGetProperty("score", out var scoreProp))
                return false;

            if (scoreProp.ValueKind != JsonValueKind.Number)
                return false;

            score = scoreProp.GetDouble();
            return true;
        }
    }
}
    