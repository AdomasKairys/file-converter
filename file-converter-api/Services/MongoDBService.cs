using file_converter_api.Models;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Extensions.Options;
using MongoDB.Driver.GridFS;

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

    public async Task<(bool IsSuccess, string Message)> CreateAsync(FileCollection fileCollection) 
    {
        IFormFile file = fileCollection.file;
        try
        {
            using (var fileStream = file.OpenReadStream())
            {
                var convertedFile = FileConverterLogic.ConvertImage(fileStream, fileCollection.fileName, fileCollection.convertTo);

                GridFSUploadOptions uploadOptions = new GridFSUploadOptions() { ContentType = convertedFile.ConvertedType };
                var id = await _bucket.UploadFromStreamAsync(convertedFile.FileName, convertedFile.Stream, uploadOptions);

                fileCollection.convertedFileId = id;

                await _fileCollection.InsertOneAsync(fileCollection);
            }
            return (true, fileCollection.Id);
        }
        catch (Exception ex)
        {
            return (false,ex.Message);
        }
    }
    public async Task<List<FileCollection>> GetAsync()
    {
        return await _fileCollection.Find(new BsonDocument()).ToListAsync();
    }
    public async Task<(byte[] Bytes, string ConvertedType, string FileName)> GetConvertedFileAsync(string id)
    {
        FilterDefinition<FileCollection> filter = Builders<FileCollection>.Filter.Eq("Id", id);
        var element = (await _fileCollection.FindAsync(filter)).First();

        if (element.convertedFileId.HasValue) // change to convertedFileId
        {
            FilterDefinition<GridFSFileInfo> filterGridFs = Builders<GridFSFileInfo>.Filter.Eq("_id", element.convertedFileId.Value);
            var file = (await _bucket.FindAsync(filterGridFs)).First();
            byte[] fileBytes = await _bucket.DownloadAsBytesAsync(element.convertedFileId.Value);
           
            return (fileBytes, file.ContentType, file.Filename);
        }
        throw new Exception("no file found");
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
        var element = (await _fileCollection.FindAsync(filter)).First();
        if (element.convertedFileId.HasValue)
        {
            await _bucket.DeleteAsync(element.convertedFileId.Value);
        }
        await _fileCollection.DeleteOneAsync(filter);
        return;
    }
}

