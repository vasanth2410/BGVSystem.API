using BGVSystem.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BGVSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _adminDashboardService;
        public AdminDashboardController(IAdminDashboardService adminDashboardService)
        {
            _adminDashboardService = adminDashboardService;
        }

        [HttpGet("pending-candidates")]
        public async Task<IActionResult>
    GetPendingCandidates()
        {
            var result =
                await _adminDashboardService
                    .GetPendingCandidatesAsync();

            return Ok(result);
        }

        [HttpGet("approved-candidates")]
        public async Task<IActionResult>
    GetCompletedCandidates()
        {
            var result =
                await _adminDashboardService
                    .GetCompletedCandidatesAsync();

            return Ok(result);
        }

        [HttpGet("rejected-candidates")]
        public async Task<IActionResult>
    GetRejectedCandidates()
        {
            var result =
                await _adminDashboardService
                    .GetRejectedCandidatesAsync();

            return Ok(result);
        }

        [HttpGet("summary")]
        public async Task<IActionResult>
    GetSummary()
        {
            var result =
                await _adminDashboardService
                    .GetSummaryAsync();

            return Ok(result);
        }
    }
}
