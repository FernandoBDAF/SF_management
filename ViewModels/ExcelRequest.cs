namespace SFManagement.ViewModels
{
    public class ExcelRequest
    {
        public Guid ManagerId { get; set; }

        public IFormFile PostFile { get; set; }
    }
}
