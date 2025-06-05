namespace SFManagement.ViewModels;

public class NicknameResponse : BaseResponse
{
    public string Name { get; set; }

    public Guid WalletId { get; set; }

    public Guid ClientId { get; set; }
}