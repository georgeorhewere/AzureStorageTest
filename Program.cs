using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorageTest
{
    class Program
    {
        private static string _defaultContainerName;
        public static string ConnectionString { get; }
        public static BlobServiceClient BlobServiceClient;
        static async Task Main(string[] args)
        {
           string ConnectionString = ConfigurationManager.AppSettings["StorageAccountConnection"];
           BlobServiceClient = new BlobServiceClient(ConnectionString);
           _defaultContainerName = ConfigurationManager.AppSettings["StudentContainer"];

            string inputFile = "F:/Projects/Test Files/StudentDataValid.xlsx";
            try
            {

                // get container
                var container = GetContainer();
                if (container != null && File.Exists(inputFile))
                {
                    FileInfo importFile = new FileInfo(inputFile);
                    Console.WriteLine($"File Name {importFile.Name}");
                    var importFileName = $"{importFile.Name.Split('.')[0]}{Guid.NewGuid().ToString()}{importFile.Extension}";
                    if (!container.GetBlobClient(importFileName).Exists())
                    {
                        Console.WriteLine($"File Name {importFileName}");
                        var result = await UploadBlobAsync(container, importFileName, inputFile);
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error: {ex.Message} {ex.InnerException.Message}");
            }

            Console.ReadLine();
        }

        public static BlobContainerClient GetContainer(string containerName = null)
        {
            containerName = string.IsNullOrWhiteSpace(containerName) ? _defaultContainerName : containerName;
            return BlobServiceClient.GetBlobContainerClient(containerName);
        }

        /// <summary>
        /// Upload to blob container
        /// </summary>
        /// <param name="containerClient">The BlobContainerClient representing the container to upload to</param>
        /// <param name="fileName">The name of the file in the Container</param>
        /// <param name="fullLocalPath">The local file path of the blob to be uploaded e.g ./data/reading.jpg</param>
        /// <returns></returns>
        public static async Task<Azure.Response<BlobContentInfo>> UploadBlobAsync(BlobContainerClient containerClient, string fileName, string fullLocalPath)
        {
            BlobClient blobClient = containerClient.GetBlobClient(fileName);
            Azure.Response<BlobContentInfo> result = null;

            // Open the file and upload its data
            using (FileStream uploadFileStream = File.OpenRead(fullLocalPath))
            {
                result = await blobClient.UploadAsync(uploadFileStream);
                uploadFileStream.Close();
            }

            return result;
        }

        public static async Task<Azure.Response<BlobContentInfo>> UploadBlobAsync(BlobContainerClient containerClient, string fileName, byte[] file)
        {
            BlobClient blobClient = containerClient.GetBlobClient(fileName);
            Azure.Response<BlobContentInfo> result = null;

            // Open the file and upload its data
            using (MemoryStream uploadFileStream = new MemoryStream(file))
            {
                result = await blobClient.UploadAsync(uploadFileStream);
                uploadFileStream.Close();
            }
            return result;
        }
    }
}
