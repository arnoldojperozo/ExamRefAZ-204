using System.Threading.Tasks;
using System;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace ch2_2_3
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Copy items between Containers Demo!");
            Task.Run(async () => await StartContainersDemo()).Wait();
            Console.WriteLine("Move items between Storage Accounts Demo!");
            Task.Run(async () => await StartAccountDemo()).Wait();
        }

        public static async Task StartContainersDemo()
        {
            string sourceBlobFileName = "Testing.zip";
            AppSettings appSettings = AppSettings.LoadAppSettings();

            //Get a cloud client for the source Storage Account
            BlobServiceClient sourceClient = Common.CreateBlobClientStorageFromSAS(appSettings.SourceSASConnectionString);

            //Get a reference for each container
            var sourceContainerReference = sourceClient.GetBlobContainerClient(appSettings.SourceContainerName);
            var destinationContainerReference = sourceClient.GetBlobContainerClient(appSettings.DestinationContainerName);

            //Get a reference for the source blob
            var sourceBlobReference = sourceContainerReference.GetBlobClient(sourceBlobFileName);
            var destinationBlobReference = destinationContainerReference.GetBlobClient(sourceBlobFileName);

            //Copy the blob from the source container to the destination container
            await destinationBlobReference.StartCopyFromUriAsync(sourceBlobReference.Uri);

        }

        public static async Task StartAccountDemo()
        {
            string sourceBlobFileName = "Testing.zip";
            AppSettings appSettings = AppSettings.LoadAppSettings();

            //Get a cloud client for the source Storage Account
            BlobServiceClient sourceClient = Common.CreateBlobClientStorageFromSAS(appSettings.SourceSASConnectionString);
            //Get a cloud client for the destination Storage Account
            BlobServiceClient destinationClient = Common.CreateBlobClientStorageFromSAS(appSettings.DestinationSASConnectionString);

            //Get a reference for each container
            var sourceContainerReference = sourceClient.GetBlobContainerClient(appSettings.SourceContainerName);
            var destinationContainerReference = destinationClient.GetBlobContainerClient(appSettings.DestinationContainerName);

            //Get a reference for the source blob
            var sourceBlobReference = sourceContainerReference.GetBlobClient(sourceBlobFileName);
            var destinationBlobReference = destinationContainerReference.GetBlobClient(sourceBlobFileName);

            //Move the blob from the source container to the destination container
            await destinationBlobReference.StartCopyFromUriAsync(sourceBlobReference.Uri);
            await sourceBlobReference.DeleteAsync();
        }
    }
}