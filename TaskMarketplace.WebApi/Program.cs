using System.Text;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using TaskMarketplace.DAL;
using TaskMarketplace.Service;
using TaskMarketplace.Service.Abstractions;
using FluentValidation.AspNetCore;
using TaskMarketplace.WebApi.Validators.Auth;
using TaskMarketplace.WebApi.Validators.Tasks;
using FluentValidation;
using TaskMarketplace.DAL.Repositories;




var builder = WebApplication.CreateBuilder(args);

// JSON: avoid cycles
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "TaskMarketplace API", 
        Version = "v1",
        Description = "API для управления задачами и пользователями",
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });


    c.AddServer(new OpenApiServer { Url = "https://localhost:5000", Description = "Development server" });

    try
    {
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        c.IncludeXmlComments(xmlPath);
    }
    catch
    {
        Console.WriteLine("Не удалось загрузить XML-документацию основного проекта");
    }
    
    try
    {
        var contractsXmlPath = Path.Combine(AppContext.BaseDirectory, "TaskMarketplace.Contracts.xml");
        c.IncludeXmlComments(contractsXmlPath);
    }
    catch
    {
        Console.WriteLine("Не удалось загрузить XML-документацию Contracts проекта");
    }
    
    try
    {
        var serviceXmlPath = Path.Combine(AppContext.BaseDirectory, "TaskMarketplace.Service.xml");
        c.IncludeXmlComments(serviceXmlPath);
    }
    catch
    {
        Console.WriteLine("Не удалось загрузить XML-документацию Service проекта");
    }
    
    c.EnableAnnotations();
    c.OrderActionsBy(apiDesc => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.HttpMethod}");
    
    c.UseAllOfToExtendReferenceSchemas();
    c.UseAllOfForInheritance();
    c.UseOneOfForPolymorphism();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 16)
{
    throw new InvalidOperationException("JWT ключ не настроен или слишком короткий (минимум 16 символов)");
}

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
});
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateTaskRequestValidator>();
var app = builder.Build();

app.UseStaticFiles();
app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskMarketplace API v1");
    c.RoutePrefix = "";
    c.DocumentTitle = "TaskMarketplace API Documentation";
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


app.UseExceptionHandler("/error");
app.Map("/error", () => Results.Problem("Произошла ошибка на сервере"));

app.Run();