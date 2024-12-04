namespace SFManagement.ViewModels
{
    public class ExcelResponse : BaseResponse
    {
        public Guid ManagerId { get; set; }

        public string? FileName { get; set; }

        public string? FileType { get; set; }
    }
}
