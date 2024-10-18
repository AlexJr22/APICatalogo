namespace APICatalogo.RateLimitOptions
{
    public class MyRateLimitOptions
    {
        public const string RateLimitParameters = "RateLimitParameters";
        public int PermitLimit { get; set; } = 2;
        public int Window { get; set; } = 2;
        public int ReplanishmentPeriod { get; set; } = 2;
        public int QueueLimit { get; set; } =2;
        public int SugmentPerWindow { get; set; } =2;
        public int TokenLimit { get; set; } =2;
        public int TokenLimit2 { get; set; } =2;
        public int TokenPerPeriod { get; set; } =2;
        public bool AutoRepletiment { get; set; } = false;
    }
}
