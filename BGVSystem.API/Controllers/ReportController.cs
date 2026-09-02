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
        public async Task<IActionResult> ExportCandidates()
        {
            var file = await _reportService.ExportCandidatesAsync();
            return File(
                file,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Candidates.xlsx");
        }

        [HttpGet("candidate/{candidateId}/pdf")]
        [AllowAnonymous]
        public async Task<IActionResult> DownloadCandidatePdfReport(int candidateId)
        {
            try
            {
                var pdfBytes = await _reportService.GenerateCandidatePdfReportAsync(candidateId);
                return File(
                    pdfBytes,
                    "application/pdf",
                    $"BGV_Verification_Report_{candidateId}.pdf");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PDF ERROR] Exception generating PDF report for candidate {candidateId}: {ex}");
                return StatusCode(500, new { Message = "Failed to generate PDF report", Error = ex.ToString() });
            }
        }
    }
}
