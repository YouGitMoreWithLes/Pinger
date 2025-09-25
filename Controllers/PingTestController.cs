using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Dynamic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace Pinger.Controllers;

[ApiController]
[Route("[controller]")]
public class PingTestController : ControllerBase
{
    private readonly ILogger<PingTestController> _logger;
    private readonly ConfigurationManager _configration;


    public PingTestController(ConfigurationManager configration, ILogger<PingTestController> logger)
    {
        _configration = configration;
        _logger = logger;
    }

    [HttpGet]
    [Route("/health")]
    public async Task<string> Get()
    {
        return await Task.FromResult("Healthy");
    }

    [HttpGet("Ping")]
    public string Get([FromQuery] string hostName = "www.google.com", [FromQuery] int tries = 1, [FromQuery] int timeOut = 5)
    {
        string result = "Default string value";

        try
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < tries; i++)
            {
                Ping myPing = new Ping();
                PingReply reply = myPing.Send(hostName, 1000 * timeOut);

                if (reply != null)
                {
                    stringBuilder.AppendLine("Status :  " + reply.Status + " Time : " + reply.RoundtripTime.ToString() + " Address : " + reply.Address);
                }
                else
                {
                    stringBuilder.AppendLine("Reply was null");
                }

                result = stringBuilder.ToString();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.GetBaseException().Message);
            result = ex.GetBaseException().Message;
        }

        return result;
    }

    [HttpGet("GetHttpContent")]
    public async Task<string> GetHttpContent([FromQuery] string url = "https://www.google.com")
    {
        _logger.LogInformation("GetHttpContent: starting");
        string result = "Default value";

        try
        {
            HttpClient client = new HttpClient();
            client.Timeout = new TimeSpan(0, 0, 15);

            _logger.LogInformation($"GetHttpContent: GET: {url}");

            var data = await client.GetAsync(url, new HttpCompletionOption()
            {

            });
            _logger.LogInformation("GetHttpContent: executed http GET");

            if (data.IsSuccessStatusCode)
            {
                _logger.LogInformation("GetHttpContent: GET was successful");

                Uri hostUrl = new Uri(url);
                var hmmm = CheckHostDns(hostUrl.Host);
                result = await data.Content.ReadAsStringAsync();

                _logger.LogInformation($"GetHttpContent: DNS result: {hmmm}");

                result = hmmm + " : " + result;
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"GetHttpContent: Exception: {exception.GetBaseException().Message}");
            result = exception.GetBaseException().Message;
        }

        _logger.LogInformation($"GetHttpContent: result: {result}");

        return result;
    }

    [HttpGet("GetHttpContentWithProxy")]
    public async Task<string> GetHttpContentWithProxy([FromQuery] string url = "https://www.google.com")
    {
        _logger.LogInformation("GetHttpContent: starting");
        string result = "Default value";

        try
        {
            HttpClient client = GetProxyHttpClient();
            client.Timeout = new TimeSpan(0, 0, 15);

            _logger.LogInformation($"GetHttpContent: GET: {url}");

            var data = await client.GetAsync(url, new HttpCompletionOption()
            {

            });
            _logger.LogInformation("GetHttpContent: executed http GET");

            if (data.IsSuccessStatusCode)
            {
                _logger.LogInformation("GetHttpContent: GET was successful");

                Uri hostUrl = new Uri(url);
                var hmmm = CheckHostDns(hostUrl.Host);
                result = await data.Content.ReadAsStringAsync();

                _logger.LogInformation($"GetHttpContent: DNS result: {hmmm}");

                result = hmmm + " : " + result;
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"GetHttpContent: Exception: {exception.GetBaseException().Message}");
            result = exception.GetBaseException().Message;
        }

        _logger.LogInformation($"GetHttpContent: result: {result}");

        return result;
    }


    [HttpGet("CheckHostDns")]
    public string CheckHostDns([FromQuery] string hostName = "www.google.com")
    {
        _logger.LogInformation($"CheckHostDns: starting");
        string result = "Default string";

        try
        {
            _logger.LogInformation($"CheckHostDns: checking DNS host name: {hostName}");

            var dnsResult = System.Net.Dns.GetHostEntry(hostName);
            result = "Found: ";

            _logger.LogInformation($"CheckHostDns: GET complete");

            foreach (var address in dnsResult.AddressList)
            {
                result += " " + address;
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"CheckHostDns: Exception: {exception.GetBaseException().Message}");
            result = exception.GetBaseException().Message;
        }

        _logger.LogInformation($"CheckHostDns: starting");

        return result;
    }

    private HttpClient GetProxyHttpClient()
    {
        var proxy = new WebProxy
        {
            Address = new Uri($"http://proxy_cgw.sdsheriff.com:8080"),
            BypassProxyOnLocal = true,
            UseDefaultCredentials = false,
        };

        var httpClientHandler = new HttpClientHandler
        {
            Proxy = proxy,
        };

        httpClientHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

        var client = new HttpClient(handler: httpClientHandler, disposeHandler: true);

        return client;
    }

    [HttpGet("CheckDbConnection")]
    public string CheckDbConnection()
    {
        StringBuilder sb = new StringBuilder();

        try
        {
            sb.AppendLine("Starting");

            var conf = _configration["Database:ConnString"];
            sb.AppendLine(conf);

            using (SqlConnection conn = new SqlConnection(conf))
            {
                sb.AppendLine("Opening");
                conn.Open();

                sb.AppendLine("Closing");
                conn.Close();
            }

            sb.AppendLine("Finished");
        }
        catch (Exception exception)
        {
            _logger.LogError("Ooppss", exception);
            sb.AppendLine(exception.GetBaseException().Message);
        }

        return sb.ToString();
    }

    [HttpGet("TcpConnect")]
    public string TcpConnect([FromQuery] string host, [FromQuery] string port)
    {
        StringBuilder sb = new StringBuilder();

        try
        {
            sb.AppendLine("Starting");

            int iPort = int.Parse(port);
            sb.AppendLine($"Host: {host} Port: {iPort}");

            sb.AppendLine("Dns lookup");
            string ipAddressString = System.Net.Dns.GetHostEntry(host).AddressList[0].ToString();

            IPAddress ipAddress = IPAddress.Parse(host);
            sb.AppendLine($"IP address: {ipAddress.ToString()}");
            
            IPEndPoint iPEndpoint = new IPEndPoint(ipAddress, iPort);
            TcpClient tcpClient = new TcpClient(AddressFamily.InterNetwork);

            tcpClient.ReceiveTimeout = 30;
            tcpClient.SendTimeout = 30;

            sb.AppendLine("Opening");
            tcpClient.Connect(iPEndpoint);

            sb.AppendLine($"Connected? {tcpClient.Connected}");

            sb.AppendLine("Closing");
            tcpClient.Close();

            sb.AppendLine("Finished");
        }
        catch (Exception exception)
        {
            _logger.LogError("Ooppss", exception);
            sb.AppendLine(exception.GetBaseException().Message);
            sb.AppendLine(exception.StackTrace);
        }

        return sb.ToString();
    }
}
