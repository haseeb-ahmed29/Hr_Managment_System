using HrMangmentSystem_Application.Common.Responses;
using HrMangmentSystem_Application.Interfaces.Services;
using HrMangmentSystem_Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrManagmentSystem_API.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = RoleNames.HrAdmin + "," + RoleNames.Recruiter +  "," + RoleNames.SystemAdmin)]
    [ApiController]
    public class AiCvRankingController : ControllerBase
    {
        private readonly ICvRankingService _cvRankingService;

        public AiCvRankingController(ICvRankingService cvRankingService)
        {
            _cvRankingService = cvRankingService;
        }

        // POST: 
        [HttpPost("rank/{jobApplicationId:int}")]
        public async Task<ActionResult<ApiResponse<double>>> RankCv(int jobApplicationId)
        {
            var result = await _cvRankingService.CalculateMatchScoreAsync(jobApplicationId);
            return Ok(result);
        }
    }
}
