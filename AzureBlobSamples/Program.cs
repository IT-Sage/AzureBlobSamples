using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
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
        private const string containerName = "pictures-from-portal-demo";
        static async Task Main()
        {
            CreateDemo();
            //await CreateContainer();
            //await UploadBlob();
            //await SetMetadata();
            //await CopyBlob();
            //await ListBlobs();
            //await DownloadBlob();
            //await DeleteContainer();
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
            
            containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        }

        private static async Task CreateContainer()
        {
            containerClient = await blobServiceClient.CreateBlobContainerAsync(containerName);
        }

        private static async Task UploadBlob()
        {
            string path = "Data";
            blobClient = containerClient.GetBlobClient(fileName);
            
            string localFilePath = Path.Combine(path, fileName);
            var blobHttpHeaders = new BlobHttpHeaders { ContentType = "image/png" };
            var response = await blobClient.UploadAsync(localFilePath, httpHeaders: blobHttpHeaders);
        }

        private static async Task SetMetadata()
        {
            blobClient = containerClient.GetBlobClient(fileName);

            IDictionary<string, string> metadata = new Dictionary<string, string>();

            metadata.Add("blobType", "picture");

            metadata["description"] = "Itixo logo.";

            await blobClient.SetMetadataAsync(metadata);
        }

        private static async Task CopyBlob()
        {
            blobClient = containerClient.GetBlobClient(fileName);
            if (await blobClient.ExistsAsync())
            {
                BlobLeaseClient leaseClient = blobClient.GetBlobLeaseClient();

                // Specifying -1 for the lease interval creates an infinite lease.
                await leaseClient.AcquireAsync(TimeSpan.FromSeconds(-1));

                // Get the source blob's properties and display the lease state.
                BlobProperties sourceProperties = await blobClient.GetPropertiesAsync();
                Console.WriteLine($"Lease state: {sourceProperties.LeaseState}");

                // Get a BlobClient representing the destination blob with a unique name.
                BlobClient destBlob = containerClient.GetBlobClient(Guid.NewGuid() + "-" + blobClient.Name);

                // Start the copy operation.
                await destBlob.StartCopyFromUriAsync(blobClient.Uri);

                // Get the destination blob's properties and display the copy status.
                BlobProperties destProperties = await destBlob.GetPropertiesAsync();

                Console.WriteLine($"Copy status: {destProperties.CopyStatus}");
                Console.WriteLine($"Copy progress: {destProperties.CopyProgress}");
                Console.WriteLine($"Completion time: {destProperties.CopyCompletedOn}");
                Console.WriteLine($"Total bytes: {destProperties.ContentLength}");

                // Update the source blob's properties.
                sourceProperties = await blobClient.GetPropertiesAsync();

                if (sourceProperties.LeaseState == LeaseState.Leased)
                {
                    // Break the lease on the source blob.
                    await leaseClient.BreakAsync();

                    // Update the source blob's properties to check the lease state.
                    sourceProperties = await blobClient.GetPropertiesAsync();
                    Console.WriteLine($"Lease state: {sourceProperties.LeaseState}");
                }
            }
        }

        private static async Task ListBlobs()
        {
            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
            {
                Console.WriteLine($"Blobs in container {containerClient.Name}:");
                Console.WriteLine($"\tName: {blobItem.Name}");
                Console.WriteLine($"\tETag: {blobItem.Properties.ETag}");
                Console.WriteLine($"\tLastModified: {blobItem.Properties.LastModified}");
                Console.WriteLine();
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
