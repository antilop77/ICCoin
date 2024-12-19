namespace ICCoin.Helper
{
    public class BinanceHttpResponse
    {
        public bool IsSuccess { get; set; } = false;
        public dynamic? Result { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ReqPath { get; set; } = string.Empty;
        public string ReqBody { get; set; } = string.Empty;
        public string ResBody { get; set; } = string.Empty;
    }
}
