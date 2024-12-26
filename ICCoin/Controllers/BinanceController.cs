using Binance.Net.Clients;
using Binance.Net.SymbolOrderBooks;
using ICCoin.Helper;
using Microsoft.AspNetCore.Mvc;
using Binance.Net.Enums;
using CryptoExchange.Net.CommonObjects;

namespace ICCoin.Controllers;

[ApiController]
[Route("[controller]")]
public class BinanceController : ControllerBase
{
    private readonly ILogger<BinanceController> _logger;
    private readonly string _apiKey;
    private readonly string _secretKey;

    public BinanceController(ILogger<BinanceController> logger)
    {
        _logger = logger;
        _apiKey = AppSettings.apiKey;
        _secretKey = AppSettings.secretKey;
    }

    [HttpGet("GetHello")]
    public IActionResult Get()
    {
        var client = new BinanceRestClient();
        var tickersResult = client.SpotApi.ExchangeData.GetTickersAsync();
        if (!tickersResult.Result.Success)
        {
            // Handle error, tickersResult.Error contains more information
        }
        else
        {
            // Handle data, tickersResult.Data will contain the actual data
        }

        var spotSharedRestClients = client.SpotApi.SharedClient;


        return new JsonResult(tickersResult.Result.Data);
    }

    [HttpGet("GetOrderBook")]
    public IActionResult GetOrderBook()
    {
        var book = new BinanceSpotSymbolOrderBook("ETHUSDT");
        var startResult = book.StartAsync();
        if (!startResult.Result.Success)
        {
            // Handle error, error info available in startResult.Error
        }
        // Book has successfully started and synchronized

        // Once no longer needed you can stop the live sync functionality by calling StopAsync()
        book.StopAsync();

        return new JsonResult(startResult.Result.Data);
    }

    [HttpGet("GetBalance")]
    public async Task<IActionResult> GetBalance()
    {
        //var optionsApiCredentials = new ApiCredentials(_apiKey, _secretKey); 

        using (var binanceClient = new BinanceRestClient())
        {
            var balance = await binanceClient.SpotApi.Account.GetBalancesAsync();

            if (!balance.Success)
            {
                return BadRequest(balance.Error);
            }

            return Ok(balance.Data);
        }


    }

    [HttpPost("Buy")]
    public async Task<IActionResult> Buy(string SYMBOL)
    {
        // burdan almýyor. program.cs ye eklendi!!
        //var optionsApiCredentials = new ApiCredentials(_apiKey, _secretKey); 

        using (var binanceClient = new BinanceRestClient())
        {
            //binanceClient.ClientOptions.ApiCredentials = optionsApiCredentials;
            var sonucallopens = await binanceClient.SpotApi.Trading.GetOpenOrdersAsync();
            string? newClientOrderId = null;
            long? orderId = null;
            var sonuc = await binanceClient.SpotApi.Trading.PlaceOrderAsync("BAKEUSDT", OrderSide.Buy, SpotOrderType.Limit, quantity: 20, price: Decimal.Parse("0.3100")
                , timeInForce: TimeInForce.GoodTillCanceled, newClientOrderId: newClientOrderId);

            var sonucopens = await binanceClient.SpotApi.Trading.GetOpenOrdersAsync("BAKEUSDT");
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
