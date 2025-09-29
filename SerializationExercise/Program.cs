using System.Text.Json;
using System.Xml.Serialization;

var builder = WebApplication.CreateBuilder(args);

/*******************************************
* Global configuration of Serializer format
********************************************/
// builder.Services.ConfigureHttpJsonOptions(options =>
// {
//     // Globally configure Json format here (Kebabcase for clear example)
//     options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.KebabCaseUpper;
// });

/*****************************************************
* TypedResults.Json (Best Practice to send JSON over)
* Creates an HTTP response with a JSON body 
* and EXPLICITLY sets the Content-Type header to application/json
******************************************************/
var app = builder.Build();

var samplePerson = new Person { UserName = "Alice", UserAge = 30 };

app.MapGet("/", () => "I am Root!");


/*******************************************
* JsonSerializer.Serialize(samplePerson) 
* returns pascal case by default 
********************************************/
app.MapGet("/manual-json", () =>
{
    // This is how we serialize data in .NET
    var jsonString = JsonSerializer.Serialize(samplePerson);

    // Explicitly set type as "application/json"
    return TypedResults.Text(jsonString, "application/json");

});

/*******************************************
* JsonSerializer.Serialize(samplePerson, options) Returns 
* camelCase because we set it 
********************************************/
app.MapGet("/custom-serializer", () =>
{
    // Configures how we want our Json to be serialized
    var options = new JsonSerializerOptions
    {
        // Converts conventional PascalCase to camelCase
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    var customJsonString = JsonSerializer.Serialize(samplePerson, options);
    return TypedResults.Text(customJsonString, "application/json");

});

/******************************************* 
* returns camelCase by default but later
* in the course we globally set default to KEBAB-CASE 
********************************************/
app.MapGet("/json", () =>
{
    // Returns camelCase (camelCase is the default with TypedResults)
    return TypedResults.Json(samplePerson);

});

/*******************************************
* [SIMPLEST IMPLEMENTATION]
* Returns camelCase by default but later
* in the course we globally set default to KEBAB-CASE 
********************************************/
app.MapGet("/auto", () =>
{
    // Default implicit default return
    // If I'm ok with the PascalCase keys, doing this will auto-convert
    // object into JSON
    return samplePerson;
});

/*******************************************
* [LONGEST IMPLEMENTATION]
* XmlSerialize(samplePerson) returns XML
* XML is typically used in older systems 
********************************************/
app.MapGet("/xml", () =>
{
    // Serializing XML
    var xmlSerializer = new XmlSerializer(typeof(Person));
    var stringWriter = new StringWriter();
    xmlSerializer.Serialize(stringWriter, samplePerson);
    var xmlOutput = stringWriter.ToString();

    return TypedResults.Text(xmlOutput, "application/xml");

});

app.Run();

public class Person
{
    required public string UserName { get; set; }
    required public int UserAge { get; set; }
    // public Person(string UserName, int UserAge)
    // {
    //     UserName = UserName;
    //     UserAge = UserAge;
    // }

}