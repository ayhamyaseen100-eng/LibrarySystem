namespace LibrarySystem;


public abstract class LibraryItem
{
    public int ItemId { get; }
    public string Title { get; set; }
    public int Year { get; set; }
    public bool IsActive { get; set; } = true;    // false = خارج التداول (بدون حذف)
    public bool IsBorrowed { get; set; } = false;

    protected LibraryItem(int itemId, string title, int year)
    {
        ItemId = itemId;
        Title = title;
        Year = year;
    }

    public bool IsAvailable => IsActive && !IsBorrowed;
    public string Status => !IsActive ? "Disabled" : IsBorrowed ? "Borrowed" : "Available";

    
    public abstract int GetLoanPeriod();                    // مدة الاستعارة بالأيام
    public abstract decimal CalculateLateFee(int daysLate); // سياسة الغرامة
    public abstract string TypeName { get; }

   
    public virtual string GetDetails() =>
        $"ID: {ItemId} | Type: {TypeName} | Title: {Title} | Year: {Year} | Status: {Status}";

 
    public virtual IEnumerable<string> EditableFields => new[] { "Title", "Year" };

    public virtual bool SetField(string field, string value)
    {
        switch (field.ToLower())
        {
            case "title": Title = value; return true;
            case "year":
                if (int.TryParse(value, out int y)) { Year = y; return true; }
                return false;
            default: return false;
        }
    }

    public override string ToString() => GetDetails();
}

public class Book : LibraryItem
{
    public string Author { get; set; }
    public int Pages { get; set; }

    public Book(int id, string title, int year, string author, int pages) : base(id, title, year)
    {
        Author = author;
        Pages = pages;
    }

    public override string TypeName => "Book";
    public override int GetLoanPeriod() => 14;
    public override decimal CalculateLateFee(int daysLate) => daysLate * 1.0m;

    public override string GetDetails() => base.GetDetails() + $" | Author: {Author} | Pages: {Pages}";

    public override IEnumerable<string> EditableFields => base.EditableFields.Concat(new[] { "Author", "Pages" });

    public override bool SetField(string field, string value)
    {
        switch (field.ToLower())
        {
            case "author": Author = value; return true;
            case "pages":
                if (int.TryParse(value, out int p)) { Pages = p; return true; }
                return false;
            default: return base.SetField(field, value);
        }
    }
}

public class Magazine : LibraryItem
{
    public int IssueNumber { get; set; }

    public Magazine(int id, string title, int year, int issueNumber) : base(id, title, year)
    {
        IssueNumber = issueNumber;
    }

    public override string TypeName => "Magazine";
    public override int GetLoanPeriod() => 7;
    public override decimal CalculateLateFee(int daysLate) => daysLate * 0.5m;

    public override string GetDetails() => base.GetDetails() + $" | Issue: {IssueNumber}";

    public override IEnumerable<string> EditableFields => base.EditableFields.Concat(new[] { "IssueNumber" });

    public override bool SetField(string field, string value)
    {
        if (field.Equals("IssueNumber", StringComparison.OrdinalIgnoreCase))
        {
            if (int.TryParse(value, out int n)) { IssueNumber = n; return true; }
            return false;
        }
        return base.SetField(field, value);
    }
}

public class ResearchPaper : LibraryItem
{
    public string Researcher { get; set; }
    public string Field { get; set; }

    public ResearchPaper(int id, string title, int year, string researcher, string field)
        : base(id, title, year)
    {
        Researcher = researcher;
        Field = field;
    }

    public override string TypeName => "ResearchPaper";
    public override int GetLoanPeriod() => 3;
 
    public override decimal CalculateLateFee(int daysLate) => daysLate <= 0 ? 0m : 2.0m + daysLate * 2.0m;

    public override string GetDetails() => base.GetDetails() + $" | Researcher: {Researcher} | Field: {Field}";

    public override IEnumerable<string> EditableFields => base.EditableFields.Concat(new[] { "Researcher", "Field" });

    public override bool SetField(string field, string value)
    {
        switch (field.ToLower())
        {
            case "researcher": Researcher = value; return true;
            case "field": Field = value; return true;
            default: return base.SetField(field, value);
        }
    }
}