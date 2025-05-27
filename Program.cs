using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog;
using EmployeeApp.Data;
using EmployeeApp.Models;
using EmployeeApp.Services;
using Bogus;
using EmployeeApp.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog Logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/EmployeeApp.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Configure Database Connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .LogTo(Console.WriteLine, LogLevel.Information)); // Log EF Core queries to the console

// Register Identity with ApplicationUser
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<EmployeeService>();

// Register Authentication & Authorization
builder.Services.AddAuthorization();
builder.Services.AddRazorPages();

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAuditLogging();
app.MapRazorPages();

//  Seed roles and generate random employees if table is empty
await SeedDataAsync(app);

Log.Information("EmployeeApp started successfully.");
app.Run();


//  SEEDING METHOD
async Task SeedDataAsync(IHost app)
{
    using var scope = app.Services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var employeeService = scope.ServiceProvider.GetRequiredService<EmployeeService>(); // Inject EmployeeService

    logger.LogInformation("Seeding roles...");
    string[] roles = { "Admin", "User" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
            logger.LogInformation($"Role '{role}' created.");
        }
    }

    var faker = new Faker("en");

    // Seed default admin user
    var adminEmail = "admin@example.com";
    var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
    if (existingAdmin == null)
    {
        var adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Admin",
            Department = "HR",
            EmployeeCode = "ADM001",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, "Admin@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            logger.LogInformation("Admin user created and added to role 'Admin'.");
        }
        else
        {
            logger.LogWarning("Failed to create default admin user:");
            foreach (var error in result.Errors)
                logger.LogWarning($"- {error.Description}");
        }
    }

    // Seed employees if none exist
    if (!await dbContext.Employees.AnyAsync())
    {
        logger.LogInformation("Seeding employees...");
        var departments = new[] { "Finance", "HR", "IT", "Marketing", "Sales" };

        var employees = new List<Employee>(); // Create a list to hold the employees to be added

        for (int i = 1; i <= 50; i++)
        {
            var fullName = faker.Name.FullName();
            var email = faker.Internet.Email().ToLower();
            var employeeCode = employeeService.GenerateEmployeeCode(fullName); // Use the injected service

            // Create Employee record
            var employee = new Employee
            {
                Name = fullName,
                Email = email,
                PhoneNumber = faker.Phone.PhoneNumber("9#########"),
                Department = faker.PickRandom(departments),
                Salary = faker.Random.Int(30000, 100000),
                EmployeeCode = employeeCode // Use generated employee code
            };

            employees.Add(employee); // Add the employee to the list
        }

        dbContext.Employees.AddRange(employees); // Add all employees to the database at once
        await dbContext.SaveChangesAsync(); // Save all employee records in one go

        // Create corresponding ApplicationUser and link to employee
        foreach (var employee in employees)
        {
            var employeeUser = new ApplicationUser
            {
                UserName = employee.Email,
                Email = employee.Email,
                FullName = employee.Name,
                Department = employee.Department,
                EmployeeId = employee.Id,
                EmployeeCode = employee.EmployeeCode,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(employeeUser, "User@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(employeeUser, "User");
                logger.LogInformation($"Employee user created: {employee.Email} and linked to EmployeeId {employee.Id}");
            }
        }
    }
    else
    {
        logger.LogInformation("Employees already exist, skipping seeding.");
    }
}

