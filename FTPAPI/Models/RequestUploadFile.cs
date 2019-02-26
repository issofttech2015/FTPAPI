using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FTPAPI.Models
{
  public class RequestUploadFile : RequestDownloadFile
  {
    public string FileData { get; set; }
    public int FileContentLength { get; set; }
  }
}
