namespace VendEstatesApp.Models.Enums;

public enum PaymentType
{
    BookingPayment,
    SalaryPayment,
    ExpensePayment,
    VehicleRentalPayment
}

public enum PaymentMethod
{
    Cash,
    BankTransfer,
    MobileMoney,
    Card
}

public enum PaymentStatus
{
    Pending,
    Approved,
    Rejected,
    Completed
}
