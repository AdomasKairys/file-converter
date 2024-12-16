using FFMpegCore;
using System;
using System.IO;
using Microsoft.AspNetCore.StaticFiles;
using SkiaSharp;
using ImageMagick;
using Amazon.Runtime.Internal.Util;
using FFMpegCore.Enums;

namespace file_converter_api;

public static class FileConverterLogic
{
    public static (Stream Stream, string ConvertedType, string FileName) ImagesToVideo(List<Stream> frames, string fileName)
    {
        string[] framePaths = new string[frames.Count];
        int i = 0;
        foreach (var fileStream in frames)
        {
            var randomFile = Path.GetTempFileName();
            var extension = Path.GetExtension(fileName);
            var targetFilePath = Path.ChangeExtension(randomFile, extension);

            var destinationStream = new FileStream(targetFilePath, FileMode.Create);
            fileStream.CopyTo(destinationStream);
            destinationStream.Close();
            framePaths[i] = targetFilePath;
            i++;
        }
        var outputFile = Path.GetTempFileName();
        var outputFilePath = Path.ChangeExtension(outputFile, "mp4");
        var outputStream = new FileStream(outputFilePath, FileMode.Create);

        // Could use this if it worked, but it doesn't there is still an open issue on github
        // FFMpeg.JoinImageSequence(outputFilePath, frameRate: 1, framePaths);

        FFMpegArguments
            .FromConcatInput(framePaths, options => options.WithFramerate(1))
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

