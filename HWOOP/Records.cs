namespace LibrarySystem;

public class LoanRecord
{
    public int LoanId { get; }
    public LibraryItem Item { get; }
    public Member Member { get; }
    public DateTime BorrowDate { get; }
    public DateTime DueDate { get; }
    public DateTime? ReturnDate { get; private set; }
    public int LateDays { get; private set; }
    public decimal Fine { get; private set; }

    public bool IsReturned => ReturnDate.HasValue;

    public LoanRecord(int loanId, LibraryItem item, Member member, DateTime borrowDate)
    {
        LoanId = loanId;
        Item = item;
        Member = member;
        BorrowDate = borrowDate;
        DueDate = borrowDate.AddDays(item.GetLoanPeriod()); 
    }

    public bool IsOverdue(DateTime now) => !IsReturned && now.Date > DueDate.Date;

    public int CurrentLateDays(DateTime now) => IsOverdue(now) ? (now.Date - DueDate.Date).Days : 0;

    public void Close(DateTime returnDate)
    {
        ReturnDate = returnDate;
        LateDays = Math.Max(0, (returnDate.Date - DueDate.Date).Days);
        Fine = Item.CalculateLateFee(LateDays); 
    }

    public override string ToString() =>
        $"Loan#{LoanId} | {Item.Title} | {Member.Name} | Borrowed: {BorrowDate:yyyy-MM-dd} | Due: {DueDate:yyyy-MM-dd} | " +
        (IsReturned ? $"Returned: {ReturnDate:yyyy-MM-dd} | Late: {LateDays}d | Fine: {Fine:0.00}" : "Not returned");
}

public class Payment
{
    public int PaymentId { get; }
    public string Reference { get; }  
    public Member Member { get; }
    public decimal Amount { get; }
    public DateTime Date { get; }

    public Payment(int id, string reference, Member member, decimal amount)
    {
        PaymentId = id;
        Reference = reference;
        Member = member;
        Amount = amount;
        Date = DateTime.Now;
    }

    public override string ToString() =>
        $"Payment#{PaymentId} | Ref: {Reference} | {Member.Name} | {Amount:0.00} | {Date:yyyy-MM-dd HH:mm}";
}