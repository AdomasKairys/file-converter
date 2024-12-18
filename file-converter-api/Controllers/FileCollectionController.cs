using System;
using Microsoft.AspNetCore.Mvc;
using file_converter_api.Services;
using file_converter_api.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IO;

namespace file_converter_api.Controllers;

[Controller]
[Route("api/[controller]")]
public class FileCollectionController : ControllerBase
{
    private readonly FileConverterService _fileConverterService;
    private readonly ClamAVService _clamService;
    private readonly ILogger<FileCollectionController> _logger;

    public FileCollectionController(FileConverterService fileConverterService, ClamAVService clamService, ILogger<FileCollectionController> logger)
    {
        _fileConverterService = fileConverterService;
        _logger = logger;
        _clamService = clamService;
    }

    [HttpGet]
    public async Task<List<FileCollection>> Get()
    {
        return await _fileConverterService.GetAsync();
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetConvertedFile(string id)
    {
        try
        {
            var file = await _fileConverterService.GetConvertedFileAsync(id);
            return File(file.Bytes, file.ConvertedType, file.FileName);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }
    [HttpGet("MultiDownload")]
    public async Task<IActionResult> GetMultipleFilesAsZip([FromQuery] string[] ids)
    {
        try
        {
            _logger.LogInformation(string.Join(";", ids));
            var file = await _fileConverterService.GetMultipleFilesAsZipAsync(ids);
            HttpContext.Response.RegisterForDispose(file.Stream);
            return File(file.Stream, file.ConvertedType, file.FileName);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // TODO: change to [HttpPost("/convertImage")]
    [HttpPost]
    public async Task<IActionResult> Post([FromForm] FileCollection fileToConvert)
    {
        if(!await _clamService.ScanFileForViruses(fileToConvert.file))
        {
            return BadRequest("File failed virus scan");
        }

        var result = await _fileConverterService.ConvertImageAsync(fileToConvert);

        if (result.IsSuccess)
            return Ok(result.Message);
        return BadRequest(result.Message);
    }

    [HttpPost("ImagesToVideo")]
    public async Task<IActionResult> PostImagesToVideo([FromForm] FileCollection fileToConvert)
    {
        if (!await _clamService.ScanFileForViruses(fileToConvert.file))
        {
            return BadRequest("File failed virus scan");
        }

        var result = await _fileConverterService.ImagesToVideoAsync(fileToConvert);

        if (result.IsSuccess)
            return Ok(result.Message);
        return BadRequest(result.Message);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> AddToFileCollection(string id, [FromBody] string fileId)
    {
        await _fileConverterService.AddToFileCollectionAsync(id, fileId);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id) 
    {
        await _fileConverterService.DeleteAsync(id);
        return NoContent();
    }
}

