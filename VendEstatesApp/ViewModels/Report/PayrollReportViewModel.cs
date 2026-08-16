namespace VendEstatesApp.ViewModels.Report;

public class PayrollReportViewModel
{
    public int Month { get; set; }

    public int Year { get; set; }

    public List<Models.Payroll> Payrolls { get; set; } = [];

    public decimal TotalGrossSalary => Payrolls.Sum(p => p.GrossSalary);

    public decimal TotalDeductions => Payrolls.Sum(p => p.TotalDeductions);

    public decimal TotalNetSalary => Payrolls.Sum(p => p.NetSalary);
}
