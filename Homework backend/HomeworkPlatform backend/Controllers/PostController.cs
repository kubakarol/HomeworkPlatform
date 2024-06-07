﻿using HomeworkPlatform_backend.Models;
using HomeworkPlatform_backend.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace HomeworkPlatform_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly ILogger<PostController> _logger;

        public PostController(IPostService postService, ILogger<PostController> logger)
        {
            _postService = postService;
            _logger = logger;
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreatePost([FromBody] CreatePost model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state invalid: {ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            model.UserId = userId;

            _logger.LogInformation("Creating post with Title: {Title}, Content: {Content}, UserId: {UserId}", model.Title, model.Content, model.UserId);

            try
            {
                var post = await _postService.CreatePostAsync(model);
                if (post == null)
                {
                    return BadRequest("Post creation failed.");
                }
                return Ok(post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("addComment")]
        [Authorize]
        public async Task<IActionResult> AddComment([FromBody] AddComment model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            model.UserId = userId;

            _logger.LogInformation($"Adding comment with PostId: {model.PostId}, Content: {model.Content}, UserId: {model.UserId}");

            try
            {
                var comment = await _postService.AddCommentAsync(model);
                if (comment == null)
                {
                    return BadRequest("Adding comment failed.");
                }
                return Ok(comment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("getAll")]
        public async Task<ActionResult<List<Post>>> GetAllPosts()
        {
            try
            {
                var posts = await _postService.GetAllPostsAsync();
                return Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving posts.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}