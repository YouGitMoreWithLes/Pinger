using Microsoft.AspNetCore.Mvc;
using System.Net.NetworkInformation;
using System.Text;

namespace Pinger.Controllers;

[ApiController]
[Route("[controller]")]
public class PingTestController : ControllerBase
{
    private readonly ILogger<PingTestController> _logger;

    public PingTestController(ILogger<PingTestController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    [Route("/health")]
    public async Task<string> Get()
    {
        return "Healthy";
    }

    [HttpGet("Ping")]
    public string Get([FromQuery] string hostName = "www.google.com", [FromQuery] int tries = 4)
    {
        string result = "Default string value";

        try
        {
            StringBuilder stringBuilder= new StringBuilder();
            for (int i = 0; i < tries; i++)
            {
                Ping myPing = new Ping();
                PingReply reply = myPing.Send(hostName, 1000 * 15);

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
        catch
        {
            Console.WriteLine("ERROR: You have Some TIMEOUT issue");
        }

        return result;
    }

    [HttpGet("GetHttpContent")]
    public async Task<string> GetHttpContent([FromQuery] string url)
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


    [HttpGet("CheckHostDns")]
    public string CheckHostDns([FromQuery] string hostName)
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
}
