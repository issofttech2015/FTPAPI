using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FTPAPI.Models
{
  public class RequestDownloadFile:RequestDirectoryDetalis
  {
    public string FileName { get; set; }
    public string FolderName { get; set; }
  }
}
