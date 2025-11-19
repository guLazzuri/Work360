namespace Work360.Infrastructure.Security
{
    public class ApiKeyConfig
    {
        public const string HeaderName = "X-Api-Key";
        public string Key { get; set; }
    }
}
