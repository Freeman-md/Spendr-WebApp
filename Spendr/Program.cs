using Microsoft.Extensions.Azure;
using Microsoft.Azure.Cosmos;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddSingleton<CosmosClient>(
    _ => new CosmosClient(builder.Configuration["ConnectionsStrings:CosmosDB"])
    );

builder.Services.AddSingleton<CosmosDBService>(
    sp => new CosmosDBService(
        sp.GetRequiredService<CosmosClient>(),
        builder.Configuration["CosmosDB:DatabaseName"]
    )
);

builder.Services.AddScoped<ExpenseRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
