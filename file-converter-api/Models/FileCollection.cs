using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using System.Text.Json.Serialization;

namespace file_converter_api.Models;

public class FileCollection
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public ObjectId? convertedFileId { get; set; } = null!;

    [BsonIgnore]
    public IFormFile file { get; set; } = null!;
    [BsonIgnore]
    public string convertTo { get; set; } = null!;
    [BsonIgnore]
    public string fileName { get; set; } = null!;
}

