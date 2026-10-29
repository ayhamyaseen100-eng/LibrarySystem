namespace LibrarySystem;

public class AuthService
{
    private readonly List<Librarian> _librarians = new()
    {
        new Librarian(1, "Admin Librarian", "admin@lib.com", "admin", "admin123")
    };

    public Librarian? Login(string username, string password) =>
        _librarians.FirstOrDefault(l => l.CheckCredentials(username, password));
}

public class ItemService
{
 
    private readonly List<LibraryItem> _items = new();
    private int _nextId = 1;

    public IReadOnlyList<LibraryItem> All => _items;
    public int NextId() => _nextId++;

    public void Add(LibraryItem item) => _items.Add(item);

    
    public LibraryItem? Find(int itemId) => _items.FirstOrDefault(i => i.ItemId == itemId);

    public List<LibraryItem> Find(string title) =>
        _items.Where(i => i.Title.Contains(title, StringComparison.OrdinalIgnoreCase)).ToList();

    public List<LibraryItem> Available() => _items.Where(i => i.IsAvailable).ToList();

    public bool Disable(int itemId, out string message)
    {
        var item = Find(itemId);
        if (item == null) { message = "Item not found."; return false; }
        if (item.IsBorrowed) { message = "Cannot disable an item that is currently borrowed."; return false; }
        item.IsActive = false;
        message = "Item removed from circulation (history kept).";
        return true;
    }

    public bool Enable(int itemId)
    {
        var item = Find(itemId);
        if (item == null) return false;
        item.IsActive = true;
        return true;
    }
}

public class MemberService
{
    private readonly List<Member> _members = new();
    private int _nextId = 1001;

    public IReadOnlyList<Member> All => _members;

    public Member Register(string name, string email)
    {
        var m = new Member(_nextId++, name, email);
        _members.Add(m);
        return m;
    }

    public Member Register(string name, string email, int maxLoans)
    {
        var m = new Member(_nextId++, name, email, maxLoans);
        _members.Add(m);
        return m;
    }

    public Member? Find(int memberId) => _members.FirstOrDefault(m => m.Id == memberId);

    public List<Member> Find(string name) =>
        _members.Where(m => m.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();
}

public class FineService
{
    private readonly List<LoanRecord> _loans;       
    private readonly List<Payment> _payments = new();
    private readonly HashSet<string> _references = new();
    private int _nextPaymentId = 1;

    public FineService(List<LoanRecord> loans) { _loans = loans; }

    public IReadOnlyList<Payment> Payments => _payments;

    public decimal TotalFines(Member m) => _loans.Where(l => l.Member.Id == m.Id).Sum(l => l.Fine);
    public decimal TotalPaid(Member m) => _payments.Where(p => p.Member.Id == m.Id).Sum(p => p.Amount);
    public decimal Outstanding(Member m) => TotalFines(m) - TotalPaid(m);
    public decimal TotalCollected() => _payments.Sum(p => p.Amount);

  
    public bool Pay(Member m, decimal amount, out string message) =>
        Pay(m, amount, $"AUTO-{Guid.NewGuid():N}".Substring(0, 13), out message);

    public bool Pay(Member m, decimal amount, string reference, out string message)
    {
        if (amount <= 0) { message = "Amount must be positive."; return false; }
        if (_references.Contains(reference)) { message = "Duplicate payment: this reference was already recorded."; return false; }
        if (amount > Outstanding(m)) { message = $"Amount exceeds outstanding fines ({Outstanding(m):0.00})."; return false; }

        _references.Add(reference);
        _payments.Add(new Payment(_nextPaymentId++, reference, m, amount));
        message = "Payment recorded.";
        return true;
    }
}

public class LoanService
{
    private readonly List<LoanRecord> _loans;
    private readonly ItemService _items;
    private readonly MemberService _members;
    private readonly FineService _fines;
    private int _nextLoanId = 1;

    public LoanService(List<LoanRecord> loans, ItemService items, MemberService members, FineService fines)
    {
        _loans = loans; _items = items; _members = members; _fines = fines;
    }

    public IReadOnlyList<LoanRecord> Loans => _loans;

