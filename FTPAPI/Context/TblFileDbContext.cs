using Save_Files_Database_EF_MVC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Save_Files_Database_EF_MVC.Context
{
    public class TblFileDbContext: DbContext
    {
        public TblFileDbContext() : base("TblFileDbContext")
        {

        }

        public virtual DbSet<TblFile> TblFile { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var typesToRegister = Assembly.GetExecutingAssembly().GetTypes()
         .Where(type => !String.IsNullOrEmpty(type.Namespace))
         .Where(type => type.BaseType != null && type.BaseType.IsGenericType
              && type.BaseType.GetGenericTypeDefinition() == typeof(EntityTypeConfiguration<>));
            foreach (var type in typesToRegister)
            {
                dynamic configurationInstance = Activator.CreateInstance(type);
                modelBuilder.Configurations.Add(configurationInstance);
            }
            Database.SetInitializer<TblFileDbContext>(null);
            //modelBuilder.Entity<Institution>().MapToStoredProcedures();
            base.OnModelCreating(modelBuilder);
        }
    }
}
