using iTaxSuite.Library.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Sockets;

namespace iTaxSuite.WebHook.Controllers
{
    public abstract class MBaseController : ControllerBase
    {
        // https://code-maze.com/aspnetcore-how-to-get-the-remote-host-ip-address/
        // https://medium.com/@luisalexandre.rodrigues/logging-http-request-and-response-in-net-web-api-268135dcb27b
        internal string GetClientIpAddress()
        {
            string _method_ = "GetClientIpAddress";
            string clientAddress = null;
            try
            {
                string HdrForwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(HdrForwardedFor))
                {
                    var ips = HdrForwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries)
                              .Select(s => s.Trim());
                    foreach (var ip in ips)
                    {
                        if (IPAddress.TryParse(ip, out var address) && (address.AddressFamily
                            is AddressFamily.InterNetwork or AddressFamily.InterNetworkV6))
                        {
                            clientAddress = Convert.ToString(address);
                            break;
                        }
                    }
                }

                var HdrRemoteAddr = Request.Headers["REMOTE_ADDR"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(HdrRemoteAddr))
                {
                    if (IPAddress.TryParse(HdrRemoteAddr, out var address) && (address.AddressFamily
                        is AddressFamily.InterNetwork or AddressFamily.InterNetworkV6))
                    {
                        clientAddress = Convert.ToString(address);
                    }
                }

                var HdrRealIP = Request.Headers["X-Real-IP"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(HdrRealIP))
                {
                    if (IPAddress.TryParse(HdrRealIP, out var address) && (address.AddressFamily
                        is AddressFamily.InterNetwork or AddressFamily.InterNetworkV6))
                    {
                        clientAddress = Convert.ToString(address);
                    }
                }

                if (string.IsNullOrWhiteSpace(clientAddress))
                    clientAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} hook/echo error : {ex.GetBaseException()}");
            }
            return clientAddress;
        }
    }
}
