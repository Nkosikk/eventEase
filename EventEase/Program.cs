using Microsoft.EntityFrameworkCore;
using Azure.Storage.Blobs;
using EventEase.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddLogging();

// Configure DbContext - use DefaultConnection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<EventEase.Data.EventEaseDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register validation service
builder.Services.AddScoped<BookingValidationService>();

// Register Azure Storage service
var azureStorageConfig = builder.Configuration.GetSection("AzureStorage");
var storageConnectionString = azureStorageConfig["ConnectionString"];
var containerName = azureStorageConfig["ContainerName"] ?? "eventease-images";

// Only register Azure Storage service if connection string is provided and not placeholder
if (!string.IsNullOrEmpty(storageConnectionString) && !storageConnectionString.StartsWith("DefaultEndpointsProtocol"))
{
    try
    {
        var blobContainerClient = new BlobContainerClient(new Uri(storageConnectionString), new Azure.Storage.StorageSharedKeyCredential("", ""));
        builder.Services.AddSingleton(blobContainerClient);
        builder.Services.AddScoped<AzureStorageService>();
    }
    catch
    {
        // Azure Storage configuration not available, skipping
    }
}

var app = builder.Build();

// Create the database schema without inserting sample records.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<EventEase.Data.EventEaseDbContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Ensure root URL redirects to Index page
app.MapGet("/", () => Results.Redirect("/Index"));

app.MapRazorPages();

app.Run();
