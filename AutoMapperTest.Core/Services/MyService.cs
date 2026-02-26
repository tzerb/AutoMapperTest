namespace AutoMapperTest.Core.Services;

public class MyService : IMyService
{
    ICtx _ctx;

    public MyService(ICtx ctx)
    {
        _ctx = ctx;
    }
    public string FormatFullName(string firstName, string lastName)
    {
        var timezone = _ctx.GetTimeZone(0);
        return $"{lastName}, {firstName} : {timezone}";
    }
}
