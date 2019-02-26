

using System.Collections.Generic;
using System.Net.Http;

namespace FTPAPI.Models
{
  public class ResposTblFile
  {
    public int TblFileID { get; set; }
    public string Name { get; set; }
    public string ContentType { get; set; }
    //public string Data { get; set; }
    public byte[] Data { get; set; }
  }
}
