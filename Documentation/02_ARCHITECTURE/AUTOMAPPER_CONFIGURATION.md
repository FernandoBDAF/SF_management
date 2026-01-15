# AutoMapper Configuration

## Overview

The SF Management system uses AutoMapper for object-to-object mapping between domain entities, request models, and response ViewModels. This configuration is centralized in `AutoMapperProfile.cs`.

---

## Configuration

### Registration

```csharp
// Program.cs
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
```

### Profile Definition

**File**: `AutoMapperProfile.cs`

```csharp
public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // Entity mappings
        CreateEntityMappings();
        // Transaction mappings
        CreateTransactionMappings();
        // Asset mappings
        CreateAssetMappings();
    }
}
```

---

## Key Mappings

### Entity Mappings

```csharp
// Request → Entity
CreateMap<ClientRequest, Client>();
CreateMap<BankRequest, Bank>();
CreateMap<MemberRequest, Member>();
CreateMap<PokerManagerRequest, PokerManager>();

// Entity → Response
CreateMap<Client, ClientResponse>()
    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.BaseAssetHolder.Name));
```

### Transaction Response Mappings

```csharp
CreateMap<FiatAssetTransaction, FiatAssetTransactionResponse>()
    .AfterMap((src, dest, ctx) =>
    {
        dest.SenderWallet = MapWalletIdentifierSummary(src.SenderWalletIdentifier, ctx);
        dest.ReceiverWallet = MapWalletIdentifierSummary(src.ReceiverWalletIdentifier, ctx);
    });
```

### Wallet Summary Helper

```csharp
private static WalletIdentifierSummary? MapWalletIdentifierSummary(
    WalletIdentifier? wallet, 
    ResolutionContext ctx)
{
    if (wallet == null) return null;

    var summary = new WalletIdentifierSummary
    {
        Id = wallet.Id,
        AssetType = wallet.AssetType,
        AssetGroup = wallet.AssetGroup,
        AccountClassification = wallet.AccountClassification
    };

    // Map metadata based on AssetGroup
    switch (wallet.AssetGroup)
    {
        case AssetGroup.FiatAssets:
            summary.DisplayName = wallet.GetBankMetadata(BankWalletMetadata.BankName);
            summary.AccountNumber = wallet.GetBankMetadata(BankWalletMetadata.AccountNumber);
            break;
        case AssetGroup.PokerAssets:
            summary.DisplayName = wallet.GetPokerMetadata(PokerWalletMetadata.PlayerNickname);
            summary.InputForTransactions = wallet.GetPokerMetadata(PokerWalletMetadata.InputForTransactions);
            break;
        // etc.
    }

    return summary;
}
```

### Company Pool Mapping

```csharp
CreateMap<AssetPool, CompanyAssetPoolResponse>()
    .ForMember(dest => dest.IsCompanyPool, 
        opt => opt.MapFrom(src => src.BaseAssetHolderId == null))
    .ForMember(dest => dest.OwnerName, 
        opt => opt.MapFrom(src => src.BaseAssetHolderId == null 
            ? "Company" 
            : src.BaseAssetHolder.Name));
```

---

## Usage

### In Controllers

```csharp
public class ClientController : BaseAssetHolderController<Client, ClientRequest, ClientResponse>
{
    private readonly IMapper _mapper;

    public override async Task<IActionResult> Get()
    {
        var entities = await _service.List();
        var response = _mapper.Map<List<ClientResponse>>(entities);
        return Ok(response);
    }
}
```

### In Services

```csharp
public async Task<Client> AddFromRequest(ClientRequest request)
{
    var client = _mapper.Map<Client>(request);
    return await _service.Add(client);
}
```

---

## Best Practices

1. **Use AfterMap for complex logic** - Don't put business logic in mapping
2. **Create helper methods for reusable mappings** - Like `MapWalletIdentifierSummary`
3. **Handle null references** - Always check for null before accessing nested properties
4. **Use ForMember for explicit mappings** - Makes intent clear

---

## Related Documentation

- [SERVICE_LAYER_ARCHITECTURE.md](SERVICE_LAYER_ARCHITECTURE.md) - Where mappings are used
- [TRANSACTION_RESPONSE_VIEWMODELS.md](../03_CORE_SYSTEMS/TRANSACTION_RESPONSE_VIEWMODELS.md) - Transaction ViewModels

