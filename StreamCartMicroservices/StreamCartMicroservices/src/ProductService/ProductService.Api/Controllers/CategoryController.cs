using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Commands.CategoryCommands;
using ProductService.Application.DTOs.Attributes;
using ProductService.Application.DTOs.Category;
using ProductService.Domain.Entities;
using Shared.Common.Models;
using System.Security.Claims;

namespace ProductService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IMediator _mediator;
        public CategoryController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<Category>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCatgoryDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid attribute value data"));

            try
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var command = new CreateCategoryCommand()
                {
                    CategoryName = request.CategoryName,
                    Description = request.Description,
                    IconURL = request.IconURL,
                    CreatedBy = userId,
                    LastModifiedBy = userId,
                    ParentCategoryID = request.ParentCategoryID,
                    IsDeleted = true,
                    Slug = request.Slug,

                };

                var createdAttributeValue = await _mediator.Send(command);
                return Created(userId, createdAttributeValue);
            }
            catch (Exception ex) {

                return BadRequest(ApiResponse<object>.ErrorResult($"Error creating attribute value: {ex.Message}"));
            }
            

        }
    }
}