    public int ActiveCount(Member m) => _loans.Count(l => l.Member.Id == m.Id && !l.IsReturned);

   
    public bool Borrow(int memberId, int itemId, out string message)
    {
        var member = _members.Find(memberId);
        var item = _items.Find(itemId);
        if (member == null) { message = "Member not found."; return false; }
        if (item == null) { message = "Item not found."; return false; }
        return Borrow(member, item, out message);
    }

    public bool Borrow(Member member, LibraryItem item, out string message)
    {
        if (!member.IsActive) { message = "Membership is disabled."; return false; }
        if (!item.IsActive) { message = "Item is out of circulation."; return false; }
        if (item.IsBorrowed) { message = "Item is currently borrowed."; return false; }
        if (ActiveCount(member) >= member.MaxLoans) { message = $"Loan limit reached ({member.MaxLoans})."; return false; }
        if (_fines.Outstanding(member) > 0) { message = $"Member has unpaid fines: {_fines.Outstanding(member):0.00}"; return false; }

        var loan = new LoanRecord(_nextLoanId++, item, member, DateTime.Now);
        _loans.Add(loan);
        item.IsBorrowed = true;
        message = $"Borrowed successfully. Due date: {loan.DueDate:yyyy-MM-dd}";
        return true;
    }

    public bool Return(int itemId, out string message) => Return(itemId, DateTime.Now, out message);

 
    public bool Return(int itemId, DateTime returnDate, out string message)
    {
        var loan = _loans.FirstOrDefault(l => l.Item.ItemId == itemId && !l.IsReturned);
        if (loan == null) { message = "No active loan for this item."; return false; }

        loan.Close(returnDate);
        loan.Item.IsBorrowed = false;
        message = loan.LateDays > 0
            ? $"Returned late by {loan.LateDays} day(s). Fine: {loan.Fine:0.00}"
            : "Returned on time. No fine.";
        return true;
    }

    public List<LoanRecord> CurrentLoans(Member m) => _loans.Where(l => l.Member.Id == m.Id && !l.IsReturned).ToList();
    public List<LoanRecord> History(Member m) => _loans.Where(l => l.Member.Id == m.Id).ToList();
}

public class ReportService
{
    private readonly ItemService _items;
    private readonly MemberService _members;
    private readonly LoanService _loans;
    private readonly FineService _fines;

    public ReportService(ItemService items, MemberService members, LoanService loans, FineService fines)
    {
        _items = items; _members = members; _loans = loans; _fines = fines;
    }

    public void AvailableAndBorrowed()
    {
        Console.WriteLine("--- Available ---");
        foreach (var i in _items.All.Where(i => i.IsAvailable)) Console.WriteLine(i);
        Console.WriteLine("--- Borrowed ---");
        foreach (var i in _items.All.Where(i => i.IsBorrowed)) Console.WriteLine(i);
    }

    public void MostBorrowed()
    {
        var top = _loans.Loans.GroupBy(l => l.Item)
            .Select(g => new { Item = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count).Take(5);
        foreach (var x in top) Console.WriteLine($"{x.Count} loan(s) -> {x.Item.Title}");
    }

    public void OverdueMembers()
    {
        var now = DateTime.Now;
        foreach (var l in _loans.Loans.Where(l => l.IsOverdue(now)))
        {
            int late = l.CurrentLateDays(now);
            Console.WriteLine($"{l.Member.Name} (#{l.Member.Id}) | {l.Item.Title} | Late {late}d | Expected fine: {l.Item.CalculateLateFee(late):0.00}");
        }
    }

    public void MembersWithUnpaidFines()
    {
        foreach (var m in _members.All.Where(m => _fines.Outstanding(m) > 0))
            Console.WriteLine($"{m.Name} (#{m.Id}) owes {_fines.Outstanding(m):0.00}");
    }

    public void TotalCollected() => Console.WriteLine($"Total collected fines: {_fines.TotalCollected():0.00}");

    public void MemberHistory(Member m)
    {
        foreach (var l in _loans.History(m)) Console.WriteLine(l);
        Console.WriteLine($"Fines total: {_fines.TotalFines(m):0.00} | Paid: {_fines.TotalPaid(m):0.00} | Outstanding: {_fines.Outstanding(m):0.00}");
    }
}