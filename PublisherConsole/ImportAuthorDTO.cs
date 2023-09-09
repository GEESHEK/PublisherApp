namespace PublisherConsole;

public class ImportAuthorDTO
{
    public ImportAuthorDTO(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public string FirstName { get; set; }
    public string LastName { get; set; }
    
    // public ImportAuthorDTO(string firstName, string lastName)
    // {
    //     _firstName = firstName;
    //     _lastName = lastName;
    // }
    //
    // private string _firstName;
    // private string _lastName;
    // private string FirstName => _firstName;
    // private string LastName => _lastName;
}