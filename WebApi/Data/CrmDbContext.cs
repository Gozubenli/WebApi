using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApi.DbModels;

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
        public DbSet<Employee_Work> Employee_Works { get; set; }
        public DbSet<Customer_Project> Customer_Projects { get; set; }
        public DbSet<Employee_Group> Employee_Groups { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Title> Titles { get; set; }
        public DbSet<Employee_Role> Employee_Roles { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Option> Options { get; set; }
        public DbSet<Category_Option> Category_Options { get; set; }

        public CrmDbContext(DbContextOptions<CrmDbContext> options) : base(options)
        {
        }
    }
}
