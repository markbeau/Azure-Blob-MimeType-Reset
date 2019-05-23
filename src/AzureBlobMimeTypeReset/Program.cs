using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;

namespace AzureBlobMimeTypeReset
{
    class Options
    {
        [Option('n', Required = true, HelpText = "Storage Account Name")]
        public string AccountName { get; set; }

        [Option('k', Required = true, HelpText = "Storage Account Key")]
        public string AccountKey { get; set; }

        [Option('c', Required = true, HelpText = "Blob Container Name")]
        public string ContainerName { get; set; }
    }

    class Program
    {
        public static Options Options { get; set; }

        public static CloudBlobContainer BlobContainer { get; set; }

        static async Task Main(string[] args)
        {
            var parserResult = Parser.Default.ParseArguments<Options>(args);
            if (parserResult.Tag == ParserResultType.Parsed)
            {
                Options = ((Parsed<Options>)parserResult).Value;

                // 1. Get Azure Blob Files
                WriteMessage($"[{DateTime.Now}] Finding Image Files on Azure Blob Storage...");
                BlobContainer = GetBlobContainer();
                if (null == BlobContainer)
                {
                    WriteMessage("ERROR: Can not get BlobContainer.", ConsoleColor.Red);
                    Console.ReadKey();
                    return;
                }

                WriteMessage($"[{DateTime.Now}] Updating Mime Type...");
                foreach (var blob in BlobContainer.ListBlobs().OfType<CloudBlockBlob>())
                {
                    WriteMessage($"[{DateTime.Now}] Updating {blob.Uri.AbsoluteUri}");
                    string extension = Path.GetExtension(blob.Uri.AbsoluteUri);
                    switch (extension)
                    {
                        case ".jpg":
                        case ".jpeg":
                            blob.Properties.ContentType = "image/jpeg";
                            break;
                        case ".png":
                            blob.Properties.ContentType = "image/png";
                            break;
                        default:
                            break;
                    }
                    await blob.SetPropertiesAsync();
                }
            }

            Console.ReadKey();
        }

        private static void WriteMessage(string message, ConsoleColor color = ConsoleColor.White, bool resetColor = true)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            if (resetColor)
            {
                Console.ResetColor();
            }
        }

        private static CloudBlobContainer GetBlobContainer()
        {
            CloudStorageAccount storageAccount = new CloudStorageAccount(new StorageCredentials(Options.AccountName, Options.AccountKey), true);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(Options.ContainerName);
            return container;
        }
    }
}
