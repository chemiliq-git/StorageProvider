using DataContext.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataContext.DataContext
{
   public class EFDBContext :DbContext
    {
        public EFDBContext()
        {

        }

        public EFDBContext(DbContextOptions<EFDBContext> options) : base(options)
        {
            this.Database.Migrate();
        }

        public DbSet<Product> Product { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                //optionsBuilder.UseSqlServer(@"data source=(local)\SQLEXRESS; initial catalog=Pers_ProjectDB;persist security info=True;Trusted_Connection=True");
                optionsBuilder.UseSqlServer(@"Server=(local)\SQLEXPRESS;Database=Pers_ProjectDB;Trusted_Connection=True;");
            }
            base.OnConfiguring(optionsBuilder);
        }
    }
}
