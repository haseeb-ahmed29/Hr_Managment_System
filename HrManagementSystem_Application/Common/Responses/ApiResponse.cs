namespace HrMangmentSystem_Application.Common.Responses
{
    public class ApiResponse<T>
    {
     
            public bool Success { get; init; }
            public string? Message { get; init; }

            public T? Data { get; init; }

            public Dictionary<string, string[]>? Errors { get; init; }
            public string? TraceId { get; init; } // Optional trace identifier for debugging purposes

        public static ApiResponse<T> Ok(T data, string? message = null, string? traceId = null)
                => new() { Success = true, Data = data, Message = message, TraceId = traceId }; // new instance of ApiResponse with success true and data

        public static ApiResponse<T> Fail(string message, Dictionary<string, string[]>? error = null, string? traceId = null)
                => new() { Success = false, Message = message, Errors = error, TraceId = traceId };

           
        }
    
    
}
