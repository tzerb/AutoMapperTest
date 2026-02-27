namespace AutoMapperTest.Core.Services
{
    public class Ctx : ICtx
    {
        public async Task<string> GetTimeZone(int providerId)
        {
            await Task.CompletedTask;
            return providerId == 0 ? "Central" : "Eastern";
        }
    }
    public interface ICtx
    {
        Task<string> GetTimeZone(int providerId);
    }
}