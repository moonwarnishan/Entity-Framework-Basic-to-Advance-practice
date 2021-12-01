using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


var factory = new CookBookContextFactory();
using var context = factory.CreateDbContext(args);


// Console.WriteLine("Add some data");
// var bf1 = new Dish {Title = "breakfast",Notes = "this is so good", Star = 5};
// var bf2 = new Dish { Title = "eakfast", Notes = "is so good", Star = 6 };
// var bf3 = new Dish { Title = "east", Notes = "isgood", Star = 7 };
// context.Dishes.Add(bf1);
// context.Dishes.Add(bf2);
// context.Dishes.Add(bf3);
//  await context.SaveChangesAsync();
// Console.WriteLine("added");
//Console.WriteLine("remove");
//context.Dishes.Remove(bf);
//await context.SaveChangesAsync();
//Console.WriteLine("removed");
//bf.Star = 6;
//await context.SaveChangesAsync();

var dis = context.Dishes
    .Where(d => d.Star == 6)
    .ToList();
foreach (var dish in dis)
{
    Console.WriteLine($"{dish.Title} {dish.Star}");
}



class Dish
{
    public int Id { get; set; }
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;


    //can be null so use ?
    [MaxLength(100)]
    public string? Notes { get; set; }
    public int? Star { get; set; }

    //a dish can have multiple dish ingredient
    public List<DishIngredient> Ingredients { get; set; } = new();
}

class DishIngredient
{
    public int Id { get; set; }
    [MaxLength(100)]
    public string Description { get; set; } = string.Empty;
    [MaxLength(50)]
    public string UnitOfMeasure { get; set; }=string.Empty;
    [Column(TypeName = "decimal(5,2)")]
    public decimal Amount { get; set; }
    
    //A ingredient can have Dish
    public Dish? Dish { get; set; }
    //forgien key
    public int DishId { get; set; }
}


class CookBookContext: DbContext
{
#pragma warning disable CS8618
    public CookBookContext(DbContextOptions<CookBookContext> options)
#pragma warning restore CS8618
        :base(options)
    {
        
    }

    public DbSet<Dish> Dishes { get; set; }
    public DbSet<DishIngredient> DishIngredients { get; set; }



}

class CookBookContextFactory:IDesignTimeDbContextFactory<CookBookContext>
{
    public CookBookContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        var optionsBuilder = new DbContextOptionsBuilder<CookBookContext>();
        optionsBuilder
            // Uncomment the following line if you want to print generated
            // SQL statements on the console.
            .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
            .UseSqlServer(configuration["ConnectionStrings:DefaultConnection"]);

        return new CookBookContext(optionsBuilder.Options);
    }
}