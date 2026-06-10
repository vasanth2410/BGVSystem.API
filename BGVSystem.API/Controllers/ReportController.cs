using BGVSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BGVSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ReportController
    : ControllerBase
    {
        private readonly
            IReportService
            _reportService;

        public ReportController(
            IReportService reportService)
        {
            _reportService =
                reportService;
        }

        [HttpGet("candidates")]
        public async Task<IActionResult>
            ExportCandidates()
        {
            var file =
                await _reportService
                    .ExportCandidatesAsync();

            return File(
                file,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Candidates.xlsx");
        }
    }
}
