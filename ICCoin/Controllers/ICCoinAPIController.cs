using Binance.Net.Clients;
using Binance.Net.SymbolOrderBooks;
using ICCoin.Helper;
using Microsoft.AspNetCore.Mvc;
using Binance.Net.Enums;
using CryptoExchange.Net.CommonObjects;

namespace ICCoin.Controllers;

[ApiController]
[Route("[controller]")]
public class ICCoinAPIController : ControllerBase
{
    private readonly ILogger<ICCoinAPIController> _logger;
    private readonly string _apiKey;
    private readonly string _secretKey;

    public ICCoinAPIController(ILogger<ICCoinAPIController> logger)
    {
        _logger = logger;
        _apiKey = AppSettings.apiKey;
        _secretKey = AppSettings.secretKey;
    }

    [HttpGet("getBalances")]
    public async Task<IActionResult> getBalances(string? pAsset)
    {
        //var optionsApiCredentials = new ApiCredentials(_apiKey, _secretKey); 

        using (var binanceClient = new BinanceRestClient())
        {
            var balance = await binanceClient.SpotApi.Account.GetBalancesAsync(pAsset);

            if (!balance.Success)
            {
                return BadRequest(balance.Error);
            }

            return Ok(balance.Data);
        }
    }

    [HttpGet("getOrderBook")]
    public IActionResult getOrderBook()
    {
        //var book = new BinanceSpotSymbolOrderBook("ETHUSDT");
        var book = new BinanceSpotSymbolOrderBook("ETHUSDT");
        var startResult = book.StartAsync();
        if (!startResult.Result.Success)
        {
            // Handle error, error info available in startResult.Error
        }
        // Book has successfully started and synchronized

        // Once no longer needed you can stop the live sync functionality by calling StopAsync()
        Thread.Sleep(10000);
        book.StopAsync();

        return new JsonResult(startResult.Result.Data);
    }

    [HttpGet("getTicker")]
    public IActionResult getTicker(string SYMBOL)
    {
        var client = new BinanceRestClient();
        var tickersResult = client.SpotApi.ExchangeData.GetTickerAsync(SYMBOL); //.GetTickersAsync();
        if (!tickersResult.Result.Success)
        {
            // Handle error, tickersResult.Error contains more information
        }
        else
        {
            // Handle data, tickersResult.Data will contain the actual data
        }

        //var spotSharedRestClients = client.SpotApi.SharedClient;


        return new JsonResult(tickersResult.Result.Data);
    }

    [HttpPost("postPlaceOrder")]
    public async Task<IActionResult> postPlaceOrder(string? SYMBOL)
    {
        /* CENGIZ
         * // burdan alm�yor. program.cs ye eklendi!!
        //var optionsApiCredentials = new ApiCredentials(_apiKey, _secretKey); 

        using (var binanceClient = new BinanceRestClient())
        {
            //binanceClient.ClientOptions.ApiCredentials = optionsApiCredentials;
            var sonuc = await binanceClient.SpotApi.Trading.PlaceOrderAsync("BAKEUSDT", OrderSide.Buy, SpotOrderType.Limit, 0.1m, price: 50000, timeInForce: TimeInForce.GoodTillCanceled);


            if (!sonuc.Success)
            {
                return BadRequest(sonuc.Error);
            }

            return Ok(sonuc.Data);
        }

        CENGIZ */
        // burdan almýyor. program.cs ye eklendi!!
        //var optionsApiCredentials = new ApiCredentials(_apiKey, _secretKey); 

        using (var binanceClient = new BinanceRestClient())
        {
            //binanceClient.ClientOptions.ApiCredentials = optionsApiCredentials;
            var sonucallopens = await binanceClient.SpotApi.Trading.GetOpenOrdersAsync("BTCUSDT");
            string? newClientOrderId = null;
            long? orderId = null;
            var sonuc = await binanceClient.SpotApi.Trading.PlaceOrderAsync("BAKEUSDT", OrderSide.Buy, SpotOrderType.Limit, quantity: 20, price: Decimal.Parse("0.3100")
                , timeInForce: TimeInForce.GoodTillCanceled, newClientOrderId: newClientOrderId);

            var sonucopens = await binanceClient.SpotApi.Trading.GetOpenOrdersAsync("BTCUSDT");
            orderId = 1274034100; //1274012796;
            try
            {
                var sonuc2 = await binanceClient.SpotApi.Trading.CancelOrderAsync("BAKEUSDT", orderId: orderId);
                //int c = 1;
            }
            catch (Exception pExc)
            {
                Exception exc = pExc;
            }
            
            var sonuc3 = await binanceClient.SpotApi.Trading.PlaceOrderAsync("BAKEUSDT", OrderSide.Buy, SpotOrderType.Limit, quantity: 20, price: Decimal.Parse("0.3160")
                , timeInForce: TimeInForce.GoodTillCanceled, newClientOrderId: newClientOrderId);


            if (!sonuc.Success)
            {
                return BadRequest(sonuc.Error);
            }

            return Ok(sonuc.Data);
        }
    }
}
