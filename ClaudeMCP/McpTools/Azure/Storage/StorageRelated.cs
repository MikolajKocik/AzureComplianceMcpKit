using System.ComponentModel;
using Azure.Storage.Blobs;
using ModelContextProtocol.Server;
using System.Text;
using Azure.ResourceManager.Resources;
using Azure.Core;
using Azure.ResourceManager.Storage.Models;
using Azure;
using Azure.ResourceManager.Storage;
using Azure.ResourceManager;

namespace ClaudeMCP.McpTools.Azure.Storage;

[McpServerToolType]
public sealed class StorageRelated
{   
    private readonly ArmClient arm;
    private readonly BlobServiceClient blobService;

    public StorageRelated(
         BlobServiceClient blobService,
         ArmClient arm
         )
    {
        this.blobService = blobService;
        this.arm = arm;
    }

    /// <summary>
    /// Downloads the content of a text file from Azure Blob Storage and returns it as a string.
    /// </summary>
    /// <remarks>This method retrieves the specified blob from Azure Blob Storage, reads its content into
    /// memory,  and converts it to a string using the specified encoding. The default encoding is UTF-8.</remarks>
    /// <param name="containerName">The name of the Azure Blob Storage container that contains the blob. This value cannot be null or empty.</param>
    /// <param name="blobName">The name of the blob to download. This value cannot be null or empty.</param>
    /// <param name="encoding">The name of the text encoding to use when converting the blob's content to a string.  Supported values are
    /// "utf8" (default), "utf-8", or "ascii". If null or an unsupported value is provided, UTF-8 encoding is used.</param>
    /// <returns>A string containing the content of the downloaded blob.</returns>
    [McpServerTool, Description("Downloads the content of a text file from Azure Blob Storage and returns it as a string.")]
    public async Task<string> FetchBlobTextAsync(
        string containerName,
        string blobName,
        string? encoding = "utf8"
        )
    {
        if (string.IsNullOrEmpty(containerName))
            throw new ArgumentException("Container name cannot be null or empty.", nameof(containerName));

        if (string.IsNullOrEmpty(blobName))
            throw new ArgumentException("Blob name cannot be null or empty.", nameof(blobName));

        BlobContainerClient container = this.blobService.GetBlobContainerClient(containerName);
        BlobClient blob = container.GetBlobClient(blobName);

        using var stream = new MemoryStream();
        await blob.DownloadToAsync(stream);

        stream.Position = 0;

        return encoding?.ToLowerInvariant() switch
        {
            "utf8" or "utf-8" or null => Encoding.UTF8.GetString(stream.ToArray()),
            "ascii" => Encoding.ASCII.GetString(stream.ToArray()),
            _ => Encoding.UTF8.GetString(stream.ToArray()),
        };
    }

    /// <summary>
    /// Checks whether encryption is enabled for the Blob service of a specified storage account.
    /// </summary>
    /// <remarks>This method retrieves the encryption settings for the Blob service of the specified storage
    /// account by querying the Azure Resource Manager. Ensure that the caller has appropriate permissions to access the
    /// subscription, resource group, and storage account.</remarks>
    /// <param name="subscriptionId">The subscription ID that contains the storage account.</param>
    /// <param name="resourceGroup">The name of the resource group that contains the storage account.</param>
    /// <param name="storageAccountName">The name of the storage account to check.</param>
    /// <returns>A string indicating the encryption status of the Blob service.  Returns "Encryption BLOB: Turned On" if
    /// encryption is enabled; otherwise, "Encryption BLOB: Turned Off".</returns>
    [McpServerTool, Description("Checks if Storage Account has encryption enabled")]
    public async Task<string> CheckStorageEncryptionAsync(string subscriptionId, string resourceGroup, string storageAccountName)
    {
        SubscriptionResource sub = this.arm.GetSubscriptionResource(new ResourceIdentifier($"/subscriptions/{subscriptionId}"));
        Response<ResourceGroupResource> rg = sub.GetResourceGroup(resourceGroup);
        Response<StorageAccountResource> storage = await rg.Value.GetStorageAccountAsync(storageAccountName);

        StorageAccountEncryption props = storage.Value.Data.Encryption;
        bool enabled = props.Services?.Blob?.IsEnabled ?? false;

        return enabled
            ? "Encryption BLOB: Turned On"
            : "Encryption BLOB: Turned Off";

    }
}
