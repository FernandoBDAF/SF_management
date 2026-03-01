using SFManagement.Domain.Entities.Assets;
using SFManagement.Domain.Enums.Assets;

namespace SFManagement.Application.Helpers;

public static class FlexibleWalletTypes
{
    public static bool IsSystemWallet(WalletIdentifier wallet) =>
        wallet.AssetPool?.AssetGroup == AssetGroup.Flexible &&
        wallet.AssetPool?.BaseAssetHolderId == null;

    public static bool IsConversionWallet(WalletIdentifier wallet) =>
        wallet.AssetPool?.AssetGroup == AssetGroup.Flexible &&
        wallet.AssetPool?.BaseAssetHolderId != null;

    public static bool IsFlexibleWallet(WalletIdentifier wallet) =>
        wallet.AssetPool?.AssetGroup == AssetGroup.Flexible;
}
