using System;
using Microsoft.AspNetCore.Mvc;
using file_converter_api.Services;
using file_converter_api.Models;
using Microsoft.AspNetCore.Mvc.Filters;

namespace file_converter_api.Controllers;

[Controller]
[Route("api/[controller]")]
public class FileCollectionController : Controller
{
    private readonly MongoDBService _mongoDBService;
    private readonly ILogger<FileCollectionController> _logger;

    public FileCollectionController(MongoDBService mongoDBService, ILogger<FileCollectionController> logger)
    {
        _mongoDBService = mongoDBService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<List<FileCollection>> Get()
    {
        return await _mongoDBService.GetAsync();
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromForm] FileCollection file) 
    {
        await _mongoDBService.CreateAsync(file);
        return NoContent();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> AddToFileCollection(string id, [FromBody] string fileId)
    {
        await _mongoDBService.AddToFileCollectionAsync(id, fileId);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id) 
    {
        await _mongoDBService.DeleteAsync(id);
        return NoContent();
    }
}

