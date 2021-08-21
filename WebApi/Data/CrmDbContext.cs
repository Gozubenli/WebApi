using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApi.Models;

namespace WebApi.Data
{
    public class CrmDbContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Address> Address { get; set; }
        public DbSet<AppSettings> AppSettings { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Warehouse> Warehouse { get; set; }
        public DbSet<WebSettings> WebSettings { get; set; }
        public DbSet<Work> Works { get; set; }
        public DbSet<Customer_Project> Customer_Projects { get; set; }
        public DbSet<Employee_Group> Employee_Groups { get; set; }

        public CrmDbContext(DbContextOptions<CrmDbContext> options) : base(options)
        {
        }
    }
}
