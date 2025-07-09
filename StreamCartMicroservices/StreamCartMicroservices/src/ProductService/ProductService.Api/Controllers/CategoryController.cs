using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Commands.CategoryCommands;
using ProductService.Application.DTOs.Attributes;
using ProductService.Application.DTOs.Category;
using ProductService.Application.Queries.CategoryQueries;
using ProductService.Domain.Entities;
using Shared.Common.Models;
using Shared.Common.Services.User;
using System.Security.Claims;

namespace ProductService.Api.Controllers
{
    [Route("api/categorys")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;
        public CategoryController(IMediator mediator, ICurrentUserService currentUserService)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
        }
        [HttpPost]
        [Authorize(Roles ="OperationManager")]
        [ProducesResponseType(typeof(ApiResponse<Category>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCatgoryDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid attribute value data"));

            try
            {
                string? userId = _currentUserService.GetUserId().ToString();
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

                var createdCategory = await _mediator.Send(command);
                return Created(userId, createdCategory);
            }
            catch (Exception ex)
            {

                return BadRequest(ApiResponse<object>.ErrorResult($"Error creating attribute value: {ex.Message}"));
            }


        }
        [HttpGet]
        public async Task<IActionResult> GetCategories([FromQuery] GetAllCategoryQuery query)
        {
            var response = await _mediator.Send(query);
            if (response.Data == null) return NotFound(response);

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory([FromRoute] Guid id)
        {
            var query = new GetDetailCategoryQuery() { Id = id };
            var response = await _mediator.Send(query);
            if (response.Data == null) return NotFound(response);
            return Ok(response);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "OperationManager")]
        [ProducesResponseType(typeof(ApiResponse<Category>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> UpdateCategory([FromBody] UpdateCategoryDTO request, [FromRoute] Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid attribute value data"));

            try
            {
                string? userId = _currentUserService.GetUserId().ToString();
                var command = new UpdateCategoryCommand
                {
                    Id = id,
                    CategoryName = request.CategoryName,
                    Description = request.Description,
                    IconURL = request.IconURL,
                    LastModifiedBy = userId ?? "123",
                    ParentCategoryID = request.ParentCategoryID,
                    IsDeleted = true,
                    Slug = request.Slug,


                };

                var updateCategory = await _mediator.Send(command);
                return Ok(updateCategory);
            }
            catch (Exception ex)
            {

                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi cập nhật danh mục: {ex.Message}"));
            }


        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "OperationManager")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<bool>), 400)]
        public async Task<IActionResult> UpdateCategoryStatus([FromRoute] Guid id)
        {
            try
            {
                string? userId = _currentUserService.GetUserId().ToString();
                var command = new DeleteCategoryCommand()
                {
                    CategoryId = id,
                    Modifier = userId,
                };
                var deleteCategory = await _mediator.Send(command);
                return Ok(deleteCategory);

            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Lỗi cập nhật trạng thái danh mục: {ex.Message}"));
            }
        }
    }
}