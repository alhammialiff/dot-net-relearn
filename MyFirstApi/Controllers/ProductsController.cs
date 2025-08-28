using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace MyFirstApi.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {

        [HttpGet] // HTTP GET HERE
        public ActionResult<List<string>> Get()
        {
            // Return 
            return new List<string>
            {
                "Apple",
                "Banana",
                "Orange"
            };

        }


        [HttpGet("featured")]
        public string GetFeaturedProduct() => "Mango";


        [HttpPost] // HTTP POST HERE
        public ActionResult<string> Post([FromBody] string newProduct)
        {

            return $"Added: {newProduct}";

        }


        [HttpPut("{id}")] // HTTP PUT HERE
        public ActionResult<string> Put(int id, [FromBody] string updatedProduct)
        {

            return $"Updated product {id} to: {updatedProduct}";

        }


        [HttpDelete("{id}")] // HTTP DELETE HERE
        public ActionResult<string> Delete(int id)
        {

            return $"Deleted product with ID: {id}";

        }

    }
    
}