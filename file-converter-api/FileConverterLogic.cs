using FFMpegCore;
using System;
using System.IO;
using Microsoft.AspNetCore.StaticFiles;
using SkiaSharp;
using ImageMagick;
using Amazon.Runtime.Internal.Util;

namespace file_converter_api;

public static class FileConverterLogic
{
    public static (Stream Stream, string ConvertedType, string FileName) ConvertImage(Stream fileStream, string fileName, string convertTo)
    {
        Console.WriteLine(convertTo);
        if (Enum.TryParse(convertTo, out MagickFormat format))
        {
            //SKImage image = SKImage.FromEncodedData(fileStream);

            var image = new MagickImage(fileStream);
            var outputStream = new MemoryStream();

            string outputContentType;

            image.Write(outputStream, format);

            outputStream.Position = 0;
            string outputFileName = Path.ChangeExtension(fileName, format.ToString().ToLower());
            new FileExtensionContentTypeProvider().TryGetContentType(outputFileName, out outputContentType);
            outputContentType = outputContentType ?? "application/octet-stream";


            return (outputStream, outputContentType, outputFileName);
        }
        throw new Exception("Unsupported file format");
    }
}

