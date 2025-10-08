using Microsoft.EntityFrameworkCore;
using UserManagementAPI.Data;
using UserManagementAPI.Models;

namespace UserManagementAPI.Services;

public class UserDbService : IUserService
{
    private readonly UserDbContext _context;

    public UserDbService(UserDbContext context)
    {
        _context = context;
    }

    // ✅ HERE'S WHY ASYNC IS ESSENTIAL - Database I/O operations!
    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        // 🔥 REAL async database call - this would BLOCK without async!
        return await _context.Users.ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        // 🔥 REAL async database call - this would BLOCK without async!
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User> CreateUserAsync(User user)
    {
        // Add to context (in memory)
        _context.Users.Add(user);
        
        // 🔥 REAL async database I/O - saves to database!
        await _context.SaveChangesAsync();
        
        return user;
    }

    public async Task<User?> UpdateUserAsync(int id, User user)
    {
        // 🔥 REAL async database call to find the user
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (existingUser == null)
            return null;

        // Update properties
        existingUser.Name = user.Name;
        
        // 🔥 REAL async database I/O - saves changes to database!
        await _context.SaveChangesAsync();
        
        return existingUser;
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        // 🔥 REAL async database call to find the user
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
            return false;

        // Remove from context
        _context.Users.Remove(user);
        
        // 🔥 REAL async database I/O - saves changes to database!
        await _context.SaveChangesAsync();
        
        return true;
    }
}