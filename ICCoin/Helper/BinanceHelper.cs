using Data.Models;
using Newtonsoft.Json;
using Serilog;
using System.Security.Cryptography;
using System.Text;

namespace ICCoin.Helper
{
    public class BinanceHelper
    {
        // live cengiz
        //private static string apiKey = "F0zsjsgh2QCr7JM5C00JK8PtX12t1TWlls9IBAvlX8d18X5SfVbNB8gcJhjw61FW";
        //private static string secretKey = "RJDLLnJh0F53XcjrZLbOuhss99RCG1Pt0Yu6PPTrEZ8b9XhVVJpQyiaUD3c00zbn";
        //private static string endpoint = "https://api.binance.com";
        private static string endpoint = AppSettings.endPoint; // "https://testnet.binancefuture.com";
        //private readonly DbHelper _dbHelper;
        /*
         test API Key
            6f741d8722979ad85cdbb7c4d94c7a0de05a73d9507fad50a0921984567ac3dc
            API Secret
            bd08ec0fe4242762f02e0f1843d6a98102cc37ddd336846d95fc7e69c81a5599
         */
        public BinanceHelper()
        {
            //_dbHelper = new DbHelper();
        }

        private static string apiKey = "yFviDUaU85iSjExhKbODs7mm0BX05KEZqFwHLF7BQzJF9C40kdMLpR7Y106CHueW";// AppSettings.apiKey; // "6f741d8722979ad85cdbb7c4d94c7a0de05a73d9507fad50a0921984567ac3dc";
        private static string secretKey = "ODDz6ZI65IBYot5sZ8SrPCsTS4bsjphR1nxujVIOhqu8WxjLEPpXa9O6wDHaAllZ";//AppSettings.secretKey; // "bd08ec0fe4242762f02e0f1843d6a98102cc37ddd336846d95fc7e69c81a5599";
                                                                 // Kaldıraç seviyesi (10x)
        private static int _leverage = 5;

        static string Sign(string queryString, string secretKey)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(queryString));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        static string Sign(object data, string secretKey)
        {
            string jsonString = JsonConvert.SerializeObject(data);
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(jsonString));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        public static async Task<dynamic> account()
        {
            string path = "/fapi/v2/account";
            //string method = "GET";

            long timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            // İmza oluştur
            string queryString = $"timestamp={timestamp}";
            string signature = Sign(queryString, secretKey);

            // İstek URL'sini oluştur
            string url = $"{endpoint}{path}?{queryString}&signature={signature}";

            // HTTP isteği oluştur
            using (HttpClient client = new HttpClient())
            {
                // İstek ayarları
                client.DefaultRequestHeaders.Add("X-MBX-APIKEY", apiKey);

                // İstek gönder
                HttpResponseMessage response = await client.GetAsync(url);

                // Yanıtı oku
                string responseData = await response.Content.ReadAsStringAsync();
                return responseData;
                //Console.WriteLine(responseData);
            }
        }

        public static async Task<dynamic> balance()
        {
            string path = "/fapi/v2/balance";
            //string method = "GET";

            // Unix epoch zamanı hesapla
            long timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            // İmza oluştur
            string queryString = $"timestamp={timestamp}";
            string signature = Sign(queryString, secretKey);

            // İstek URL'sini oluştur
            string url = $"{endpoint}{path}?{queryString}&signature={signature}";

            // HTTP isteği oluştur
            using (HttpClient client = new HttpClient())
            {
                // İstek ayarları
                client.DefaultRequestHeaders.Add("X-MBX-APIKEY", apiKey);

                // İstek gönder
                HttpResponseMessage response = await client.GetAsync(url);

                // Yanıtı oku
                string responseData = await response.Content.ReadAsStringAsync();

                return responseData;
                //Console.WriteLine(responseData);
            }
        }

