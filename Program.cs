using Databasuppgift_2;
using Databasuppgift_2.Migrations;
using Databasuppgift_2.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;

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
        Console.WriteLine("Seeded db (Category)!");
    }
    // Seeding för Products
    if (!await db.Products.AnyAsync())
    {
        db.Products.AddRange(
            new Product { ProductName = "The Dark Knight", ProductDescription = "A movie", ProductPrice = 59, CategoryID = 2}
        );
        await db.SaveChangesAsync();
        Console.WriteLine("Seeded db (Product)!");
    }
    // Seeding för Author
    if (!await db.Authors.AnyAsync())
    {
        db.Authors.AddRange(
            new Author { Name = "J.K. Rowling", Country = "Scotland"},
            new Author { Name = "Grebert", Country= "Sweden"}
        );
        await db.SaveChangesAsync();
        Console.WriteLine("Seeded db (Author)!");
    }
    // Seeding för Book
    if (!await db.Books.AnyAsync())
    {
        db.Books.AddRange(
            new Book { Title = "Harry Potter", Year = 2001, AuthorID = 1},
            new Book { Title = "Barbapappa", Year = 1945, AuthorID = 2 },
            new Book { Title = "Bibeln", Year = 0, AuthorID = 2 }
        );
        await db.SaveChangesAsync();
        Console.WriteLine("Seeded db (Book)!");
    }
    // Seeding för members
    if (!await db.Members.AnyAsync())
    {
        db.Members.AddRange(
            new Member { Name = "Alice Andersson", Email = "alice.andersson@gmail.com" },
            new Member { Name = "Anders Persson", Email = "anders.persson@hotmail.com" }
        );
        await db.SaveChangesAsync();
        Console.WriteLine("Seeded db (Members)");
    }
    if (!await db.Loans.AnyAsync())
    {
        db.Loans.AddRange(
            new Loan
            {
                MemberID = 1,
                BookID = 1,
                LoanDate = DateTime.Today.AddDays(-3),
                DueDate = DateTime.Today.AddDays(9)
            },
            new Loan
            {
                MemberID = 2,
                BookID = 2,
                LoanDate = DateTime.Today.AddDays(-15),
                DueDate = DateTime.Today.AddDays(-1)
            }
        );
        await db.SaveChangesAsync();
        Console.WriteLine("Seeded db (Loans)");
    }
}

// CLI för CRUD; CREATE, READ, UPDATE, DELETE
while (true)
{
    Console.WriteLine("\nChoose entity to manage: categories | products | authors | books | loans | members | exit");
    var choice = Console.ReadLine()?.Trim() ?? string.Empty;
    // hoppa över tomma rader
    if (string.IsNullOrEmpty(choice))
    {
        continue;
    }

    if (choice.Equals("exit", StringComparison.OrdinalIgnoreCase))
    {
        break; // Avsluta programmet , hoppa ut ur loopen
    }

    while (true)
    {
        Console.WriteLine("\nChosen entity: " + choice);
        Console.WriteLine("Commands: list | add | delete <id> | edit <id> | ProductsByCategory | back");
        Console.WriteLine(">");
        var line = Console.ReadLine()?.Trim().ToLower() ?? string.Empty;
        // hoppa över tomma rader
        if (string.IsNullOrEmpty(line))
        {
            continue;
        }

        if (line.Equals("back", StringComparison.OrdinalIgnoreCase))
        {
            break; // Gå tillbaks till choice , hoppa ut ur loopen
        }

        // Delar upp raden på mellanslag: t.ex. "edit 2" --> ["edit", "2"]
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var cmd = parts[0].ToLowerInvariant();

        // Enkel switch för kommandotolkning
        switch (cmd)
        {
            case "list":
                // Lista vår (choice)
                await ListAsync(choice);
                break;
            case "add":
                // Lägga till en (choice)
                await AddAsync(choice);
                break;
            case "edit":
                // Redigera en (choice)
                // Kräver id efter kommandot "edit"
                if (parts.Length < 2 || !int.TryParse(parts[1], out var id))
                {
                    Console.WriteLine("Usage: Edit <id>");
                    break;
                }
                await EditAsync(choice, id);
                break;
            case "delete":
                // Radera en (choice)
                if (parts.Length < 2 || !int.TryParse(parts[1], out var idD))
                {
                    Console.WriteLine("Usage: Delete <id>");
                    break;
                }
                await DeleteAsync(choice, idD);
                break;
            case "productsbycategory":
                if (parts.Length < 2 || !int.TryParse(parts[1], out var idC))
                {
                    Console.WriteLine("Usage: ProductsByCategory <id>");
                    break;
                }
                await ProductsByCategoryAsync(idC);
                break;
            case "productsbyprice":
                await ProductsByPriceAsync();
                break;
            case "listlateloans":
                await ListLateLoansAsync();
                break;
            default:
                Console.Write("Unknown command: ");
                break;
        }
    }
}

