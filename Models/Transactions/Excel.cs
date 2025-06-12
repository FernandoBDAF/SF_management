using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFManagement.Models.Transactions;

public class Excel : BaseDomain
{
    public Excel(Guid managerId, string fileName, string fileType)
    {
        ManagerId = managerId;
        FileName = fileName;
        FileType = fileType;
        ExcelTransactions = new List<ExcelTransaction>();
    }
    
    [Required] [ForeignKey("Manager")] public Guid ManagerId { get; set; }

    public virtual Manager Manager { get; set; } = new();

    [Required] [MaxLength(20)] public string FileName { get; set; }

    [Required] [MaxLength(20)] public string FileType { get; set; }

    public ICollection<ExcelTransaction> ExcelTransactions { get; set; }
}