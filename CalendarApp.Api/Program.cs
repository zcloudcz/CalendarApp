using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the dependency injection container
builder.Services.AddControllers();  // Enable API controllers
builder.Services.AddOpenApi();      // Enable OpenAPI/Swagger documentation

// Configure CORS (Cross-Origin Resource Sharing) for Blazor WebAssembly client
// This allows the frontend (running on ports 7216/5193) to call this API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorWasm",
        policy =>
        {
            policy.WithOrigins("https://localhost:7216", "http://localhost:5193")  // Blazor WASM URLs
                  .AllowAnyHeader()     // Allow any HTTP header
                  .AllowAnyMethod();    // Allow any HTTP method (GET, POST, etc.)
        });
});

// Set Czech culture globally for proper date/month name formatting
// This ensures that month names appear in Czech (e.g., "Leden" instead of "January")
CultureInfo.CurrentCulture = new CultureInfo("cs-CZ");

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();  // Enable OpenAPI endpoint in development
}

app.UseHttpsRedirection();          // Redirect HTTP to HTTPS
app.UseCors("AllowBlazorWasm");     // Apply CORS policy
app.UseStaticFiles();               // Serve static files (uploaded images from wwwroot/uploads)
app.MapControllers();               // Map API controller endpoints

app.Run();
