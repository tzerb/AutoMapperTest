namespace AutoMapperTest.Core.Services;

public interface IMyService
{
    Task<string> FormatFullName(string firstName, string lastName);
}
