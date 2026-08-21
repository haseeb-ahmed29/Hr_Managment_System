using HrMangmentSystem_Application.Common.Responses;

namespace HrMangmentSystem_Application.Interfaces.Services
{
    public interface ICvRankingService 
    {
        Task<ApiResponse<double>> CalculateMatchScoreAsync(int jobApplicationId);
    }
}
