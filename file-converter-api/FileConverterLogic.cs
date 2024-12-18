using FFMpegCore;
using System;
using System.IO;
using Microsoft.AspNetCore.StaticFiles;
using SkiaSharp;
using ImageMagick;
using Amazon.Runtime.Internal.Util;
using FFMpegCore.Enums;
using System.IO.Compression;
using file_converter_api.Models;
using System.Text.Json;

namespace file_converter_api;

public static class FileConverterLogic
{
    public static void GetImageFormatInfoToFile()
    {
        List<FileFormat> fileFormats = new List<FileFormat>();

        foreach (var format in Enum.GetNames(typeof(MagickFormat)))
        {
            var formatInfo = MagickFormatInfo.Create((MagickFormat)Enum.Parse(typeof(MagickFormat),format));
            if(formatInfo == null) continue;
            fileFormats.Add(new FileFormat() { 
                Format = format, 
                Mime = formatInfo.MimeType ?? "",
                IsReadable = formatInfo.SupportsReading, 
                IsWritable = formatInfo.SupportsWriting
            });
        }
        string json = JsonSerializer.Serialize(fileFormats);
        File.WriteAllText("format-info.json", json);
    }
    public static (Stream Stream, string ConvertedType, string FilePath) ImagesToVideo(Stream framesZip, string format, int framerate = 1)
    {
        string zipDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        ZipFile.ExtractToDirectory(framesZip, zipDirectory);

        var outputFile = Path.GetTempFileName();
        var outputFilePath = Path.ChangeExtension(outputFile, "mp4");
        var outputStream = new FileStream(outputFilePath, FileMode.Create);

        // Could use this if it worked, but it doesn't there is still an open issue on github
        // FFMpeg.JoinImageSequence(outputFilePath, frameRate: 1, framePaths);

        FFMpegArguments
            .FromConcatInput(Directory.GetFiles(zipDirectory), options => options.WithFramerate(framerate))
            .OutputToFile(outputFilePath, true,
                options =>
                    options
                    .WithConstantRateFactor(1)
                    .ForcePixelFormat("yuv420p")
                )
            .ProcessSynchronously();

        new FileExtensionContentTypeProvider().TryGetContentType(outputFilePath, out string outputContentType);
        outputContentType = outputContentType ?? "application/octet-stream";

        return (outputStream, outputContentType, outputFilePath);
    }
    public static (Stream Stream, string ConvertedType, string FileName) ConvertImage(Stream fileStream, string fileName, string convertTo)
    {
        Console.WriteLine(convertTo);
        if (Enum.TryParse(convertTo, out MagickFormat format))
        {
            //SKImage image = SKImage.FromEncodedData(fileStream);

            var image = new MagickImage(fileStream);
            var outputStream = new MemoryStream(); // Replace with FileStream if traffic is big

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

