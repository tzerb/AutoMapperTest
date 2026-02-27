namespace AutoMapperTest.Core.Services;

public class MyService : IMyService
{
    ICtx _ctx;

    public MyService(ICtx ctx)
    {
        _ctx = ctx;
    }
    public async Task<string> FormatFullName(string firstName, string lastName)
    {
        var timezone = await _ctx.GetTimeZone(0);
        return $"{lastName}, {firstName} : {timezone}";
    }
}
