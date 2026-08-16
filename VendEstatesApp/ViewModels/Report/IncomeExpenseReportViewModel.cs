namespace VendEstatesApp.ViewModels.Report;

public class IncomeExpenseReportViewModel
{
    public int Year { get; set; }

    public List<int> AvailableYears { get; set; } = [];

    public List<Services.IncomeExpenseReportRow> Rows { get; set; } = [];

    public decimal TotalIncome => Rows.Sum(r => r.Income);

    public decimal TotalExpenses => Rows.Sum(r => r.Expenses);

    public decimal TotalNet => Rows.Sum(r => r.Net);
}
