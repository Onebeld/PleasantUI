namespace PleasantUI.Example.Models;

public class DataModel
{
    public string Name { get; set; }
    
    public int Age { get; set; }
    
    public bool IsNew { get; set; }

    public DataModel(string name, int age, bool isNew)
    {
        Name = name;
        Age = age;
        IsNew = isNew;
    }
}