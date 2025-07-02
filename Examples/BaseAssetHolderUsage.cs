using SFManagement.Models.Entities;

namespace SFManagement.Examples;

public class BaseAssetHolderUsageExamples
{
    public void Example1_GettingTheSpecificAssetHolder()
    {
        // Let's say you have a BaseAssetHolder that represents a Client
        var baseAssetHolder = new BaseAssetHolder
        {
            Id = Guid.NewGuid(),
            Name = "John Doe",
            Client = new Client { Id = Guid.NewGuid(), BaseAssetHolderId = Guid.NewGuid() }
            // Bank, Member, PokerManager are null
        };

        // Get the specific asset holder object
        var specificAssetHolder = baseAssetHolder.SpecificAssetHolder;
        // specificAssetHolder will be the Client object

        // Check the type using the enum
        var assetHolderType = baseAssetHolder.AssetHolderType;
        // assetHolderType will be AssetHolderType.Client
    }

    public void Example2_TypeChecking()
    {
        var baseAssetHolder = new BaseAssetHolder();
        
        // Check what type of asset holder this is
        switch (baseAssetHolder.AssetHolderType)
        {
            case AssetHolderType.Client:
                var client = baseAssetHolder.Client;
                // Do client-specific operations
                break;
            case AssetHolderType.Bank:
                var bank = baseAssetHolder.Bank;
                // Do bank-specific operations
                break;
            case AssetHolderType.Member:
                var member = baseAssetHolder.Member;
                // Do member-specific operations
                break;
            case AssetHolderType.PokerManager:
                var pokerManager = baseAssetHolder.PokerManager;
                // Do poker manager-specific operations
                break;
            case AssetHolderType.Unknown:
                // Handle case where no specific asset holder is set
                break;
        }
    }

    public void Example3_ServiceMethodUsage()
    {
        // In your BaseAssetHolderService, you can now easily determine the type
        var baseAssetHolder = new BaseAssetHolder();
        
        // Get the specific entity for filtering
        var specificEntity = baseAssetHolder.SpecificAssetHolder;
        
        // Or check the type to apply different logic
        if (baseAssetHolder.AssetHolderType == AssetHolderType.Client)
        {
            // Apply client-specific logic
        }
        else if (baseAssetHolder.AssetHolderType == AssetHolderType.Bank)
        {
            // Apply bank-specific logic
        }
    }

    public void Example4_ControllerUsage()
    {
        // In your controllers, you can now easily work with the specific type
        var baseAssetHolder = new BaseAssetHolder();
        
        // Get the specific asset holder for operations
        var specificAssetHolder = baseAssetHolder.SpecificAssetHolder;
        
        // You can cast it to the specific type if needed
        if (baseAssetHolder.AssetHolderType == AssetHolderType.Client)
        {
            var client = (Client)specificAssetHolder;
            // Work with client-specific properties
        }
    }

    public void Example5_Validation()
    {
        var baseAssetHolder = new BaseAssetHolder();
        
        // Validate that exactly one specific asset holder is set
        var hasClient = baseAssetHolder.Client != null;
        var hasBank = baseAssetHolder.Bank != null;
        var hasMember = baseAssetHolder.Member != null;
        var hasPokerManager = baseAssetHolder.PokerManager != null;
        
        var count = (hasClient ? 1 : 0) + (hasBank ? 1 : 0) + (hasMember ? 1 : 0) + (hasPokerManager ? 1 : 0);
        
        if (count != 1)
        {
            throw new InvalidOperationException("BaseAssetHolder must have exactly one specific asset holder type");
        }
    }
} 