using Microsoft.EntityFrameworkCore;

namespace SFManagement.ViewModels
{
    public class ManagerResponse : BaseResponse
    {
        public string? Name { get; set; }
    }
}
