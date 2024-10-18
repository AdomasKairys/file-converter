using FFMpegCore;
using System;
using System.IO;
using Microsoft.AspNetCore.StaticFiles;
using SkiaSharp;

namespace file_converter_api;

public static class FileConverterLogic
{
    public static (Stream Stream, string ConvertedType, string FileName) ConvertImage(Stream fileStream, string contentType, string fileName)
    {
        SKEncodedImageFormat format = SKEncodedImageFormat.Jpeg;
        
        SKImage image = SKImage.FromEncodedData(fileStream);

        var outputStream = image.Encode(format, 100).AsStream();

        string outputContentType;
        string outputFileName = Path.ChangeExtension(fileName, format.ToString().ToLower());
        new FileExtensionContentTypeProvider().TryGetContentType(outputFileName, out outputContentType);
        outputContentType = outputContentType ?? "application/octet-stream";

        image.Dispose();

        return (outputStream, outputContentType, outputFileName);
    }
}

