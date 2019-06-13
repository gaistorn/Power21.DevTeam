using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PES.Models;
using Power21.PEIUEcosystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PES.Service.WebApiService
{
    public class AccountRecordContext : IdentityDbContext<AccountModel>
    {
        public DbSet<AssetDBModel> Assets { get; set; }
        public DbSet<AddressModel> Address { get; set; }

        public AccountRecordContext(DbContextOptions<AccountRecordContext> options) : base(options)
        {
            
            //AccountRecordContext s;s.Find()
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<AssetDBModel>().HasKey(x => x.PK);
            builder.Entity<AddressModel>().HasKey(m => m.PK);

            //// shadow properties
            //builder.Entity<EventLogData>().Property<DateTime>("UpdatedTimestamp");
            //builder.Entity<SourceInfo>().Property<DateTime>("UpdatedTimestamp");


        }
    }

}
