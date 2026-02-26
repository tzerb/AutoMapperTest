namespace AutoMapperTest.Core.Services
{
    public class Ctx : ICtx
    {
        public string GetTimeZone(int providerId)
        {

            return providerId == 0 ? "Central" : "Eastern";
        }
    }
    public interface ICtx
    {
        string GetTimeZone(int providerId);
    }
}