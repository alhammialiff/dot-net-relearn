using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

[Route("api/products")]
[ApiController]
public class ProductController : ControllerBase
{
    
    private static List<Product> products = new List<Product>();


    // ======================================
    // GET ALL: Read all existing products
    // ======================================
    [HttpGet]
    public ActionResult<List<Product>> GetAll() => products;

    // ======================================
    // GET ALL: Read a specific existing products
    // ======================================
    [HttpGet("{id}")]
    public ActionResult<Product> GetById(int id)
    {
        // Find Product by FirstOrDefault (like JS filter)
        var product = products.FirstOrDefault(p => p.Id == id);
        return product != null ? Ok(product) : NotFound();
    }

    // ======================================
    // CREATE: Create a new product and add it into the product list
    // ======================================
    [HttpPost]
    public ActionResult<Product> Create(Product newProduct)
    {
        newProduct.Id = products.Count + 1;
        products.Add(newProduct);
        return CreatedAtAction(nameof(GetById), new { id = newProduct.Id }, newProduct);
    }

    // ======================================
    // UPDATE: Update a produce
    // ======================================
    [HttpPut("{id}")]
    public ActionResult Update(int id, Product updatedProduct)
    {
        var product = products.FirstOrDefault(p => p.Id == id);
        if (product == null) return NotFound();

        // Update existing product
        product.Name = updatedProduct.Name;
        product.Description = updatedProduct.Description;
        product.Price = updatedProduct.Price;
        return Ok(product);
    }

    // ======================================
    // DELETE: Remove a product
    // ======================================
    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        var product = products.FirstOrDefault(p => p.Id == id);
        if (product == null) return NotFound();

        // .NET has a very simple way or removing items from a list. Cool.
        products.Remove(product);
        return NoContent();
    }


}
