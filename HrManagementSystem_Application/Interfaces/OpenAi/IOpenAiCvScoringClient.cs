namespace HrMangmentSystem_Application.Interfaces.OpenAi
{
    public interface IOpenAiCvScoringClient
    {
        Task<double> GetScoreAsync(string jobText, string pdfPhysicalPath);
    }
}
