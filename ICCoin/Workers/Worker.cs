using Data.Models;
using ICCoin.Helper;

namespace ICCoin.Workers
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly HttpClient _httpClient;
        //private readonly string _connectionString = "";
        private ICContext _dbContext;
        private readonly string _apiKey;
        private readonly string _secretKey;
        public Worker(ILogger<Worker> logger, IHttpClientFactory httpClientFactory, ICContext dbContext)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
            _dbContext = dbContext;

            _apiKey = AppSettings.apiKey;
            _secretKey = AppSettings.secretKey;
            // Düzeltilmiş yapılandırma
            //_client = new BinanceClient(new ClientConfiguration
            //{
            //    ApiKey = "",
            //    SecretKey = ""
            //});
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await binanceBuySell();

                await Task.Delay(10000, stoppingToken); // 10 saniye bekle
                Console.WriteLine(DateTime.Now.ToString());
            }
        }

        protected async Task<bool> binanceBuySell()
        {
            // zaman kilidi için bu satırı aç : 
            //var emirler = _dbContext.ORDERs.Where(x => new[] { "NEW" }.Contains(x.STATUS) && x.INSERT_DATETIME >= DateTime.Now.AddMinutes(-5)).ToList();
            var emirler = _dbContext.ORDERs.Where(x => new[] { "NEW" }.Contains(x.STATUS) ).ToList();

            foreach (var order in emirler)
            {
                string quantity = order.QUANTITY.ToString().Replace(',', '.');
                
                var orderSonuc = await BinanceHelper.OrderSPOT(order.SYMBOL, order.ORDER_SIDE, quantity, 0, "MARKET", "");

                if(orderSonuc.IsSuccess)
                {
                    order.STATUS = order.STATUS+"_"+"SUCCESS";
                }
                else
                {
                    order.STATUS = order.STATUS + "_" + "ERROR";
                }

                order.ORDER_RESULT_JSON = orderSonuc.ResBody;
                _dbContext.SaveChanges();

            }

            return true;
        }
    }
}
