using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FTPAPI.Models
{
  public class Response<T>
  {
    public List<T> data { get; set; }
    public int status { get; set; }
    public string message { get; set; }
  }
}
