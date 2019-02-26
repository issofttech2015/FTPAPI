using Save_Files_Database_EF_MVC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace Save_Files_Database_EF_MVC.Context.Maping
{
    public class TblFileMap:EntityTypeConfiguration<TblFile>
    {
        public TblFileMap()
        {
            this.HasKey(t => t.TblFileID);
            this.Property(t => t.TblFileID).HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity);

            this.ToTable("TblFile");
        }
    }
}