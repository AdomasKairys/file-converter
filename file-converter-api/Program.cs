using file_converter_api.Models;
using file_converter_api.Services;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using FFMpegCore;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDB"));
builder.Services.AddSingleton<FileConverterService>();
GlobalFFOptions.Configure(new FFOptions { BinaryFolder = "/usr/bin", TemporaryFilesFolder = "/tmp" });

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


