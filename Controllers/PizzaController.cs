using ContosoPizza.Data;
using ContosoPizza.Models;
using ContosoPizza.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContosoPizza.Controllers;

[ApiController]
[Route("[controller]")]
public class PizzaController : ControllerBase
{
    private readonly PizzaService _service;
    public PizzaController(PizzaDbContext context)
    {
        _service = new PizzaService(context);
    }

    // GET all action
    [HttpGet]
    public ActionResult<List<Pizza>> GetAll()
    {
        return _service.GetAll();
    }

    // GET by Id action
    [HttpGet("{id}")]
    public ActionResult<Pizza> Get(int id)
    {
        var pizza = _service.Get(id);

        if(pizza == null)
            return NotFound();

        return pizza;
    }

    // POST action
    [Authorize]
    [HttpPost]
    public IActionResult Create(Pizza pizza){
        _service.Add(pizza);
        return CreatedAtAction(nameof(Get), new{id = pizza.Id}, pizza);
    }

    // PUT action
    [Authorize]
    [HttpPut("{Id}")]
    public IActionResult Update(int Id, Pizza pizza){
        if(Id!=pizza.Id)
            return BadRequest();
        
        if(_service.Get(Id)==null)
            return NotFound();

        _service.Update(pizza);

        return NoContent();

    }

    // DELETE action
    [Authorize]
    [HttpDelete("{Id}")]
    public IActionResult Delete(int Id){
        if(_service.Get(Id)==null)
            return NotFound();

        _service.Delete(Id);

        return NoContent();
    }
}