using file_converter_api.Models;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Extensions.Options;
using MongoDB.Driver.GridFS;
using System;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace file_converter_api.Services;

public class MongoDBService
{
    private readonly IMongoCollection<FileCollection> _fileCollection;
    private readonly IGridFSBucket _bucket;

    public MongoDBService(IOptions<MongoDBSettings> mongoDBSettigns)
    {
        MongoClient client = new MongoClient(mongoDBSettigns.Value.ConnectionURI);
        IMongoDatabase database = client.GetDatabase(mongoDBSettigns.Value.DatabaseName);
        _fileCollection = database.GetCollection<FileCollection>(mongoDBSettigns.Value.CollectionName);
        _bucket = new GridFSBucket(database);
    }

    public async Task CreateAsync(FileCollection fileCollection) 
    {
        IFormFile file = fileCollection.file;
        long length = file.Length;
        using var fileStream = file.OpenReadStream();
        byte[] bytes = new byte[length];
        fileStream.Read(bytes, 0, (int)file.Length);

        var id = await _bucket.UploadFromBytesAsync("filename", bytes);

        //await _fileCollection.InsertOneAsync(fileCollection);
        return;
    }
    public async Task<List<FileCollection>> GetAsync()
    {
        return await _fileCollection.Find(new BsonDocument()).ToListAsync();
    }
    public async Task AddToFileCollectionAsync(string id, string fileId)
    {
        FilterDefinition<FileCollection> filter = Builders<FileCollection>.Filter.Eq("Id", id);
        UpdateDefinition<FileCollection> update = Builders<FileCollection>.Update.AddToSet<string>("fileId", fileId);
        await _fileCollection.UpdateOneAsync(filter, update);
        return;
    }
    public async Task DeleteAsync(string id)
    {
        FilterDefinition<FileCollection> filter = Builders<FileCollection>.Filter.Eq("Id", id);
        await _fileCollection.DeleteOneAsync(filter);
        return;
    }
}

