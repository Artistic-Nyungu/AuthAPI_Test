using ContosoPizza.Data;
using ContosoPizza.Models;

namespace ContosoPizza.Services;

public  class PizzaService{
    private  PizzaDbContext _context;
    public  PizzaService(PizzaDbContext context)
    {
        _context = context;
    }

    public  List<Pizza> GetAll() => _context.Pizzas.ToList();
    public  Pizza? Get(int id)=>_context.Pizzas.FirstOrDefault(p => p.Id == id);
    public  int Add(Pizza pizza)
    {

        if(_context.Pizzas.Any(p => p.Name == pizza.Name))
            return -3;

        _context.Add(pizza);

        try{
            _context.SaveChanges();
        }
        catch(Exception ex)
        {
            var og = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(ex.InnerException?.Message ?? ex.Message);
            Console.ForegroundColor = og;
            return -1;
        }

        var last = _context.Pizzas.Last();

        return last.Name == pizza.Name ? last.Id : -2;
    }

    public bool Delete(int id)
    {
        var pizza = _context.Pizzas.Find(id);

        if(pizza == null)
        {
            var og = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"Pizza with ID:{id} does not exist. Deletion failed.");
            Console.ForegroundColor = og;
            return false;
        }

        _context.Remove(pizza);

        try{
            _context.SaveChanges();
        }
        catch(Exception ex)
        {
            var og = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(ex.InnerException?.Message ?? ex.Message);
            Console.ForegroundColor = og;
            return false;
        }

        return true;
    }

    public  bool Update(Pizza pizza)
    {
        var ogPizza = _context.Pizzas.Find(pizza.Id);

        if(ogPizza == null)
        {
            var og = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"Pizza with ID:{pizza.Id} does not exist. Update failed");
            Console.ForegroundColor = og;
            return false;
        }

        ogPizza.IsGlutenFree = pizza.IsGlutenFree;
        ogPizza.Name = pizza.Name;

        try{
            _context.SaveChanges();
        }
        catch(Exception ex)
        {
            var og = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(ex.InnerException?.Message ?? ex.Message);
            Console.ForegroundColor = og;
            return false;
        }

        return true;
    }
}