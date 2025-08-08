
using System.ComponentModel.DataAnnotations;

public class PortfolioUploadRequest
{
    [Required]
    public required IFormFile ApplicationForm { get; set; }          // Required: FMA

    [Required]
    public required IFormFile EquifaxCreditSearch { get; set; }      // Required: Equifax PDF

    public IFormFile? PortfolioForm { get; set; }            // Optional: KMC Portfolio Form

    public List<IFormFile>? ASTAgreements { get; set; }      // Optional: AST documents

    public List<IFormFile>? BankStatements { get; set; }     // Optional: Bank statements

    public List<IFormFile>? MortgageStatements { get; set; }  // Optional: Supporting docs
}