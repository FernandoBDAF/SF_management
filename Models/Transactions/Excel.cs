using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SFManagement.Models.Entities;

namespace SFManagement.Models.Transactions;

public class Excel : BaseDomain
{
    // public Excel(Guid managerId, string fileName, string fileType)
    // {
    //     ManagerId = managerId;
    //     FileName = fileName;
    //     FileType = fileType;
    //     ExcelTransactions = new List<ExcelTransaction>();
    // }
    
    public Guid PokerManagerId { get; set; }
    public virtual PokerManager PokerManager { get; set; } = new PokerManager();

    [MaxLength(40)] public string? FileName { get; set; }

    [MaxLength(40)] public string? FileType { get; set; }

    public ICollection<ExcelTransaction> ExcelTransactions { get; set; } = new List<ExcelTransaction>();
}