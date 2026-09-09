using BGVSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BGVSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ICandidateRepository _candidateRepository;

        public ReportController(
            IReportService reportService,
            ICandidateRepository candidateRepository)
        {
            _reportService = reportService;
            _candidateRepository = candidateRepository;
        }

        [HttpGet("candidates")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportCandidates()
        {
            var file = await _reportService.ExportCandidatesAsync();
            return File(
                file,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Candidates.xlsx");
        }

        [HttpGet("candidate/{candidateId}/pdf")]
        [Authorize(Roles = "Admin,Candidate")]
        public async Task<IActionResult> DownloadCandidatePdfReport(int candidateId)
        {
            try
            {
                if (User.IsInRole("Candidate"))
                {
                    var email = User.FindFirstValue(ClaimTypes.Email);
                    if (string.IsNullOrEmpty(email))
                    {
                        return StatusCode(StatusCodes.Status403Forbidden, new { Message = "Access denied: Missing candidate identity email claim." });
                    }

                    var candidate = await _candidateRepository.GetByEmailAsync(email);
                    if (candidate == null || candidate.Id != candidateId)
                    {
                        return StatusCode(StatusCodes.Status403Forbidden, new { Message = "Access denied: You can only download your own PDF report." });
                    }
                }

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
