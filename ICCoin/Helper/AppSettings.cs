namespace ICCoin.Helper
{
    public static class AppSettings
    {
        private static IConfiguration _configuration;
        
        static AppSettings()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
                
            _configuration = builder.Build();
            
            apiKey = _configuration["AppSettings:apiKey"]??"";
            secretKey = _configuration["AppSettings:SecretKey"]??"";
            endPoint = _configuration["AppSettings:EndPoint"] ?? "";
            ConnectionString = _configuration.GetConnectionString("sql_coin")??"";
        }

        public static string ConnectionString { get; private set; } = string.Empty;
        public static string apiKey { get; private set; } = string.Empty;
        public static string secretKey { get; private set; } = string.Empty;
        public static string endPoint { get; private set; } = string.Empty;
    }
}
