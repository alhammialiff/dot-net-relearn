public class MyService : IMyService
{
    private readonly int _serviceId;

    // =============================
    // Constructor
    // =============================
    public MyService()
    {
        _serviceId = new Random().Next(100000, 999999);
    }

    // =============================
    // Implementing Interface
    // =============================
    public void LogCreation(string message)
    {
        Console.WriteLine($"{message} - Service ID: {_serviceId}");
    }


}
