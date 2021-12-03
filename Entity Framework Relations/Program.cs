using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

var context = new BrickContextFactory().CreateDbContext();
//await AddData();
await QueryData();
async Task AddData()
{
    Vendor bricking, helder;
    await context.AddRangeAsync(new[]{
        bricking=new Vendor(){VendorName = "Brick King"},
        helder=new Vendor(){VendorName = "Held er"}
    });
    await context.SaveChangesAsync();
    Tag rare, ninjago, minecraft;
    await context.AddRangeAsync(new[]
    {
        rare=new Tag(){Title = "Rare"},
        ninjago=new Tag(){Title ="Ninjago"},
        minecraft=new Tag(){Title ="MineCraft"}
    });
    await context.SaveChangesAsync();

    await context.AddAsync(new BasePlate()
        {
            Title = "Base plate 16*16 with blue",
            Tags = new (){ninjago,rare},
            Color = Color.Green,
            Length = 16,
            Width = 16,
            Availability = new ()
            {
                new (){ Vendor = bricking, AvailableAmount = 5,Price = 6.5m},
                new() { Vendor =helder , AvailableAmount = 15, Price = 7.5m }
            }
        }
    );
    await context.SaveChangesAsync();
}

async Task QueryData()
{
    //USE include for Join two table
    var BrickAvaibility = await context.BrickAvailabilities
        .Include(ba => ba.Vendor)
        .Include(b => b.Brick)
        .ToArrayAsync();
    foreach (var b in BrickAvaibility)
    {
        Console.WriteLine($"{b.Brick.Title}  {b.Price}  {b.Vendor.VendorName} {b.AvailableAmount}");
    }

    Console.WriteLine("");
    //Bricks with vendor and tags
    var BrickswithVendotandTags = await context.Bricks
        .Include(nameof(Brick.Availability)+'.'+nameof(BrickAvailability.Vendor))
        .Include(b => b.Tags)
        .ToArrayAsync();
    foreach (var  T in BrickswithVendotandTags)
    {
        Console.WriteLine($"{T.Title}");
        if (T.Tags.Any())Console.WriteLine($"{string.Join(',',T.Tags.Select(t=>t.Title))}");
        if (T.Availability.Any())
        {
            Console.WriteLine($"{string.Join(',',T.Availability.Select(t=>t.Vendor.VendorName))}");
        }
    }


    Console.WriteLine("");


}

#region Model
enum Color
{
    Black,
    White,
    Red,
    Yellow,
    Orange,
    Green
}


class Brick
{
    public int Id { get; set; }

    [MaxLength(250)]
    public string Title { get; set; } = string.Empty;
    public Color? Color { get; set; }
    public List<Tag> Tags { get; set; }
    public List<BrickAvailability> Availability { get; set; }
}

class BasePlate : Brick
{
    public int Length { get; set; }
    public int Width { get; set; }  
}

class MiniFigured : Brick
{
    public bool IsDualSided { get; set; }
}

class Tag
{
    public int  Id { get; set; }
    [MaxLength(250)]
    public string Title { get; set; } = string.Empty;

    public List<Brick> Bricks { get; set; }
}

class Vendor
{
    public int Id { get; set; }
    [MaxLength(150)]
    public string VendorName { get; set; }

    public List<BrickAvailability> Availability { get; set; } = new();
}

class BrickAvailability
{
    public int Id { get; set; }
    public Vendor Vendor { get; set; }
    public int VendorId { get; set; }
    public Brick Brick { get; set; }
    public int  BrickId { get; set; }
    public int AvailableAmount { get; set; }
    [Column(TypeName = "decimal(8,2)")]
    public decimal Price { get; set; }

}
#endregion

#region Datacontext

class BrickContext:DbContext
{
    public BrickContext(DbContextOptions<BrickContext> options)
        : base(options)
    {

    }

    public DbSet<Brick>? Bricks { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Vendor> Vendors { get; set; }
    public DbSet<BrickAvailability> BrickAvailabilities { get; set; }


    //for inheritance and derivative classes 
    //this is build in function OnModelCreating
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BasePlate>().HasBaseType<Brick>();
        modelBuilder.Entity<MiniFigured>().HasBaseType<Brick>();
    }
}


class BrickContextFactory : IDesignTimeDbContextFactory<BrickContext>
{
    public BrickContext CreateDbContext(string[]? args = null)
    {
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        var optionsBuilder = new DbContextOptionsBuilder<BrickContext>();
        optionsBuilder
            // Uncomment the following line if you want to print generated
            // SQL statements on the console.
            .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
            .UseSqlServer(configuration["ConnectionStrings:DefaultConnection"]);

        return new BrickContext(optionsBuilder.Options);
    }
}
#endregion