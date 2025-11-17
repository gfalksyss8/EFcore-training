using Databasuppgift_2;
using Databasuppgift_2.Models;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("DB: " + Path.Combine(AppContext.BaseDirectory, "shop.db"));
// Säkerställ DB + migrations + Seed
using (var db = new ShopContext())
{
    // Migrate Async: Skapar databasen om den inte finns
    // Kör bara om det inte finns några kategorier sen innan
    await db.Database.MigrateAsync();


    // Enkel seeding för databasen
    // Kör bara om det inte finns några kategorier sen innan
    if (!await db.Categories.AnyAsync())
    {
        db.Categories.AddRange(
            new Category { CategoryName = "Books", CategoryDescription = "All books we have." },
            new Category { CategoryName = "Movies", CategoryDescription = "All movies we have." }
            );
        await db.SaveChangesAsync();
        Console.WriteLine("Seeded db!");
    }
}

// CLI för CRUD; CREATE, READ, UPDATE, DELETE
while(true)
{
    Console.WriteLine("\nCommands: list | add | delete <id> | edit <id> | exit");
    Console.WriteLine(">");
    var line = Console.ReadLine()?.Trim() ?? string.Empty;
    // hoppa över tomma rader
    if(string.IsNullOrEmpty(line))
    {
        continue;
    }

    if (line.Equals("exit", StringComparison.OrdinalIgnoreCase))
    {
        break; // Avsluta programmet , hoppa ut ur loopen
    }

    // Delar upp raden på mellanslag: t.ex. "edit 2" --> ["edit", "2"]
    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    var cmd = parts[0].ToLowerInvariant();

    // Enkel switch för kommandotolkning
    switch (cmd)
    {
        case "list":
            // Lista vår categories
            await ListAsync();
            break;
        case "add":
            // Lägga till en category
            await AddAsync();
            break;
        case "edit":
            // Redigera en category
            // Kräver id efter kommandot "edit"
            if (parts.Length < 2 || !int.TryParse(parts[1], out var id ))
            {
                Console.WriteLine("Usage: Edit <id>");
                break;
            }
            await EditAsync(id);
            break;
        case "delete":
            // Radera en category
            if (parts.Length < 2 || !int.TryParse(parts[1], out var idD))
            {
                Console.WriteLine("Usage: Delete <id>");
                break;
            }
            await DeleteAsync(idD);
            break;
        default:
            Console.Write("Unknown command: ");
            break;
    }
}

// CREATE: Lägg till en ny kategory
static async Task AddAsync()
{
    Console.WriteLine("Name: ");
    var name = Console.ReadLine()?.Trim() ?? string.Empty;

    // Enkel valideringssteg
    if(string.IsNullOrEmpty(name) || name.Length > 100)
    {
        Console.WriteLine("Name is required (max 100).");
        return;
    }
    Console.WriteLine("Descrption (Optional): ");
    var desc = Console.ReadLine()?.Trim() ?? string.Empty;

    using var db = new ShopContext();
    db.Categories.Add(new Category { CategoryName = name, CategoryDescription = desc });
    try
    {
        // Spara våra ändringar; Trigga en INSERT + all validering/constraints i databasen
        await db.SaveChangesAsync();
        Console.WriteLine("Category added");
    }
    catch (DbUpdateException exception)
    {
        // Hit kommer vi t.ex. om UNIQUE-indexet på CategoryName bryts
        Console.WriteLine("DB Error (Maybe duplicate?)" + exception.GetBaseException().Message);
    }
}

static async Task DeleteAsync(int id)
{
    using var db = new ShopContext();
    var category = await db.Categories.FirstOrDefaultAsync(c => c.CategoryID == id);
    if(category == null)
    {
        Console.WriteLine("Category not found");
        return;
    }
    db.Categories.Remove(category);

    try
    {
        await db.SaveChangesAsync();
        Console.WriteLine("Deleted");
    }
    catch (DbUpdateException exception)
    {
        Console.WriteLine(exception.Message);
    }
}

static async Task EditAsync(int id)
{
    using var db = new ShopContext();

    // Hämta raden vi vill uppdatera
    var category = await db.Categories.FirstOrDefaultAsync(x => x.CategoryID == id);
    if(category == null)
    {
        Console.WriteLine("Category not found");
        return;
    }

    // Visar nuvarande värden: Uppdatera namn för en specifik category
    Console.WriteLine($"{category.CategoryName} ");
    var name = Console.ReadLine()?.Trim() ?? string.Empty;
    if(!string.IsNullOrEmpty(name))
    {
        category.CategoryName = name;
    }

    // Uppdatera description för en specifik category;      TODO: FIX ME
    Console.Write($"{category.CategoryDescription} ");
    var desc = Console.ReadLine()?.Trim() ?? string.Empty;
    if(!string.IsNullOrEmpty(desc))
    {
        category.CategoryDescription = desc;
    }

    // Uppdatera db med ändringar
    try
    {
        await db.SaveChangesAsync();
        Console.WriteLine("Done");
    }
    catch (DbUpdateException exception)
    {
        Console.WriteLine(exception.Message);
    }
}

// READ: Lista alla kategorier
static async Task ListAsync()
{
    using var db = new ShopContext();

    // AsNoTracking = snabbare för read-only scenarion. (Ingen change tracking)
    var rows = await db.Categories.AsNoTracking().OrderBy(category => category.CategoryID).ToListAsync();
    Console.WriteLine("Id | Name | Descrption");
    foreach (var row in rows)
    {
        Console.WriteLine($"{row.CategoryID} | {row.CategoryName} | {row.CategoryDescription}");
    }
}