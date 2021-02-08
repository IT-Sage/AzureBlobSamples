using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AzureBlobSamples
{
    class Program
    {
        private static BlobServiceClient blobServiceClient;
        private static BlobContainerClient containerClient;
        private static BlobClient blobClient;

        private const string fileName = "itixo-logo-default.png";

        static async Task Main()
        {
            CreateDemo();
            await CreateContainer();
            await UploadBlob();
            await ListBlobs();
            await DownloadBlob();
            await DeleteContainer();
        }

        private static IConfiguration GetConfiguration()
        {
            return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .Build();
        }

        private static void CreateDemo()
        {
            IConfiguration configuration = GetConfiguration();
            string connectionString = configuration.GetConnectionString("az204storagesample");

            blobServiceClient = new BlobServiceClient(connectionString);
            string containerName = "pictures-from-portal";
            containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        }

        private static async Task CreateContainer()
        {
            string containerName2 = "pictures-from-portal-2";
            containerClient = await blobServiceClient.CreateBlobContainerAsync(containerName2);
        }

        private static async Task UploadBlob()
        {
            string path = "Data";
            blobClient = containerClient.GetBlobClient(fileName);

            string localFilePath = Path.Combine(path, fileName);
            var response = await blobClient.UploadAsync(localFilePath, overwrite: true);
        }

        private static async Task ListBlobs()
        {
            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
            {
                Console.WriteLine($"Blobs in container {containerClient.Name}:");
                Console.WriteLine("\t" + blobItem.Name);
            }
        }

        private static async Task DownloadBlob()
        {
            blobClient = containerClient.GetBlobClient(fileName);
            var download = await blobClient.DownloadToAsync(fileName);
        }

        private static async Task DeleteContainer()
        {
            await containerClient.DeleteAsync();
        }


    }
}