// CREATE: Lägg till en ny kategory
static async Task AddAsync(string choice)
{
    using var db = new ShopContext();
    switch (choice)
    {
        case "categories":
            Console.WriteLine("Name: ");
            var name = Console.ReadLine()?.Trim() ?? string.Empty;

            // Enkel valideringssteg
            if (string.IsNullOrEmpty(name) || name.Length > 100)
            {
                Console.WriteLine("Name is required (max 100).");
                return;
            }
            Console.WriteLine("Description (Optional): ");
            var desc = Console.ReadLine()?.Trim() ?? string.Empty;

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
            break;

        // Om CRUD nås med choice products
        case "products":
            // Name
            Console.WriteLine("Name: ");
            var pName = Console.ReadLine()?.Trim() ?? string.Empty;

            // Enkel valideringssteg
            if (string.IsNullOrEmpty(pName) || pName.Length > 100)
            {
                Console.WriteLine("Name is required (max 100).");
                return;
            }

            // Description
            Console.WriteLine("Description (Optional): ");
            var pDesc = Console.ReadLine()?.Trim() ?? string.Empty;

            // Price
            Console.WriteLine("Price: ");
            var stringPrice = Console.ReadLine()?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(stringPrice) || !int.TryParse(stringPrice, out var pPrice))
            {
                Console.WriteLine("Price is required, and has to be a number");
                return;
            }

            // Category
            Console.WriteLine("Set category. Available Categories: ");
            foreach (var category in db.Categories)
            {
                Console.WriteLine(category.CategoryID + ". " + category.CategoryName);
            }
            var stringID = Console.ReadLine()?.Trim() ?? string.Empty;
            if (!int.TryParse(stringID, out var pID))
            {
                Console.WriteLine("Choose category using its ID");
                return;
            }

            db.Products.Add(new Product { ProductPrice = pPrice, ProductDescription = pDesc, ProductName = pName, CategoryID = pID });
            //pCategory.CategoryProducts.Add(new Product { ProductPrice = pPrice, ProductDescription = pDesc, ProductName = pName, CategoryID = pID});
            try
            {
                // Spara våra ändringar; Trigga en INSERT + all validering/constraints i databasen
                await db.SaveChangesAsync();
                Console.WriteLine("Product added");
            }
            catch (DbUpdateException exception)
            {
                // Hit kommer vi t.ex. om UNIQUE-indexet på CategoryName bryts
                Console.WriteLine("DB Error (Maybe duplicate?)" + exception.GetBaseException().Message);
            }
            break;
        case "authors":
            // Author name
            Console.WriteLine("Name: ");
            var aName = Console.ReadLine()?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(aName) || aName.Length > 100)
            {
                Console.WriteLine("Name is required (max 100).");
                return;
            }

            // Author country
            Console.WriteLine("Country: ");
            var aCountry = Console.ReadLine()?.Trim() ?? string.Empty;

            // Author's books (add several, divide user input by comma, optional
            Console.WriteLine("Add books by ID to author: (Optional)\nUsage: Enter BookID, divide each entry by comma ','");
            await ListAsync("books");
            var input = Console.ReadLine()?.Trim() ?? string.Empty;
            if (input != string.Empty)
            {
                string[] parts = input.Split(',', StringSplitOptions.RemoveEmptyEntries);

                List<int> bookIds = new List<int>();
                foreach (var part in parts)
                {
                    if (int.TryParse(part.Trim(), out int id))
                    {
                        bookIds.Add(id);
                    }
                    else
                    {
                        Console.WriteLine("Invalid parse for " + part);
                    }
                }
                var setBooks = db.Books.Where(x => bookIds.Contains(x.BookID)).ToList();

                db.Authors.Add(new Author { Name = aName, Country = aCountry, Books = setBooks });
            }
            // If user input is none, don't add any books to author
            else
            {
                db.Authors.Add(new Author { Name = aName, Country = aCountry });
            }

            try
            {
                // Spara våra ändringar; Trigga en INSERT + all validering/constraints i databasen
                await db.SaveChangesAsync();
                Console.WriteLine("Author added");
            }
            catch (DbUpdateException exception)
            {
                // Hit kommer vi t.ex. om UNIQUE-indexet på AuthorNames bryts
                Console.WriteLine("DB Error (Maybe duplicate?)" + exception.GetBaseException().Message);
            }
            break;
        case "books":
            // Title
            Console.WriteLine("Title: ");
            var bTitle = Console.ReadLine()?.Trim() ?? string.Empty;

            // Enkel valideringssteg
            if (string.IsNullOrEmpty(bTitle) || bTitle.Length > 100)
            {
                Console.WriteLine("Title is required (max 100).");
                return;
            }

            // Year
            Console.WriteLine("Year: ");
            var stringYear = Console.ReadLine()?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(stringYear) || !int.TryParse(stringYear, out var bYear))
            {
                Console.WriteLine("Year is required, and has to be a number");
                return;
            }

            // Author
            Console.WriteLine("Set an author by ID");
            await ListAsync("authors");
            var aIDstring = Console.ReadLine()?.Trim() ?? string.Empty;

            if (!int.TryParse(aIDstring, out var aID) || string.IsNullOrEmpty(aIDstring))
            {
                Console.WriteLine("Choose author using its ID");
                return;
            }

            db.Books.Add(new Book { Title = bTitle, Year = bYear, AuthorID = aID });
            // TODO: Change this to refer to books and uncomment
            //pCategory.CategoryProducts.Add(new Product { ProductPrice = pPrice, ProductDescription = pDesc, ProductName = pName, CategoryID = pID});

            try
            {
                // Spara våra ändringar; Trigga en INSERT + all validering/constraints i databasen
                await db.SaveChangesAsync();
                Console.WriteLine("Book added");
            }
            catch (DbUpdateException exception)
            {
                // Hit kommer vi t.ex. om UNIQUE-indexet på CategoryName bryts
                Console.WriteLine("DB Error (Maybe duplicate?)" + exception.GetBaseException().Message);
            }
            break;
    }
    
}