        public static async Task<BinanceHttpResponse> positionRisk(string symbol)
        {
            BinanceHttpResponse binanceHttpResponse = new BinanceHttpResponse();

            try
            {
                string path = "/fapi/v2/positionRisk";
                long timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                var queryModel = new
                {
                    timestamp = timestamp,
                    symbol = symbol,
                };

                string queryString = $"timestamp={timestamp}&symbol={queryModel.symbol}";
                string signature = Sign(queryString, secretKey);
                string url = $"{endpoint}{path}?{queryString}&signature={signature}";

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-MBX-APIKEY", apiKey);
                    string requestBody = string.Empty;
                    HttpContent content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.GetAsync(url);

                    var responseData = await response.Content.ReadAsStringAsync();
                    binanceHttpResponse.ResBody = responseData;
                    var jsonData = JsonConvert.DeserializeObject(responseData);
                    binanceHttpResponse.Result = jsonData;

                    if (response.IsSuccessStatusCode)
                    {
                        binanceHttpResponse.IsSuccess = true;
                    }
                    else
                    {
                        binanceHttpResponse.IsSuccess = false;
                    }
                    return binanceHttpResponse;
                }
            }
            catch (Exception e)
            {
                binanceHttpResponse.IsSuccess = false;
                binanceHttpResponse.Message = e.Message;
                return binanceHttpResponse;
            }
        }

        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            // Unix zamanı 1 Ocak 1970'ten bu yana geçen saniye sayısına dönüştür
            DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            // Unix zaman damgasını kullanarak DateTime nesnesini oluştur
            return unixEpoch.AddMilliseconds(unixTimeStamp).ToLocalTime();
        }

        private static DateTime RoundToNearestPastInterval(DateTime time, int interval)
        {
            var minutes = time.Minute;
            var remainder = minutes % interval;
            var roundedMinutes = minutes - remainder;

            // Eğer şu anki zaman verilen zaman aralığının tam katı ise, aynı saat ve dakikayı döndür
            if (remainder == 0)
            {
                return new DateTime(
                    time.Year,
                    time.Month,
                    time.Day,
                    time.Hour,
                    minutes,
                    0,
                    time.Kind
                );
            }

            // Değilse, en yakın geçmiş zaman dilimine yuvarla
            return new DateTime(
                time.Year,
                time.Month,
                time.Day,
                time.Hour,
                roundedMinutes,
                0,
                time.Kind
            );
        }

        public static async Task<dynamic> marginTypeDegistir(string sembol)
        {
            string marginTipi = "ISOLATED";
            string path = "/fapi/v1/marginType";
            //string method = "GET";

            // Unix epoch zamanı hesapla
            long timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            // İmza oluştur
            string queryString = $"symbol={sembol}&marginType={marginTipi}&timestamp={timestamp}";
            string signature = Sign(queryString, secretKey);

            // İstek URL'sini oluştur
            string url = $"{endpoint}{path}?{queryString}&signature={signature}";

            // HTTP isteği oluştur
            using (HttpClient client = new HttpClient())
            {
                // İstek ayarları
                client.DefaultRequestHeaders.Add("X-MBX-APIKEY", apiKey);

                // İstek gönder
                HttpResponseMessage response = await client.PostAsync(url, null);

                // Yanıtı oku
                string responseData = await response.Content.ReadAsStringAsync();

                return responseData;
                //Console.WriteLine(responseData);
            }
        }

        public static async Task<dynamic> LeverageDegistir(string sembol, string _leverage)
        {
            string path = "/fapi/v1/leverage";
            //string method = "GET";

            // Unix epoch zamanı hesapla
            long timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            // İmza oluştur
            string queryString = $"symbol={sembol}&leverage={_leverage}&timestamp={timestamp}";
            string signature = Sign(queryString, secretKey);

            // İstek URL'sini oluştur
            string url = $"{endpoint}{path}?{queryString}&signature={signature}";

            // HTTP isteği oluştur
            using (HttpClient client = new HttpClient())
            {
                // İstek ayarları
                client.DefaultRequestHeaders.Add("X-MBX-APIKEY", apiKey);

                // İstek gönder
                HttpResponseMessage response = await client.PostAsync(url, null);

                // Yanıtı oku
                string responseData = await response.Content.ReadAsStringAsync();

                return responseData;
                //Console.WriteLine(responseData);
            }
        }


        #region ORDER İŞLEMLER

        public static async Task<BinanceHttpResponse> OrderSPOT(string symbol, string emirtipi, string quantity, decimal price, string type = "LIMIT", string orderID = "")
        {
            BinanceHttpResponse binanceHttpResponse = new BinanceHttpResponse();
            try
            {
                string path = "/api/v3/order";
                long timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                // Emir bilgilerini içeren model
                var order = new
                {
                    orderId = orderID,
                    symbol = symbol,
                    side = emirtipi,
                    type = type,
                    quantity = quantity,
                    price = price,
                    isolatedMargin = true,
                    timeInForce = "GTC",
                    reduceOnly = "true",
                    closePosition = "true",
                };

                // İmza oluştur
                string queryString = "";
                if (order.type == "LIMIT")
                {
                    if (!string.IsNullOrEmpty(order.orderId))
                    {
                        queryString = $"symbol={order.symbol}&side={order.side}&type={order.type}&orderId={order.orderId}&quantity={order.quantity}&timestamp={timestamp}";
                        queryString = queryString + $"&timeInForce={order.timeInForce}";
                        queryString = queryString + $"&price={order.price}";
                        queryString = queryString + $"&stopPrice={order.price}";
                        queryString = queryString + $"&reduceOnly={order.reduceOnly}";
                    }
                    else
                    {
                        queryString = $"symbol={order.symbol}&side={order.side}&type={order.type}&quantity={order.quantity}&timestamp={timestamp}";
                        queryString = queryString + $"&timeInForce={order.timeInForce}";
                        queryString = queryString + $"&price={order.price}";
                        queryString = queryString + $"&stopPrice={order.price}";
                        queryString = queryString + $"&reduceOnly={order.reduceOnly}";
                    }
                }
                else if (order.type == "MARKET")
                {
                    string ordertype = order.type;

                    queryString = $"symbol={order.symbol}&side={order.side}&type={ordertype}&quantity={order.quantity}&timestamp={timestamp}";
                    
                    //queryString = queryString + $"&timeInForce={order.timeInForce}";
                    //queryString = queryString + $"&price={order.price}";
                    //queryString = queryString + $"&stopPrice={order.stopPrice}";
                    //queryString = queryString + $"&reduceOnly={order.reduceOnly}";

                    if (!string.IsNullOrEmpty(order.orderId))
                    {
                        queryString = queryString + $"&orderId={order.orderId}";
                    }
                }
               
                // İmza oluştur
                string signature = Sign(queryString, secretKey);

                // İstek URL'sini oluştur
                string url = $"{endpoint}{path}?{queryString}&signature={signature}";
                binanceHttpResponse.ReqPath = url;
                // HTTP isteği oluştur
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-MBX-APIKEY", apiKey);

                    // Emir bilgisini JSON formatına dönüştür
                    string requestBody = string.Empty;

                    // İstek gövdesi (body)
                    HttpContent content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                    // İstek gönder
                    HttpResponseMessage response;
                    if (order.type == "TAKE_PROFIT")
                    {
                        response = await client.PostAsync(url, content);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(order.orderId))
                        {
                            response = await client.PutAsync(url, content);
                        }
                        else
                        {
                            response = await client.PostAsync(url, content);
                        }
                    }


                    var responseData = await response.Content.ReadAsStringAsync();
                    binanceHttpResponse.ResBody = responseData;

                    if (response.IsSuccessStatusCode)
                    {
                        // Yanıtı oku ve model içine at
                        //string responseData = await response.Content.ReadAsStringAsync();
                        //OrderResponse orderResponse = JsonConvert.DeserializeObject<OrderResponse>(responseData);
                        dynamic? jsonData = JsonConvert.DeserializeObject(responseData);

                        // Yanıtı oku
                        binanceHttpResponse.Result = jsonData;
                        binanceHttpResponse.IsSuccess = true;
                    }
                    else
                    {
                        binanceHttpResponse.IsSuccess = false;
                    }
                    return binanceHttpResponse;
                }
            }
            catch (Exception e)
            {
                binanceHttpResponse.IsSuccess = false;
                binanceHttpResponse.Message = e.Message;
                return binanceHttpResponse;
            }
        }

        public static async Task<BinanceHttpResponse> Order(string symbol, string emirtipi, string quantity, string amount, string StopPrice, string type = "LIMIT", string orderID = "")
        {
            BinanceHttpResponse binanceHttpResponse = new BinanceHttpResponse();
            try
            {
                string path = "/fapi/v1/order";
                long timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                // Emir bilgilerini içeren model
                var order = new
                {
                    orderId = orderID,
                    symbol = symbol,
                    side = emirtipi,
                    type = type,
                    quantity = quantity,
                    price = amount,
                    isolatedMargin = true,
                    leverage = _leverage,
                    timeInForce = "GTC",
                    marginType = "ISOLATED",
                    reduceOnly = "true",
                    closePosition = "true",
                    stopPrice = StopPrice,
                    workingType = "MARK_PRICE",
                    positionSide = "LONG",
                    priceProtect = "false"
                };

                // İmza oluştur
                string queryString = "";
                if (order.type == "LIMIT")
                {
                    if (!string.IsNullOrEmpty(order.orderId))
                    {
                        queryString = $"symbol={order.symbol}&side={order.side}&marginType={order.marginType}&type={order.type}&orderId={order.orderId}&leverage={order.leverage}&quantity={order.quantity}&timestamp={timestamp}";
                        queryString = queryString + $"&timeInForce={order.timeInForce}";
                        queryString = queryString + $"&price={order.price}";
                        queryString = queryString + $"&stopPrice={order.stopPrice}";
                        queryString = queryString + $"&reduceOnly={order.reduceOnly}";
                    }
                    else
                    {
                        queryString = $"symbol={order.symbol}&side={order.side}&marginType={order.marginType}&type={order.type}&leverage={order.leverage}&quantity={order.quantity}&timestamp={timestamp}";
                        queryString = queryString + $"&timeInForce={order.timeInForce}";
                        queryString = queryString + $"&price={order.price}";
                        queryString = queryString + $"&stopPrice={order.stopPrice}";
                        queryString = queryString + $"&reduceOnly={order.reduceOnly}";
                    }
                }
                else if (order.type == "MARKET")
                {
                    string ordertype = order.type;

                    //if (order.type == "MARKET")
                    //{
                    //    ordertype = "STOP_MARKET";
                    //}

                    queryString = $"symbol={order.symbol}&side={order.side}&marginType={order.marginType}&type={ordertype}&orderId={order.orderId}&leverage={order.leverage}&quantity={order.quantity}&timestamp={timestamp}";
                    //queryString = queryString + $"&timeInForce={order.timeInForce}";
                    //queryString = queryString + $"&price={order.price}";
                    //queryString = queryString + $"&stopPrice={order.stopPrice}";
                    //queryString = queryString + $"&reduceOnly={order.reduceOnly}";

                    if (!string.IsNullOrEmpty(order.orderId))
                    {
                        queryString = queryString + $"&orderId={order.orderId}";
                    }
                }
                else if (order.type == "TAKE_PROFIT")
                {
                    queryString = $"symbol={order.symbol}&side={order.side}&marginType={order.marginType}&quantity={order.quantity}";
                    queryString = queryString + $"&type={order.type}";
                    queryString = queryString + $"&timeInForce={order.timeInForce}";
                    queryString = queryString + $"&price={order.price}";
                    queryString = queryString + $"&workingType={order.workingType}";
                    queryString = queryString + $"&stopPrice={order.stopPrice}";
                    //queryString = queryString + $"&closePosition={order.closePosition}";
                    //queryString = queryString + $"&reduceOnly={order.reduceOnly}";
                    queryString = queryString + $"&timestamp={timestamp}";
                    if (!string.IsNullOrEmpty(order.orderId))
                    {
                        queryString = queryString + $"&orderId={order.orderId}";
                    }
                }
                else if (order.type == "TAKE_PROFIT_MARKET")
                {
                    queryString = $"symbol={order.symbol}&side={order.side}&quantity={order.quantity}";
                    //queryString = queryString + $"&marginType={order.marginType}";
                    queryString = queryString + $"&type={order.type}";
                    //queryString = queryString + $"&timeInForce={order.timeInForce}";
                    //queryString = queryString + $"&price={order.price}";
                    //queryString = queryString + $"&workingType={order.workingType}";
                    queryString = queryString + $"&stopPrice={order.stopPrice}";
                    //queryString = queryString + $"&closePosition={order.closePosition}";
                    //queryString = queryString + $"&reduceOnly={order.reduceOnly}";
                    queryString = queryString + $"&timestamp={timestamp}";
                    if (!string.IsNullOrEmpty(order.orderId))
                    {
                        queryString = queryString + $"&orderId={order.orderId}";
                    }
                }
                else if (order.type == "STOP_MARKET")
                {
                    queryString = $"symbol={order.symbol}&side={order.side}&type={order.type}";
                    //queryString = queryString + $"&marginType={order.marginType}";
                    //queryString = queryString + $"&timeInForce={order.timeInForce}";
                    //queryString = queryString + $"&price={order.stopPrice}";
                    queryString = queryString + $"&quantity={order.quantity}";
                    queryString = queryString + $"&stopPrice={order.stopPrice}";
                    queryString = queryString + $"&priceProtect={order.priceProtect}";
                    //queryString = queryString + $"&workingType={order.workingType}";
                    //queryString = queryString + $"&closePosition={order.closePosition}";
                    //queryString = queryString + $"&positionSide={order.positionSide}";
                    //queryString = queryString + $"&reduceOnly={order.reduceOnly}";
                    queryString = queryString + $"&timestamp={timestamp}";
                    if (!string.IsNullOrEmpty(order.orderId))
                    {
                        queryString = queryString + $"&orderId={order.orderId}";
                    }
                }
                else if (order.type == "STOP")
                {
                    queryString = $"symbol={order.symbol}&side={order.side}&marginType={order.marginType}&type={order.type}";
                    queryString = queryString + $"&timeInForce={order.timeInForce}";
                    queryString = queryString + $"&price={order.stopPrice}";
                    queryString = queryString + $"&quantity={order.quantity}";
                    queryString = queryString + $"&stopPrice={order.stopPrice}";
                    queryString = queryString + $"&workingType={order.workingType}";
                    //queryString = queryString + $"&closePosition={order.closePosition}";
                    //queryString = queryString + $"&positionSide={order.positionSide}";
                    //queryString = queryString + $"&reduceOnly={order.reduceOnly}";
                    queryString = queryString + $"&timestamp={timestamp}";
                    if (!string.IsNullOrEmpty(order.orderId))
                    {
                        queryString = queryString + $"&orderId={order.orderId}";
                    }
                }

                // İmza oluştur
                string signature = Sign(queryString, secretKey);

                // İstek URL'sini oluştur
                string url = $"{endpoint}{path}?{queryString}&signature={signature}";
                binanceHttpResponse.ReqPath = url;
                // HTTP isteği oluştur
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-MBX-APIKEY", apiKey);

                    // Emir bilgisini JSON formatına dönüştür
                    string requestBody = string.Empty;

                    // İstek gövdesi (body)
                    HttpContent content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                    // İstek gönder
                    HttpResponseMessage response;
                    if (order.type == "TAKE_PROFIT")
                    {
                        response = await client.PostAsync(url, content);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(order.orderId))
                        {
                            response = await client.PutAsync(url, content);
                        }
                        else
                        {
                            response = await client.PostAsync(url, content);
                        }
                    }


                    var responseData = await response.Content.ReadAsStringAsync();
                    binanceHttpResponse.ResBody = responseData;

                    if (response.IsSuccessStatusCode)
                    {
                        // Yanıtı oku ve model içine at
                        //string responseData = await response.Content.ReadAsStringAsync();
                        //OrderResponse orderResponse = JsonConvert.DeserializeObject<OrderResponse>(responseData);
                        dynamic? jsonData = JsonConvert.DeserializeObject(responseData);

                        // Yanıtı oku
                        binanceHttpResponse.Result = jsonData;
                        binanceHttpResponse.IsSuccess = true;
                    }
                    else
                    {
                        binanceHttpResponse.IsSuccess = false;
                    }
                    return binanceHttpResponse;
                }
            }
            catch (Exception e)
            {
                binanceHttpResponse.IsSuccess = false;
                binanceHttpResponse.Message = e.Message;
                return binanceHttpResponse;
            }
        }

        public static async Task<BinanceHttpResponse> OrderTRAILING_STOP_MARKET(string symbol, string emirtipi, string quantity, string amount, string ActivatePrice, string CallbackRate, string type = "TRAILING_STOP_MARKET", string orderID = "")
        {
            BinanceHttpResponse binanceHttpResponse = new BinanceHttpResponse();
            try
            {
                string path = "/fapi/v1/order";
                long timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                // Emir bilgilerini içeren model
                var order = new
                {
                    orderId = orderID,
                    symbol = symbol,
                    side = emirtipi,
                    type = type,
                    quantity = quantity,
                    price = amount,
                    isolatedMargin = true,
                    leverage = _leverage,
                    timeInForce = "GTC",
                    marginType = "ISOLATED",
                    reduceOnly = "true",
                    callbackRate = CallbackRate,
                    activationPrice = ActivatePrice
                };

                // İmza oluştur
                string queryString = "";
                if (order.type == "TRAILING_STOP_MARKET")
                {
                    queryString = $"symbol={order.symbol}&side={order.side}";
                    queryString = queryString + $"&marginType={order.marginType}";
                    queryString = queryString + $"&type={order.type}";
                    queryString = queryString + $"&quantity={order.quantity}";
                    queryString = queryString + $"&timeInForce={order.timeInForce}";
                    //queryString = queryString + $"&price={order.price}";
                    queryString = queryString + $"&callbackRate={order.callbackRate}";
                    queryString = queryString + $"&activationPrice={order.activationPrice}";
                    queryString = queryString + $"&reduceOnly={order.reduceOnly}";
                    queryString = queryString + $"&timestamp={timestamp}";
                    //queryString = queryString + $"&orderId={order.orderId}";

                    if (!string.IsNullOrEmpty(order.orderId))
                    {
                        queryString = queryString + $"&orderId={order.orderId}";
                    }
                }

                // İmza oluştur
                string signature = Sign(queryString, secretKey);

                // İstek URL'sini oluştur
                string url = $"{endpoint}{path}?{queryString}&signature={signature}";
                binanceHttpResponse.ReqPath = url;
                // HTTP isteği oluştur
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-MBX-APIKEY", apiKey);

                    // Emir bilgisini JSON formatına dönüştür
                    string requestBody = string.Empty;

                    // İstek gövdesi (body)
                    HttpContent content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                    // İstek gönder
                    HttpResponseMessage response;
                    if (!string.IsNullOrEmpty(order.orderId))
                    {
                        response = await client.PutAsync(url, content);
                    }
                    else
                    {
                        response = await client.PostAsync(url, content);
                    }

                    var responseData = await response.Content.ReadAsStringAsync();
                    binanceHttpResponse.ResBody = responseData;

                    if (response.IsSuccessStatusCode)
                    {
                        // Yanıtı oku ve model içine at
                        //string responseData = await response.Content.ReadAsStringAsync();
                        //OrderResponse orderResponse = JsonConvert.DeserializeObject<OrderResponse>(responseData);
                        dynamic? jsonData = JsonConvert.DeserializeObject(responseData);

                        // Yanıtı oku
                        binanceHttpResponse.Result = jsonData;
                        binanceHttpResponse.IsSuccess = true;
                    }
                    else
                    {
                        binanceHttpResponse.IsSuccess = false;
                    }
                    return binanceHttpResponse;
                }
            }
            catch (Exception e)
            {
                binanceHttpResponse.IsSuccess = false;
                binanceHttpResponse.Message = e.Message;
                return binanceHttpResponse;
            }
        }

        public static async Task<BinanceHttpResponse> GetOrderDetail(string symbol, string orderID)
        {
            BinanceHttpResponse binanceHttpResponse = new BinanceHttpResponse();

            try
            {
                string path = "/fapi/v1/order";
                long timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                var order = new
                {
                    orderId = orderID,
                    symbol = symbol
                };

                string queryString = "";
                queryString = $"symbol={order.symbol}&orderId={order.orderId}&timestamp={timestamp}";

                string signature = Sign(queryString, secretKey);

                string url = $"{endpoint}{path}?{queryString}&signature={signature}";
                binanceHttpResponse.ReqPath = url;

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-MBX-APIKEY", apiKey);

                    string requestBody = string.Empty;

                    HttpContent content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.GetAsync(url);

                    var responseData = await response.Content.ReadAsStringAsync();
                    dynamic? jsonData = JsonConvert.DeserializeObject(responseData);
                    binanceHttpResponse.Result = jsonData;
                    binanceHttpResponse.ResBody = responseData;

                    if (response.IsSuccessStatusCode)
                    {
                        binanceHttpResponse.IsSuccess = true;
                    }
                    else
                    {
                        binanceHttpResponse.IsSuccess = false;
                    }
                    return binanceHttpResponse;

                    //string responseData = await response.Content.ReadAsStringAsync();
                    //OrderResponse orderResponse = JsonConvert.DeserializeObject<OrderResponse>(responseData);

                    //return orderResponse;
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                binanceHttpResponse.Message = e.Message;
                binanceHttpResponse.IsSuccess = false;
                return binanceHttpResponse;
            }
        }

        public static async Task<dynamic> TumOrderlariSil(string symbol)
        {
            BinanceHttpResponse binanceHttpResponse = new BinanceHttpResponse();
            try
            {
                string path = "/fapi/v1/allOpenOrders";
                long timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                var emir = new
                {
                    symbol = symbol
                };

                string queryString = "";
                queryString = $"symbol={emir.symbol}&timestamp={timestamp}";
                string signature = Sign(queryString, secretKey);
                string url = $"{endpoint}{path}?{queryString}&signature={signature}";

                binanceHttpResponse.ReqPath = url;

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-MBX-APIKEY", apiKey);

                    HttpResponseMessage response = await client.DeleteAsync(url);

                    var responseData = await response.Content.ReadAsStringAsync();
                    binanceHttpResponse.IsSuccess = true;
                    binanceHttpResponse.Result = responseData;
                    binanceHttpResponse.ResBody = responseData;

                    if (response.IsSuccessStatusCode)
                    {
                        binanceHttpResponse.IsSuccess = true;
                    }
                    else
                    {
                        binanceHttpResponse.IsSuccess = false;
                    }
                    return binanceHttpResponse;
                }
            }
            catch (Exception e)
            {
                binanceHttpResponse.Message = e.Message;
                binanceHttpResponse.IsSuccess = false;
                return binanceHttpResponse;
            }
        }

        public static async Task<dynamic> OrderSil(string symbol, string orderId)
        {
            BinanceHttpResponse binanceHttpResponse = new BinanceHttpResponse();
            try
            {
                string path = "/fapi/v1/order";
                long timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                var order = new
                {
                    symbol = symbol,
                    orderId = orderId
                };

                string queryString = "";
                queryString = $"symbol={order.symbol}";
                queryString = queryString + $"&orderId={order.orderId}";
                queryString = queryString + $"&timestamp={timestamp}";

                string signature = Sign(queryString, secretKey);
                string url = $"{endpoint}{path}?{queryString}&signature={signature}";

                binanceHttpResponse.ReqPath = url;

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-MBX-APIKEY", apiKey);

                    HttpResponseMessage response = await client.DeleteAsync(url);

                    var responseData = await response.Content.ReadAsStringAsync();
                    binanceHttpResponse.IsSuccess = true;
                    binanceHttpResponse.Result = responseData;
                    binanceHttpResponse.ResBody = responseData;
                    Log.Warning(responseData);
                    if (response.IsSuccessStatusCode)
                    {
                        binanceHttpResponse.IsSuccess = true;
                    }
                    else
                    {
                        binanceHttpResponse.IsSuccess = false;
                    }
                    return binanceHttpResponse;
                }
            }
            catch (Exception e)
            {
                binanceHttpResponse.Message = e.Message;
                binanceHttpResponse.IsSuccess = false;
                return binanceHttpResponse;
            }
            finally
            {

            }
        }

        public static bool ProcessLogYaz(int ProcessId, string ReqPath, string ReqBody, string ResBody)
        {
            try
            {
                ORDER_LOG processLog = new ORDER_LOG();
                //processLog.ProcessId = ProcessId;
                //processLog.ReqPath = ReqPath;
                //processLog.ReqBody = ReqBody;
                //processLog.ResBody = ResBody;
                //processLog.InsertDateTime = DateTime.Now;

                return true;
            }
            catch (Exception e)
            {
                Log.Error("ProcessLogYaz Error ProcessID:" + ProcessId + " " + e.Message);
                return false;
            }
        }

        public static async Task<BinanceHttpResponse> OrderOCO(string symbol, string emirtipi, string quantity, string Price, string StopPrice, string stopLimitPrice)
        {
            BinanceHttpResponse binanceHttpResponse = new BinanceHttpResponse();
            try
            {
                string path = "/fapi/v1/margin/order/oco";
                long timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                var side = emirtipi;
                quantity = "0.001m";
                var price = Price;
                var stopPrice = StopPrice;
                stopLimitPrice = "44000m";
                var stopLimitTimeInForce = "GTC";

                var order = new
                {
                    symbol = symbol,
                    side = side,
                    quantity = quantity.ToString(),
                    price = price.ToString(),
                    stopPrice = stopPrice.ToString(),
                    stopLimitPrice = stopLimitPrice.ToString(),
                    stopLimitTimeInForce = stopLimitTimeInForce,
                    timestamp = timestamp.ToString()
                };

                //var order = new Dictionary<string, string>
                //{
                //	{ "symbol", symbol },
                //	{ "side", side },
                //	{ "quantity", quantity.ToString() },
                //	{ "price", price.ToString() },
                //	{ "stopPrice", stopPrice.ToString() },
                //	{ "stopLimitPrice", stopLimitPrice.ToString() },
                //	{ "stopLimitTimeInForce", stopLimitTimeInForce },
                //	{ "timestamp", timestamp.ToString() }
                //};


                // İmza oluştur
                string queryString = "";
                queryString = $"symbol={order.symbol}&side={order.side}&quantity={order.quantity}";
                queryString = queryString + $"&price={order.price}";
                queryString = queryString + $"&stopPrice={order.stopPrice}";
                queryString = queryString + $"&price={order.price}";
                queryString = queryString + $"&stopPrice={order.stopPrice}";
                queryString = queryString + $"&stopLimitTimeInForce={order.stopLimitTimeInForce}";
                queryString = queryString + $"&timestamp={timestamp}";

                // İmza oluştur
                string signature = Sign(queryString, secretKey);

                // İstek URL'sini oluştur
                string url = $"{endpoint}{path}?{queryString}&signature={signature}";
                binanceHttpResponse.ReqPath = url;
                // HTTP isteği oluştur
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-MBX-APIKEY", apiKey);

                    // Emir bilgisini JSON formatına dönüştür
                    string requestBody = string.Empty;

                    // İstek gövdesi (body)
                    HttpContent content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                    // İstek gönder
                    HttpResponseMessage response;
                    response = await client.PostAsync(url, content);

                    var responseData = await response.Content.ReadAsStringAsync();
                    binanceHttpResponse.ResBody = responseData;

                    if (response.IsSuccessStatusCode)
                    {
                        dynamic? jsonData = JsonConvert.DeserializeObject(responseData);

                        // Yanıtı oku
                        binanceHttpResponse.Result = jsonData;
                        binanceHttpResponse.IsSuccess = true;
                    }
                    else
                    {
                        binanceHttpResponse.IsSuccess = false;
                    }
                    return binanceHttpResponse;
                }
            }
            catch (Exception e)
            {
                binanceHttpResponse.IsSuccess = false;
                binanceHttpResponse.Message = e.Message;
                return binanceHttpResponse;
            }
        }

        public static async Task<BinanceHttpResponse> PozisyonKapat(string symbol)
        {
            BinanceHttpResponse binanceHttpResponse = new BinanceHttpResponse();
            try
            {
                string apiKey = AppSettings.apiKey;
                string apiSecret = AppSettings.secretKey;

                var openPositionsResponse = await GetOpenPositions(apiKey, apiSecret, symbol);
                if (openPositionsResponse.IsSuccessStatusCode)
                {
                    // Başarılı bir şekilde açık pozisyonlar alındı
                    Console.WriteLine("Açık pozisyonlar alındı:");
                    var openPositionsContent = await openPositionsResponse.Content.ReadAsStringAsync();
                    Console.WriteLine(openPositionsContent);

                    // Açık pozisyonları kapatmak için işlem yap
                    var closePositionResponse = await CloseOpenPositions(apiKey, apiSecret, symbol);
                    if (closePositionResponse.IsSuccessStatusCode)
                    {
                        binanceHttpResponse.IsSuccess = true;
                        Console.WriteLine("Açık pozisyonlar başarıyla kapatıldı.");
                    }
                    else
                    {
                        binanceHttpResponse.IsSuccess = false;
                        Console.WriteLine($"Açık pozisyonlar kapatılırken hata oluştu: {closePositionResponse.ReasonPhrase}");
                    }
                }
                else
                {
                    binanceHttpResponse.IsSuccess = false;
                    Console.WriteLine($"Açık pozisyonlar alınırken hata oluştu: {openPositionsResponse.ReasonPhrase}");
                }
            }
            catch (Exception e)
            {
                binanceHttpResponse.IsSuccess = false;
                binanceHttpResponse.Message = e.Message;
                Console.WriteLine($"Açık pozisyonlar alınırken hata oluştu: " + e.Message);
            }
            return binanceHttpResponse;
        }

        public static async Task<HttpResponseMessage> GetOpenPositions(string apiKey, string apiSecret, string symbol)
        {
            // HTTP isteği için gerekli client oluştur
            var client = new HttpClient();
            client.BaseAddress = new Uri(endpoint);

            // Açık pozisyonları almak için isteği hazırla
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v3/openOrders?symbol={symbol}");
            request.Headers.Add("X-MBX-APIKEY", apiKey);

            // İsteği imzala
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var queryString = $"symbol={symbol}&timestamp={timestamp}";
            var signature = Sign(queryString, apiSecret);
            request.RequestUri = new Uri(request.RequestUri + $"&timestamp={timestamp}&signature={signature}");

            // İsteği gönder
            var response = await client.SendAsync(request);
            return response;
        }

        public static async Task<HttpResponseMessage> CloseOpenPositions(string apiKey, string apiSecret, string symbol)
        {
            // HTTP isteği için gerekli client oluştur
            var client = new HttpClient();
            client.BaseAddress = new Uri(endpoint);

            // Açık pozisyonları kapatmak için isteği hazırla
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v3/openOrders?symbol={symbol}");
            request.Headers.Add("X-MBX-APIKEY", apiKey);

            // İsteği imzala
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var queryString = $"symbol={symbol}&timestamp={timestamp}";
            var signature = Sign(queryString, apiSecret);
            request.RequestUri = new Uri(request.RequestUri + $"&timestamp={timestamp}&signature={signature}");

            // İsteği gönder
            var response = await client.SendAsync(request);
            return response;
        }
        #endregion

    }
}
