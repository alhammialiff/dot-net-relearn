using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using System.Net;

public class Program
{

    public class User
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        /*****************************************************
        * Class method: So that every user instance has access to hashing of its data
        ******************************************************/
        public string GenerateHash()
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(ToString()));
                return Convert.ToBase64String(hashBytes);
            }

        }

        /*****************************************************
        * Class method: So that every user instance has access to encryption of its data
        ******************************************************/
        public void EncryptData()
        {
            // For this example, base64 conversion is to exemplify how we can pseudo-encrypt the password
            Password = Convert.ToBase64String(Encoding.UTF8.GetBytes(Password));
        }


    }

    public static User DeserializeUserData(string jsonData, bool isTrustedSource)
    {

        // A guarded clause to ensure it is from trusted source
        // Can be from a whitelist of sockets
        if (!isTrustedSource)
        {
            Console.WriteLine("Deserialization block: Untrusted source.");
            return null;
        }

        return JsonSerializer.Deserialize<User>(jsonData);
    }
    public static string SerializeUserData(User user)
    {

        // Validate: Guarded clause to check for empty inputs 
        if (
            string.IsNullOrWhiteSpace(user.Name) ||
            string.IsNullOrWhiteSpace(user.Email) ||
            string.IsNullOrWhiteSpace(user.Password)
        )
        {
            Console.WriteLine("Invalid data. Serialization aborted.");
            return string.Empty;
        }

        user.EncryptData();

        return JsonSerializer.Serialize(user);

    }

    public static void Main()
    {
        User user = new User
        {
            Name = "Alice",
            Email = "alice@example.com",
            Password = "SecureP@ss123"
        };

        string generatedHash = user.GenerateHash();
        string serializedData = SerializeUserData(user);
        User deserializedData = DeserializeUserData(serializedData, true);
        Console.WriteLine("Serialized data:\n " + generatedHash);

    }
}