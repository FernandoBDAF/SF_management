using SFManagement.Models.Entities;

namespace SFManagement.ViewModels;

public class BankResponse : BaseAssetHolderResponse
{
    public string? Code { get; set; }
    
    // Remove redundant collections - these should be accessed through separate endpoints
    // Ofxs collection creates circular references and performance issues in responses
    // Use dedicated endpoints for OFX data instead
}