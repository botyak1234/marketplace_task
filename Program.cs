using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using TaskMarketplace.API.Data;
using TaskMarketplace.API.Services;
using TaskMarketplace.API.Models;
using System.Security.Claims;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
        };
    });

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthorization();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

var auth = app.MapGroup("auth");

auth.MapPost("/register", async (
    User userInput,
    ApplicationDbContext db) =>
{

    if (await db.Users.AnyAsync(u => u.Username == userInput.Username))
        return Results.BadRequest("Username already taken");

    var user = new User
    {
        Username = userInput.Username,
        PasswordHash = HashPassword(userInput.PasswordHash),
        Role = "User",
        Points = 0
    };

    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Ok("Registered");
});


auth.MapPost("/login", async (
    User loginInput,
    ApplicationDbContext db,
    TokenService tokenService) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == loginInput.Username);
    if (user == null || !VerifyPassword(loginInput.PasswordHash, user.PasswordHash))
        return Results.Unauthorized();

    var token = tokenService.CreateToken(user);
    return Results.Ok(new { token });
});

var me = app.MapGroup("/me").RequireAuthorization();

me.MapGet("/points", async (
    ClaimsPrincipal userClaims,
    ApplicationDbContext db) =>
{
    var userId = int.Parse(userClaims.FindFirst("id")!.Value);
    var user = await db.Users.FindAsync(userId);
    return user is null
        ? Results.NotFound("User not found")
        : Results.Ok(new { points = user.Points });
});


var tasks = app.MapGroup("/tasks")
    .RequireAuthorization();

tasks.MapGet("/", async (ApplicationDbContext db
    ,ClaimsPrincipal userClaims) =>
    await db.Tasks.Include(t => t.TakenByUser).ToListAsync()
);

tasks.MapGet("/{id}", async (int id,
    ClaimsPrincipal userClaims,
    ApplicationDbContext db) =>
    await db.Tasks.Include(t => t.TakenByUser).FirstOrDefaultAsync(t => t.Id == id)
        is TaskItem task
            ? Results.Ok(task)
            : Results.NotFound()
);

tasks.MapGet("/by-status", async (
    string status,
    ApplicationDbContext db) =>
{
    // Проверка, что переданный статус — корректный элемент enum-а
    if (!Enum.TryParse<TaskMarketplace.API.Models.TaskStatus>(status, ignoreCase: true, out var parsedStatus))
    {
        return Results.BadRequest($"Invalid status. Allowed values: {string.Join(", ", Enum.GetNames(typeof(TaskMarketplace.API.Models.TaskStatus)))}");
    }

    var tasksList = await db.Tasks
        .Include(t => t.TakenByUser)
        .Where(t => t.Status == parsedStatus)
        .ToListAsync();

    return Results.Ok(tasksList);
});

tasks.MapGet("/sorted", async (
    string? sortBy,
    string? order,
    ApplicationDbContext db) =>
{
    var query = db.Tasks.Include(t => t.TakenByUser).AsQueryable();

    bool desc = string.Equals(order, "desc", StringComparison.OrdinalIgnoreCase);

    query = sortBy?.ToLower() switch
    {
        "created"   => desc ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt),
        "updated"   => desc ? query.OrderByDescending(t => t.UpdatedAt) : query.OrderBy(t => t.UpdatedAt),
        _           => query.OrderBy(t => t.Id)
    };

    var tasksList = await query.ToListAsync();
    return Results.Ok(tasksList);
});




tasks.MapPost("/", async (TaskItem task, ApplicationDbContext db) =>
{
    task.CreatedAt = DateTime.UtcNow;
    task.UpdatedAt = DateTime.UtcNow;
    db.Tasks.Add(task);
    await db.SaveChangesAsync();
    return Results.Created($"/tasks/{task.Id}", task);
}).RequireAuthorization("AdminOnly");


tasks.MapPost("/{id}/take", async (
    int id,
    ClaimsPrincipal userClaims,
    ApplicationDbContext db) =>
{
    var userId = int.Parse(userClaims.FindFirst("id")!.Value);

    var task = await db.Tasks.FindAsync(id);
    if (task is null) return Results.NotFound("Task not found");
    if (task.TakenByUserId != null) return Results.BadRequest("Task already taken");
    if (task.Status != TaskMarketplace.API.Models.TaskStatus.New) return Results.BadRequest("Task cannot be taken");

    task.TakenByUserId = userId;
    task.Status = TaskMarketplace.API.Models.TaskStatus.Taken;
    task.UpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();
    return Results.Ok(task);
});

tasks.MapPost("/{id}/submit", async (
    int id,
    ClaimsPrincipal userClaims,
    ApplicationDbContext db) =>
{
    var userId = int.Parse(userClaims.FindFirst("id")!.Value);

    var task = await db.Tasks.FindAsync(id);
    if (task is null) return Results.NotFound("Task not found");
    if (task.TakenByUserId != userId) return Results.Forbid();
    if (task.Status != TaskMarketplace.API.Models.TaskStatus.Taken) return Results.BadRequest("Task must be in 'Taken' status");

    task.Status = TaskMarketplace.API.Models.TaskStatus.Submitted;
    task.UpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();
    return Results.Ok(task);
});


tasks.MapPut("/{id}", async (int id, TaskItem updatedTask, ApplicationDbContext db) =>
{
    var task = await db.Tasks.FindAsync(id);
    if (task is null) return Results.NotFound();

    task.Title = updatedTask.Title;
    task.Description = updatedTask.Description;
    task.Reward = updatedTask.Reward;
    task.Status = updatedTask.Status;
    task.TakenByUserId = updatedTask.TakenByUserId;
    task.UpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();
    return Results.Ok(task);
}).RequireAuthorization("AdminOnly");

tasks.MapDelete("/{id}", async (int id, ApplicationDbContext db) =>
{
    var task = await db.Tasks.FindAsync(id);
    if (task is null) return Results.NotFound();

    db.Tasks.Remove(task);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization("AdminOnly");







string HashPassword(string password)
{
    using var sha = SHA256.Create();
    var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
    return Convert.ToBase64String(bytes);
}

bool VerifyPassword(string password, string hash) => HashPassword(password) == hash;




app.Run();
