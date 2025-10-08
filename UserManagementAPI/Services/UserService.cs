using UserManagementAPI.Models;

namespace UserManagementAPI.Services;

public class UserService : IUserService
{
    private readonly List<User> _users = new();
    private int _nextId = 1;

    public UserService()
    {
        // Add some sample data
        _users.AddRange(new[]
        {
            new User { Id = _nextId++, Name = "John Doe" },
            new User { Id = _nextId++, Name = "Jane Smith" },
            new User { Id = _nextId++, Name = "Bob Johnson" }
        });
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        await Task.Delay(1); // Simulate async operation
        return _users.ToList();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        await Task.Delay(1); // Simulate async operation
        return _users.FirstOrDefault(u => u.Id == id);
    }

    public async Task<User> CreateUserAsync(User user)
    {
        await Task.Delay(1); // Simulate async operation
        user.Id = _nextId++;
        _users.Add(user);
        return user;
    }

    public async Task<User?> UpdateUserAsync(int id, User user)
    {
        await Task.Delay(1); // Simulate async operation
        var existingUser = _users.FirstOrDefault(u => u.Id == id);
        if (existingUser == null)
            return null;

        existingUser.Name = user.Name;
        return existingUser;
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        await Task.Delay(1); // Simulate async operation
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user == null)
            return false;

        _users.Remove(user);
        return true;
    }
}