namespace AutoMapperTest.Core.Services;

public class MyService : IMyService
{
    public string FormatFullName(string firstName, string lastName)
    {
        return $"{lastName}, {firstName}";
    }
}
