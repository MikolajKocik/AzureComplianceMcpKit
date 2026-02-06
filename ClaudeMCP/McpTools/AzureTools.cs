using Azure;
using Azure.Core;
using Azure.Monitor.Query;
using Azure.Monitor.Query.Models;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Storage;
using Azure.ResourceManager.Storage.Models;
using Azure.Storage.Blobs;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text;

namespace ClaudeMCP.McpTools;

[McpServerToolType]
public sealed class AzureTools
{
    private readonly ArmClient arm;

    public AzureTools(
        LogsQueryClient logs,
        ArmClient arm)
    {
        this.arm = arm;
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
        SubscriptionResource sub = _arm.GetSubscriptionResource(new ResourceIdentifier($"/subscriptions/{subscriptionId}"));
        Response<ResourceGroupResource> rg = sub.GetResourceGroup(resourceGroup);
        Response<StorageAccountResource> storage = await rg.Value.GetStorageAccountAsync(storageAccountName);

        StorageAccountEncryption props = storage.Value.Data.Encryption;
        bool enabled = props.Services?.Blob?.IsEnabled ?? false;

        return enabled
            ? "Encryption BLOB: Turned On"
            : "Encryption BLOB: Turned Off";

    }
}