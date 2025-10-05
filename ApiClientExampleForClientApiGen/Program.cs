/********************************
* [Manual way] Generating API Client
* This is the traditional way, generating API through codes
*********************************/
// using System.Collections.Concurrent;

// Console.WriteLine("Hello, World!");

// var httpClient = new HttpClient();
// var apiBaseUrl = "http://localhost:5005";

// var httpResults = await httpClient.GetAsync($"{apiBaseUrl}/blogs");

// if (httpResults.StatusCode != System.Net.HttpStatusCode.OK) {
//     Console.WriteLine("Failed to fetch blogs.");
//     return;
// }

// var blogStream = await httpResults.Content.ReadAsStreamAsync();

// var options = new System.Text.Json.JsonSerializerOptions
// {
//     PropertyNameCaseInsensitive = true
// };

// var blogs = await System.Text.Json.JsonSerializer.DeserializeAsync<List<Blog>>(blogStream, options);

// if (blogs != null) {

//     foreach (var blog in blogs) {
//         Console.WriteLine($"{blog.Title}: {blog.Body}");
//     }
// }

// class Blog {
//     public required string Title { get; set; }
//     public required string Body { get; set; }
// }


/********************************
* [Better way] Generating API Client via NSwag
* We only need to run this code once to generate. To do this we need
* (1) ClientGenerator.cs - The config script to set up the API Client generation task
* (2) BlogApiClient.cs - The generated script to generate API Client
* [Warning] Do not that sometimes the generation is not accurate
*           In that case, regen again.
* What it achieves: 
* - This approach saves you a lot of time and effort and ensures 
*   that your client code is always up to date and stays in sync with your API
* - A more simplified way on code-level to perform API Request (similar to request.http)
*********************************/

// Run once to generate a Client API instance
// await new SwaggerClientGenerator().GenerateClient();

using BlogApi;

var httpClient = new HttpClient();
var apiBaseUrl = "http://localhost:5005";

var client = new BlogApiClient(apiBaseUrl, httpClient);

// 1. Get all blogs
var blogs = await client.BlogsAllAsync();
foreach (var blog in blogs)
{
    Console.WriteLine($"{blog.Title}: {blog.Body}");
}

// 2. Delete a blog
await client.BlogsDELETEAsync(0);

// 3. Add a new blog
var newBlog = new Blog
{
    Title = "New Blog via NSwag",
    Body = "Test Nswag"
};

await client.BlogsPOSTAsync(newBlog);