static async Task DeleteAsync(string choice, int id)
{
    using var db = new ShopContext();
    switch (choice)
    {
        case "categories":
            var category = await db.Categories.FirstOrDefaultAsync(c => c.CategoryID == id);
            if (category == null)
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
            break;

        case "products":
            var product = await db.Products.FirstOrDefaultAsync(p => p.ProductID == id);
            if (product == null)
            {
                Console.WriteLine("Product not found");
                return;
            }
            db.Products.Remove(product);

            try
            {
                await db.SaveChangesAsync();
                Console.WriteLine("Deleted product");
            }
            catch (DbUpdateException exception)
            {
                Console.WriteLine(exception.Message);
            }

            break;
    }
    
}

static async Task EditAsync(string choice, int id)
{
    using var db = new ShopContext();

    switch (choice)
    {
        case "categories":
            // Hämta raden vi vill uppdatera
            var category = await db.Categories.FirstOrDefaultAsync(x => x.CategoryID == id);
            if (category == null)
            {
                Console.WriteLine("Category not found");
                return;
            }

            // Visar nuvarande värden: Uppdatera namn för en specifik category
            Console.WriteLine($"{category.CategoryName} ");
            var categoryName = Console.ReadLine()?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(categoryName))
            {
                category.CategoryName = categoryName;
            }

            // Uppdatera description för en specifik category;      TODO: FIX ME
            Console.Write($"{category.CategoryDescription} ");
            var desc = Console.ReadLine()?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(desc))
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
            break;
        case "products":
            // Hämta raden vi vill uppdatera
            var product = await db.Products
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.ProductID == id);
            if (product == null)
            {
                Console.WriteLine("Product not found");
                return;
            }

            // Visar nuvarande värden: Uppdatera namn för en specifik product
            Console.WriteLine($"{product.ProductName} ");
            var productName = Console.ReadLine()?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(productName))
            {
                product.ProductName = productName;
            }

            // Uppdatera description för en specifik product;      TODO: FIX ME
            Console.Write($"{product.ProductDescription} ");
            var productDesc = Console.ReadLine()?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(productDesc))
            {
                product.ProductDescription = productDesc;
            }

            //Uppdatera price för en specifik product
            Console.WriteLine(product.ProductPrice + " ");
            var stringPrice = Console.ReadLine()?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(stringPrice) && int.TryParse(stringPrice, out var productPrice))
            {
                product.ProductPrice = productPrice;
            }

            // Uppdatera kategorin
            Console.WriteLine(product.CategoryID + ". " + product.Category?.CategoryName + " ");
            var productCategoryIDstring = Console.ReadLine()?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(productCategoryIDstring) || int.TryParse(productCategoryIDstring, out var parse) ) 
            {
                int.TryParse(productCategoryIDstring, out var productCategoryID);
                product.CategoryID = productCategoryID;
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
            break;
    }

}

