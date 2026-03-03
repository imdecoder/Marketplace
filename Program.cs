var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.Configure<Marketplace.Models.DatabaseSettings>(
    builder.Configuration.GetSection("DatabaseSettings"));

builder.Services.AddSingleton<Marketplace.Services.OrderAnalysisService>();
builder.Services.AddSingleton<Marketplace.Services.PlatformService>();
builder.Services.AddSingleton<Marketplace.Services.DatabaseSeeder>();

builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger();
    app.UseSwaggerUI();

    app.MapGet("/", () => Results.Redirect("/swagger"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<Marketplace.Services.DatabaseSeeder>();
    await seeder.SeedAsync();
}

app.Run();
