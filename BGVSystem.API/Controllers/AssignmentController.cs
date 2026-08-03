using BGVSystem.Application.DTOs.Assignments;
using BGVSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BGVSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize(Roles = "Admin")]
    public class AssignmentController
    : ControllerBase
    {
        private readonly IAssignmentService
            _assignmentService;

        public AssignmentController(
            IAssignmentService
                assignmentService)
        {
            _assignmentService =
                assignmentService;
        }

        [HttpPost]
        public async Task<IActionResult>
            Create(
                CreateAssignmentDto dto)
        {
            var result =
                await _assignmentService
                    .CreateAsync(dto);

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult>
            GetAll()
        {
            var result =
                await _assignmentService
                    .GetAllAsync();

            return Ok(result);
        }

        [HttpGet("reviewer/{reviewerId}")]
        public async Task<IActionResult>
    GetByReviewerId(int reviewerId)
        {
            var result =
                await _assignmentService
                    .GetByReviewerIdAsync(reviewerId);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _assignmentService.DeleteAsync(id);
            return Ok(new { message = "Assignment deleted successfully" });
        }

        [HttpPost("cleanup-duplicates")]
        public async Task<IActionResult> CleanupDuplicates()
        {
            var removedCount = await _assignmentService.CleanupDuplicatesAsync();
            return Ok(new { message = $"Removed {removedCount} duplicate assignment(s)" });
        }
    }
}
