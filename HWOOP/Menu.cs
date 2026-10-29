namespace LibrarySystem;

public class Menu
{
    private readonly AuthService _auth = new();
    private readonly ItemService _items = new();
    private readonly MemberService _members = new();
    private readonly LoanService _loans;
    private readonly FineService _fines;
    private readonly ReportService _reports;

    public Menu()
    {
        var shared = new List<LoanRecord>();
        _fines = new FineService(shared);
        _loans = new LoanService(shared, _items, _members, _fines);
        _reports = new ReportService(_items, _members, _loans, _fines);
        SeedData();
    }

    private void SeedData()
    {
        _items.Add(new Book(_items.NextId(), "Clean Code", 2008, "Robert Martin", 464));
        _items.Add(new Magazine(_items.NextId(), "National Geographic", 2024, 12));
        _items.Add(new ResearchPaper(_items.NextId(), "Deep Learning Survey", 2022, "Dr. Ahmad", "AI"));
        _members.Register("Sara Ali", "sara@mail.com");
    }

    
    private static string Read(string prompt) { Console.Write(prompt); return Console.ReadLine()?.Trim() ?? ""; }
    private static int ReadInt(string prompt) { while (true) { if (int.TryParse(Read(prompt), out int v)) return v; Console.WriteLine("Invalid number."); } }
    private static decimal ReadDecimal(string prompt) { while (true) { if (decimal.TryParse(Read(prompt), out decimal v)) return v; Console.WriteLine("Invalid number."); } }
    private static void Pause() { Console.WriteLine("\nPress Enter..."); Console.ReadLine(); }
    private static void Print<T>(IEnumerable<T> list) { bool any = false; foreach (var x in list) { Console.WriteLine(x); any = true; } if (!any) Console.WriteLine("(none)"); }

    
    public void Run()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("===== Library Management System =====\n");
            Console.WriteLine("1. Login\n2. Register New Member\n3. Browse Library\n4. Search for Items\n5. Exit\n");
            switch (Read("Choose: "))
            {
                case "1": Login(); break;
                case "2": RegisterMember(); Pause(); break;
                case "3": Print(_items.All.Where(i => i.IsActive)); Pause(); break;
                case "4": SearchItems(); Pause(); break;
                case "5": return;
            }
        }
    }

    private void Login()
    {
        Console.WriteLine("1. Librarian\n2. Member");
        string c = Read("Choose: ");
        if (c == "1")
        {
            var lib = _auth.Login(Read("Username: "), Read("Password: "));
            if (lib == null) { Console.WriteLine("Invalid credentials."); Pause(); return; }
            LibrarianMenu(lib);
        }
        else if (c == "2")
        {
            var m = _members.Find(ReadInt("Member ID: "));
            if (m == null || !m.IsActive) { Console.WriteLine("Member not found or disabled."); Pause(); return; }
            MemberMenu(m);
        }
    }

    private void RegisterMember()
    {
        var m = _members.Register(Read("Name: "), Read("Email: "));
        Console.WriteLine($"Registered. Your Member ID = {m.Id}");
    }

    private void SearchItems()
    {
        string q = Read("Enter item ID or title: ");
        if (int.TryParse(q, out int id)) { var i = _items.Find(id); Console.WriteLine(i?.ToString() ?? "(not found)"); }
        else Print(_items.Find(q));
    }

    private void MemberMenu(Member m)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine($"=== Member Menu: {m.Name} ===");
            Console.WriteLine("1. Browse available items\n2. Search items\n3. My current loans\n4. My loan history\n5. My fines\n6. Logout");
            switch (Read("Choose: "))
            {
                case "1": Print(_items.Available()); break;
                case "2": SearchItems(); break;
                case "3": Print(_loans.CurrentLoans(m)); break;
                case "4": Print(_loans.History(m)); break;
                case "5": Console.WriteLine($"Total: {_fines.TotalFines(m):0.00} | Paid: {_fines.TotalPaid(m):0.00} | Due: {_fines.Outstanding(m):0.00}"); break;
                case "6": return;
            }
            Pause();
        }
    }

   
    private void LibrarianMenu(Librarian lib)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine($"=== Librarian Menu: {lib.Name} ===");
            Console.WriteLine("-- Items --\n 1. Add item\n 2. List all items\n 3. Search item\n 4. Available items\n 5. Item details\n 6. Edit item\n 7. Disable item\n 8. Enable item");
            Console.WriteLine("-- Members --\n 9. Register member\n10. List members / find by ID\n11. Member details & history\n12. Set member loan limit\n13. Disable/Enable membership");
            Console.WriteLine("-- Loans & Fines --\n14. Borrow item\n15. Return item\n16. Pay fine\n17. All loans");
            Console.WriteLine("-- Reports --\n18. Available & borrowed\n19. Most borrowed\n20. Overdue members\n21. Members with unpaid fines\n22. Total collected fines\n23. Member history\n\n 0. Logout");
            switch (Read("Choose: "))
            {
                case "1": AddItem(); break;
                case "2": Print(_items.All); break;
                case "3": SearchItems(); break;
                case "4": Print(_items.Available()); break;
                case "5": { var i = _items.Find(ReadInt("Item ID: ")); Console.WriteLine(i?.GetDetails() ?? "Not found."); break; }
                case "6": EditItem(); break;
                case "7": { _items.Disable(ReadInt("Item ID: "), out string msg); Console.WriteLine(msg); break; }
                case "8": Console.WriteLine(_items.Enable(ReadInt("Item ID: ")) ? "Enabled." : "Not found."); break;
                case "9": RegisterMember(); break;
                case "10": FindMembers(); break;
                case "11": MemberDetails(); break;
                case "12": SetLimit(); break;
                case "13": ToggleMember(); break;
                case "14": { _loans.Borrow(ReadInt("Member ID: "), ReadInt("Item ID: "), out string msg); Console.WriteLine(msg); break; }
                case "15": { _loans.Return(ReadInt("Item ID: "), out string msg); Console.WriteLine(msg); break; }
                case "16": PayFine(); break;
                case "17": Print(_loans.Loans); break;
                case "18": _reports.AvailableAndBorrowed(); break;
                case "19": _reports.MostBorrowed(); break;
                case "20": _reports.OverdueMembers(); break;
                case "21": _reports.MembersWithUnpaidFines(); break;
                case "22": _reports.TotalCollected(); break;
                case "23": { var m = _members.Find(ReadInt("Member ID: ")); if (m == null) Console.WriteLine("Not found."); else _reports.MemberHistory(m); break; }
                case "0": return;
            }
            Pause();
        }
    }

    private void AddItem()
    {
        Console.WriteLine("1. Book  2. Magazine  3. ResearchPaper");
        string t = Read("Type: ");
        string title = Read("Title: ");
        int year = ReadInt("Year: ");
        int id = _items.NextId();
        switch (t)
        {
            case "1": _items.Add(new Book(id, title, year, Read("Author: "), ReadInt("Pages: "))); break;
            case "2": _items.Add(new Magazine(id, title, year, ReadInt("Issue number: "))); break;
            case "3": _items.Add(new ResearchPaper(id, title, year, Read("Researcher: "), Read("Field: "))); break;
            default: Console.WriteLine("Invalid type."); return;
        }
        Console.WriteLine($"Added with ID {id}.");
    }

    private void EditItem()
    {
        var item = _items.Find(ReadInt("Item ID: "));
        if (item == null) { Console.WriteLine("Not found."); return; }
        Console.WriteLine("Editable: " + string.Join(", ", item.EditableFields));
        string field = Read("Field: ");
        Console.WriteLine(item.SetField(field, Read("New value: ")) ? "Updated." : "Invalid field or value.");
    }

    private void FindMembers()
    {
        string q = Read("Member ID (or Enter to list all): ");
        if (q == "") Print(_members.All);
        else if (int.TryParse(q, out int id)) Console.WriteLine(_members.Find(id)?.ToString() ?? "Not found.");
    }

    private void MemberDetails()
    {
        var m = _members.Find(ReadInt("Member ID: "));
        if (m == null) { Console.WriteLine("Not found."); return; }
        Console.WriteLine(m);
        _reports.MemberHistory(m);
    }

    private void SetLimit()
    {
        var m = _members.Find(ReadInt("Member ID: "));
        if (m == null) { Console.WriteLine("Not found."); return; }
        m.MaxLoans = ReadInt("New max loans: ");
        Console.WriteLine("Updated.");
    }

    private void ToggleMember()
    {
        var m = _members.Find(ReadInt("Member ID: "));
        if (m == null) { Console.WriteLine("Not found."); return; }
        m.IsActive = !m.IsActive;
        Console.WriteLine(m.IsActive ? "Membership enabled." : "Membership disabled.");
    }

    private void PayFine()
    {
        var m = _members.Find(ReadInt("Member ID: "));
        if (m == null) { Console.WriteLine("Not found."); return; }
        Console.WriteLine($"Outstanding: {_fines.Outstanding(m):0.00}");
        decimal amount = ReadDecimal("Amount: ");
        string reference = Read("Receipt reference (Enter = auto): ");
        bool ok = reference == ""
            ? _fines.Pay(m, amount, out string msg)
            : _fines.Pay(m, amount, reference, out msg);
        Console.WriteLine(msg);
    }
}