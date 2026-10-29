namespace LibrarySystem;


public abstract class Person
{
    public int Id { get; }
    public string Name { get; set; }
    public string Email { get; set; }

    protected Person(int id, string name, string email)
    {
        Id = id;
        Name = name;
        Email = email;
    }
}

public class Member : Person
{
    public int MaxLoans { get; set; }         
    public bool IsActive { get; set; } = true; 
    public DateTime JoinDate { get; } = DateTime.Now;

    // Constructor Overloading
    public Member(int id, string name, string email) : this(id, name, email, 3) { }

    public Member(int id, string name, string email, int maxLoans) : base(id, name, email)
    {
        MaxLoans = maxLoans;
    }

    public override string ToString() =>
        $"[{Id}] {Name} | {Email} | MaxLoans={MaxLoans} | {(IsActive ? "Active" : "Disabled")}";
}

public class Librarian : Person
{
    public string Username { get; }
    private string _password;

    public Librarian(int id, string name, string email, string username, string password)
        : base(id, name, email)
    {
        Username = username;
        _password = password;
    }

    public bool CheckCredentials(string username, string password) =>
        Username == username && _password == password;
}