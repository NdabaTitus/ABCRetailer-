using ABCRetailer.Models;

namespace ABCRetailer.Services
{
    public interface IAzureStorageService
    {
        Task<List<T>> GetAllEntitiesAsync<T>() where T : class, Azure.Data.Tables.ITableEntity, new();
        Task<T?> GetEntityAsync<T>(string partitionKey, string rowKey) where T : class, Azure.Data.Tables.ITableEntity, new();
        Task<T> AddEntityAsync<T>(T entity) where T : class, Azure.Data.Tables.ITableEntity;
        Task<T> UpdateEntityAsync<T>(T entity) where T : class, Azure.Data.Tables.ITableEntity;
        Task DeleteEntityAsync<T>(string partitionKey, string rowKey) where T : class, Azure.Data.Tables.ITableEntity, new();
        Task<string> UploadImageAsync(IFormFile file, string containerName);
        Task<string> UploadFileAsync(IFormFile file, string containerName);
        Task DeleteBlobAsync(string blobName, string containerName);
        Task SendMessageAsync(string queueName, string message);
        Task<string?> ReceiveMessageAsync(string queueName);
        Task<string> UploadFileShareAsync(IFormFile File, string shareName, string directoryName = "");
        Task<byte[]> DownloadFromFileShareAsync(string shareName, string fileName, string dictoryName = "");

    }
}
