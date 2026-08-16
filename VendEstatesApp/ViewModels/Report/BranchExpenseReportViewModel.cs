namespace VendEstatesApp.ViewModels.Report;

public class BranchExpenseReportViewModel
{
    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public List<Services.BranchExpenseSummary> Summaries { get; set; } = [];

    public decimal TotalAmount => Summaries.Sum(s => s.TotalAmount);

    public int TotalCount => Summaries.Sum(s => s.Count);
}