// READ: Lista alla kategorier
static async Task ListAsync(string choice)
{
    using var db = new ShopContext();

    switch (choice)
    {
        case "categories":
            // AsNoTracking = snabbare för read-only scenarion. (Ingen change tracking)
            var categoryRows = await db.Categories.AsNoTracking().OrderBy(category => category.CategoryID).ToListAsync();
            Console.WriteLine("Id | Name | Description | Antal produkter");
            foreach (var row in categoryRows)
            {
                Console.WriteLine($"{row.CategoryID} | {row.CategoryName} | {row.CategoryDescription} | {row.CategoryProducts.Count}");
            }
            break;
        case "products":
            // AsNoTracking = snabbare för read-only scenarion. (Ingen change tracking)
            var productRows = await db.Products
                            .AsNoTracking()
                            .Include(p => p.Category)
                            .OrderBy(Product => Product.ProductID)
                            .ToListAsync();
            Console.WriteLine("Id | Price | Description | Name | Category ");
            foreach (var row in productRows)
            {
                Console.WriteLine($"{row.ProductID} | {row.ProductPrice} | {row.ProductDescription} | {row.ProductName} | { row.Category?.CategoryName }");
            }
            break;
        case "authors":
            // AsNoTracking = snabbare för read-only scenarion. (Ingen change tracking)
            var authorRows = await db.Authors
                            .AsNoTracking()
                            .Include(p => p.Books)
                            .OrderBy(Author => Author.AuthorID)
                            .ToListAsync();
            Console.WriteLine("Id | Name | Country | Total Books ");
            foreach (var row in authorRows)
            {
                Console.WriteLine($"{row.AuthorID} | {row.Name} | {row.Country} | {row.Books.Count}");
            }
            break;
        case "books":
            // AsNoTracking = snabbare för read-only scenarion. (Ingen change tracking)
            var bookRows = await db.Books
                            .AsNoTracking()
                            .Include(p => p.Author)
                            .OrderBy(Book => Book.BookID)
                            .ToListAsync();
            Console.WriteLine("Id | Title | Year | Author ");
            foreach (var row in bookRows)
            {
                Console.WriteLine($"{row.BookID} | {row.Title} | {row.Year} | {row.Author?.Name} ");
            }
            break;
        case "loans":
            var loanRows = await db.Loans
                            .AsNoTracking()
                            .Include(p => p.Member)
                            .Include(p => p.Book)
                            .OrderBy(l => l.DueDate)
                            .ToListAsync();
            Console.WriteLine("ID | Loan Date | Due Date | Loaner | Book | Late?");
            foreach (var row in loanRows)
            {
                var isLate = row.DueDate < DateTime.Today;
                Console.WriteLine($"{row.LoanID} | {row.LoanDate:d} | {row.DueDate:d} | {row.Member?.Name} | {row.Book?.Title} | {isLate}");
            }
            break;
    }

}

// Listar alla lån som är försenade
static async Task ListLateLoansAsync()
{
    // hämta context
    using var db = new ShopContext();
    // Sätt alla lån i variabel med LINQ bestämmelser, ta ut alla som är försenade
    var loanRows = await db.Loans
                            .Include(p => p.Member)
                            .Include(p => p.Book)
                            .AsNoTracking()
                            .Where(x => x.DueDate < DateTime.Today)
                            .OrderBy(l => l.DueDate)
                            .ToListAsync();

    if (loanRows.Count == 0)
    {
        Console.WriteLine("No late loans found");
        return;
    }
    // Initial tabellutskrift
    Console.WriteLine("ID | Loan Date | Due Date | Loaner | Book ");

    // För varje rad, skriv ut
    foreach (var row in loanRows)
    {
        var isLate = row.DueDate < DateTime.Today;
            Console.WriteLine($"{row.LoanID} | {row.LoanDate:d} | {row.DueDate:d} | {row.Member?.Name} | {row.Book?.Title}");
    }
}

static async Task ProductsByCategoryAsync(int CategoryID)
{
    using var db = new ShopContext();

    var productRows = await db.Products
                            .AsNoTracking()
                            .Include(p => p.Category)
                            .OrderBy(Product => Product.ProductID)
                            .ToListAsync();
    Console.WriteLine("Id | Price | Description | Name | Category ");
    foreach (var row in productRows)
    {
        if (row.CategoryID == CategoryID)
        {
            Console.WriteLine($"{row.ProductID} | {row.ProductPrice} | {row.ProductDescription} | {row.ProductName} | {row.Category?.CategoryName}");
        }
    }
}

static async Task ProductsByPriceAsync()
{
    using var db = new ShopContext();

    var productRows = await db.Products
                            .AsNoTracking()
                            .Include(p => p.Category)
                            .OrderBy(Product => Product.ProductPrice)
                            .ToListAsync();
    Console.WriteLine("Id | Price | Description | Name | Category ");
    foreach (var row in productRows)
    { 
        Console.WriteLine($"{row.ProductID} | {row.ProductPrice} | {row.ProductDescription} | {row.ProductName} | {row.Category?.CategoryName}");
    }
}