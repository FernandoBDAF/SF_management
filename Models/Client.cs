using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SFManagement.Models.Transactions;

namespace SFManagement.Models.Entities;

public class Client : BaseAssetHolder
{
    public DateTime? Birthday { get; set; }
}