using file_converter_api.Models;
using file_converter_api.Services;
using file_converter_api;
using FFMpegCore;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.AddConsole();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.SetMinimumLevel(LogLevel.Information);
builder.Logging.AddConsole();

// Add services to the container.
builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDB"));
builder.Services.AddSingleton<ClamAVService>();
builder.Services.AddSingleton<FileConverterService>();


GlobalFFOptions.Configure(new FFOptions { BinaryFolder = "/usr/bin", TemporaryFilesFolder = "/tmp" });
FileConverterLogic.GetImageFormatInfoToFile();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// TODO increase more and add limits in client side
app.Use(async (context, next) => {
    context.Features.Get<IHttpMaxRequestBodySizeFeature>().MaxRequestBodySize = 104857600; // 100 MB
    await next.Invoke();
});
//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


