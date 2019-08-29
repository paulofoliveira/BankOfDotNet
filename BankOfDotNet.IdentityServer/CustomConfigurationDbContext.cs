//using IdentityServer4.EntityFramework.DbContexts;
//using IdentityServer4.EntityFramework.Options;
//using Microsoft.EntityFrameworkCore;
//using System;

//namespace BankOfDotNet.IdentityServer
//{
//    public class CustomConfigurationDbContext : ConfigurationDbContext
//    {
//        public CustomConfigurationDbContext(DbContextOptions<CustomConfigurationDbContext> options, ConfigurationStoreOptions storeOptions) : base(options, storeOptions)
//        {

//        }

//        public DbSet<Tenant> Tenants { get; set; }

//        protected override void OnModelCreating(ModelBuilder modelBuilder)
//        {
//            modelBuilder.Entity<Tenant>().ToTable("Tenant");

//            base.OnModelCreating(modelBuilder);
//        }
//    }

//    public class Tenant
//    {
//        public Guid Id { get; set; }
//        public string Name { get; set; }
//    }
//}
