using AccountService.Application.Commands.ImageCommand;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AccountService.Api.Controllers
{
    [Route("api/image")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ImageController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] UploadImageCommand file)
        {
            if (file.Image == null || file.Image.Length == 0)
                return BadRequest("No file provided.");

            
            var url = await _mediator.Send(file);

            return Ok(new { imageUrl = url });
        }
    }
}