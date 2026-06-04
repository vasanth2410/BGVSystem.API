using BGVSystem.Application.DTOs.Notifications;
using BGVSystem.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BGVSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public TestController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("send-test-email")]
        public async Task<IActionResult> SendTestEmail()
        {
            await _emailService.SendEmailAsync(
                new SendEmailDto
                {
                    To = "yourpersonalemail@gmail.com",
                    Subject = "BGV Test Email",
                    Body = "Email service is working."
                });

            return Ok(
                "Email sent successfully");
        }
    }
}
