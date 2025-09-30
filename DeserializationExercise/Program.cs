using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Serialization;


var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => "I am root!");

app.MapPost("/auto", (Person personFromClient) =>
{

    Console.WriteLine(personFromClient);
    personFromClient.UserName = "Tyler";

    return TypedResults.Ok(personFromClient);

});

/************************************
* Deserializing JSON with default config
************************************/
app.MapPost("/json", async (HttpContext context) =>
{
    // Deserialize it as Person object
    // Request is only 200 if data comes in as a Person object
    var person = await context.Request.ReadFromJsonAsync<Person>();
    return TypedResults.Json(person);

});


/************************************
* Deserializing JSON with custom option configs
************************************/
app.MapPost("/custom-options", async (HttpContext context) =>
{

    // Custom options allows us to refine requirement of JSON
    var options = new JsonSerializerOptions
    {

        // Eg. This will throw an error if JSON has extra keys
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    // Deserialize data as JSON and enforce type check (Person)
    var person = await context.Request.ReadFromJsonAsync<Person>(options);

    return TypedResults.Json(person);

});


/************************************
* Deserializing XML into JSON
************************************/
app.MapPost("/xml", async (HttpContext context) =>
{

    // Use StreamReader to read body
    var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();

    // Use XML Serializer that accepts Person object
    var xmlSerializer = new XmlSerializer(typeof(Person));

    // Use StringReader to read body
    var stringReader = new StringReader(body);

    // Deserialize XML into JSON
    var person = xmlSerializer.Deserialize(stringReader);

    // Return JSON as response
    return TypedResults.Ok(person);

});


// Run ASP.NET Server
app.Run();

public class Person
{

    required public string UserName { get; set; }
    public int? UserAge { get; set; }

}