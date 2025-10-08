using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.Models;
using UserManagementAPI.Services;

namespace UserManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    // GET: api/users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    // GET: api/users/5
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    // POST: api/users
    [HttpPost]
    public async Task<ActionResult<User>> CreateUser(User user)
    {
        if (string.IsNullOrWhiteSpace(user.Name))
        {
            return BadRequest("Name is required.");
        }

        var createdUser = await _userService.CreateUserAsync(user);
        return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, createdUser);
    }

    // PUT: api/users/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, User user)
    {
        if (string.IsNullOrWhiteSpace(user.Name))
        {
            return BadRequest("Name is required.");
        }

        var updatedUser = await _userService.UpdateUserAsync(id, user);
        if (updatedUser == null)
        {
            return NotFound();
        }

        return Ok(updatedUser);
    }

    // DELETE: api/users/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var result = await _userService.DeleteUserAsync(id);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
}