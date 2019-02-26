using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Save_Files_Database_EF_MVC.Models
{
    public class TblFile
    {
          [Key]
          public int TblFileID { get; set; }
          public string Name { get; set; }
          public string ContentType { get; set; }
    }
}
