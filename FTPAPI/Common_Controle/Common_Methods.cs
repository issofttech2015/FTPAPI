using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Web;

namespace FTPAPI.Common_Controle
{
  public class Common_Methods
  {
    public static string getClientIp()
    {
      var retunVal = "";
      var host = Dns.GetHostEntry(Dns.GetHostName());
      foreach (var ip in host.AddressList)
      {
        if (ip.AddressFamily == AddressFamily.InterNetwork)
        {
          retunVal = ip.ToString();

        }
      }
      return retunVal;
    }
  }
}
