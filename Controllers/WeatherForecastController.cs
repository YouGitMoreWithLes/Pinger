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

    [HttpGet(Name = "PingHost")]
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
                    stringBuilder.AppendLine("Status :  " + reply.Status + " \n Time : " + reply.RoundtripTime.ToString() + " \n Address : " + reply.Address);
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
}
