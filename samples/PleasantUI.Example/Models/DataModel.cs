namespace PleasantUI.Example.Models;

public class DataModel
{
    public string Name { get; set; }
    public string Department { get; set; }
    public int Age { get; set; }
    public double Salary { get; set; }
    public bool IsNew { get; set; }
    public string Status { get; set; }

    public DataModel(string name, string department, int age, double salary, bool isNew, string status)
    {
        Name = name;
        Department = department;
        Age = age;
        Salary = salary;
        IsNew = isNew;
        Status = status;
    }
}
