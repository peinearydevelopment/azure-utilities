using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BlobMover
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var from = new BlobLocationInfo
            {
                ConnectionString = "AZURE_STORAGE_CONNECTION_STRING",
                ContainerName = "CONTAINER_NAME",
                VirtualDirectoryPath = string.Empty
            };

            var to = new BlobLocationInfo
            {
                ConnectionString = "AZURE_STORAGE_CONNECTION_STRING",
                ContainerName = "CONTAINER_NAME",
                VirtualDirectoryPath = string.Empty
            };


            await Task.WhenAll(
                MigrateBlobs(from, to)
            ).ConfigureAwait(false);
        }

        static async Task MigrateBlobs(BlobLocationInfo from, BlobLocationInfo to)
        {
            var fromClient = CloudStorageAccount.Parse(from.ConnectionString).CreateCloudBlobClient();
            var toClient = CloudStorageAccount.Parse(to.ConnectionString).CreateCloudBlobClient();

            var fromContainer = fromClient.GetContainerReference(from.ContainerName);
            var toContainer = toClient.GetContainerReference(to.ContainerName);

            var blobs = await fromContainer.ListBlobsSegmentedAsync(from.VirtualDirectoryPath, true, BlobListingDetails.Metadata, null, new BlobContinuationToken(), new BlobRequestOptions(), new OperationContext()).ConfigureAwait(false);
            await MoveBlobs(blobs.Results, toContainer, to.VirtualDirectoryPath).ConfigureAwait(false);
        }

        static async Task MoveBlobs(IEnumerable<IListBlobItem> blobs, CloudBlobContainer toContainer, string toVirtualDirectoryPath)
        {
            await Task.WhenAll(blobs.Select(async blobItem =>
            {
                if (blobItem is CloudBlockBlob blob)
                {
                    CloudBlockBlob newBlob;
                    if (!string.IsNullOrWhiteSpace(toVirtualDirectoryPath) && !toVirtualDirectoryPath.EndsWith("/"))
                    {
                        newBlob = toContainer.GetBlockBlobReference($"{toVirtualDirectoryPath}/{blob.Name}");
                    }
                    else
                    {
                        newBlob = toContainer.GetBlockBlobReference($"{toVirtualDirectoryPath}{blob.Name}");
                    }

                    using var ms = new MemoryStream();
                    await blob.DownloadToStreamAsync(ms).ConfigureAwait(false);
                    ms.Seek(0, 0);
                    await newBlob.UploadFromStreamAsync(ms).ConfigureAwait(false);
                    await blob.DeleteAsync().ConfigureAwait(false);
                }

                if (blobItem is CloudBlobDirectory directory)
                {
                    var blobs = await directory.ListBlobsSegmentedAsync(false, BlobListingDetails.Metadata, null, new BlobContinuationToken(), new BlobRequestOptions(), new OperationContext()).ConfigureAwait(false);
                    await MoveBlobs(blobs.Results, toContainer, toVirtualDirectoryPath).ConfigureAwait(false);
                }
            }));
        }
    }
}
