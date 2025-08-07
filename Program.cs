using Microsoft.Extensions.FileProviders;
using StudentApi.Data;
using StudentApi.Hubs; // for ChatHub.cs

var builder = WebApplication.CreateBuilder(args);

// CORS Policy Name
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// Add CORS Service
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("https://localhost:44370") // Your MVC front-end URL
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowCredentials(); // Important for SignalR
                      });
});

// Add services to the container
builder.Services.AddControllers()
    .AddNewtonsoftJson();

// Register data access services
builder.Services.AddScoped<SqlDataAccess>();
builder.Services.AddScoped<ChatDataAccess>();

// Add SignalR
builder.Services.AddSignalR();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "wwwroot/images")),
    RequestPath = "/images"
});

app.UseHttpsRedirection();

// IMPORTANT: CORS must come before SignalR and Controllers
app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization();

app.MapControllers();

// Map SignalR Hub
app.MapHub<ChatHub>("/chathub");

app.Run();

