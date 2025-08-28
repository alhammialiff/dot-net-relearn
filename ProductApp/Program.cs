using Newtonsoft.Json;

public class Product
{

    public string Name { get; set; }

    public decimal Price { get; set; }

    public List<string> Tags { get; set; }


}

public class Program
{

    public static void Main()
    {

        string json = "{\"Name\": \"Laptop\", \"Price\": 999.99, \"Tags\": [\"Electronics\",\"Computers\"]}";

        // What this does is it helps us destructure our json object and use it in our Product object 
        // (all in a line of code)
        Product product = JsonConvert.DeserializeObject<Product>(json);



    }

}
