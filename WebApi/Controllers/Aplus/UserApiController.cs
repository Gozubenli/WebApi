using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Data;
using Microsoft.EntityFrameworkCore;
using WebApi.DbModels;
using Newtonsoft.Json;
using WebApi.UiModels;
using Microsoft.AspNetCore.Http;
using WebApi.Utils;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Data.OleDb;

namespace WebApi.Aplus.Controllers
{
    [Route("aplus/[controller]")]
    [ApiController]
    public class UserApiController : ControllerBase
    {
        private readonly ILogger<AddressApiController> _logger;
        private readonly IDbContextFactory<CrmDbContext> _contextFactory;
        private DbLogger _dbLogger;
        public UserApiController(ILogger<AddressApiController> logger, IDbContextFactory<CrmDbContext> contextFactory)
        {
            _logger = logger;
            _contextFactory = contextFactory;
            _dbLogger = new DbLogger(_contextFactory);
        }

        [HttpPost("GetUserList")]
        public async Task<List<User>> GetUserList([FromBody] JObject param)
        {
            List<User> list = new List<User>();
            try
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    list = await (from m in context.Users select m).ToListAsync();
                }

                _logger.LogInformation("GetUserList Count:" + list.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return list;
        }

        [HttpPost("AddUser")]
        public async Task<bool> AddUser([FromBody] User user)
        {
            bool result = false;
            if (user != null)
            {
                try
                {
                    user.CreatedDate = DateTime.UtcNow;
                    user.UpdateDate = DateTime.UtcNow;
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        var dbResult = context.Users.Add(user);
                        await context.SaveChangesAsync();
                        result = dbResult != null;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                string message = "User " + user.Name + (result ? " Added" : "Could Not Added");
                _logger.LogInformation("AddUser\tParam: " + JsonConvert.SerializeObject(user) + "\tResult: " + result);
                await _dbLogger.logInfo(message, getUserName());
            }
            _logger.LogInformation("AddUser Result:" + result);
            return result;
        }

        [HttpPost("UpdateUser")]
        public async Task<bool> UpdateUser([FromBody] User user)
        {
            bool result = false;
            if (user != null)
            {
                try
                {
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        var existing = context.Users.FirstOrDefault(o => o.Id == user.Id);
                        if (existing != null)
                        {
                            existing.Name = user.Name;
                            existing.SurName = user.SurName;
                            existing.UserName = user.UserName;
                            existing.Email = user.Email;
                            existing.Phone = user.Phone;
                            existing.Password = user.Password;
                            existing.Status = user.Status;
                            existing.UpdateDate = DateTime.UtcNow;
                            int dbResult = await context.SaveChangesAsync();
                            result = dbResult > 0;
                        }
                        else
                        {
                            _logger.LogError("UpdateUser Not Found");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                string message = "User " + user.Name + (result ? " Updated" : "Could Not Updated");
                _logger.LogInformation("UpdateUser\tParam: " + JsonConvert.SerializeObject(user) + "\tResult: " + result);
                await _dbLogger.logInfo(message, getUserName());
            }
            return result;
        }

        [HttpPost("DeleteUser")]
        public async Task<bool> DeleteUser([FromBody] User user)
        {
            bool result = false;
            if (user != null)
            {
                try
                {
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        var existing = context.Users.FirstOrDefault(o => o.Id == user.Id);
                        if (existing != null)
                        {
                            context.Users.Remove(existing);
                            int dbResult = await context.SaveChangesAsync();
                            result = dbResult > 0;
                        }
                        else
                        {
                            _logger.LogError("DeleteUser Not Found");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                string message = "User " + user.Name + (result ? " Deleted" : "Could Not Deleted");
                _logger.LogInformation("DeleteUser\tParam: " + JsonConvert.SerializeObject(user) + "\tResult: " + result);
                await _dbLogger.logInfo(message, getUserName());
            }
            return result;
        }

        [HttpPost("Login")]
        public async Task<UiUserModel> Login([FromBody] JObject param)
        {
            var userName = param["UserName"];
            var password = param["Password"];

            UiUserModel result = null;
            try
            {
                if (userName != null && password != null)
                {
                    User user = null;
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        user = await (from m in context.Users where m.UserName == userName.ToString() && m.Password == password.ToString() select m).FirstOrDefaultAsync();
                    }

                    if (user != null)
                    {
                        result = new UiUserModel();
                        user.Password = "";
                        result.User = user;

                        using (var context = _contextFactory.CreateDbContext())
                        {
                            result.RoleList = await (from m in context.Roles
                                                     join mn in context.User_Roles on m.Id equals mn.RoleId
                                                     where mn.UserId == user.Id
                                                     select m).ToListAsync();
                        }
                        _logger.LogInformation("Login " + userName.ToString());

                        string message = "User " + userName + " Logged In";
                        _logger.LogInformation("Login\tParam: " + JsonConvert.SerializeObject(user) + "\tLogged In");
                        await _dbLogger.logInfo(message, getUserName());
                    }
                    else
                    {
                        string message = "User " + userName + " Could Not Logged In";
                        _logger.LogInformation("Login\tParam: " + JsonConvert.SerializeObject(param) + "\tCould Not Logged In");
                        await _dbLogger.logInfo(message, userName.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
            }

            return result;
        }

        [HttpPost("GetData")]
        public async Task<List<User>> GetData()
        {
            List<User> userList = new List<User>();
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com");

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            // List data response.
            HttpResponseMessage response = client.GetAsync("/users").Result;  // Blocking call! Program will wait here until a response is received or a timeout occurs.
            if (response.IsSuccessStatusCode)
            {
                userList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<User>>(response.Content.ReadAsStringAsync().Result);
            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }
            client.Dispose();
            return userList;
        }

        [HttpPost("TestData")]
        public async Task<bool> TestData([FromBody] JObject param)
        {
            try
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    context.Database.ExecuteSqlRaw("TRUNCATE TABLE aplus.Roles");
                    context.Database.ExecuteSqlRaw("TRUNCATE TABLE aplus.Projects");
                    context.Database.ExecuteSqlRaw("TRUNCATE TABLE aplus.Users");
                    context.Database.ExecuteSqlRaw("TRUNCATE TABLE aplus.Customers");
                    context.Database.ExecuteSqlRaw("TRUNCATE TABLE aplus.Address");
                    context.Database.ExecuteSqlRaw("TRUNCATE TABLE aplus.Customer_Projects");
                    context.Database.ExecuteSqlRaw("TRUNCATE TABLE aplus.Employees");
                    context.Database.ExecuteSqlRaw("TRUNCATE TABLE aplus.Groups");
                    context.Database.ExecuteSqlRaw("TRUNCATE TABLE aplus.Employee_Groups");


                    #region Roles
                    Role role1 = new Role()
                    {
                        Name = "Admin",
                        Description = "Administrator"
                    };
                    Role role2 = new Role()
                    {
                        Name = "Standart",
                        Description = "Standart"
                    };
                    context.Roles.Add(role1);
                    context.Roles.Add(role2);
                    #endregion

                    #region Projects
                    Project project1 = new Project()
                    {
                        Name = "Project1",
                        Description = "Description1"
                    };
                    Project project2 = new Project()
                    {
                        Name = "Project2",
                        Description = "Description2"
                    };
                    context.Projects.Add(project1);
                    context.Projects.Add(project2);
                    #endregion

                    #region Groups
                    Group group1 = new Group()
                    {
                        Id = 1,
                        Name = "Outsource Team"
                    };
                    Group group2 = new Group()
                    {
                        Id = 2,
                        Name = "Insource Team"
                    };
                    context.Groups.Add(group1);
                    context.Groups.Add(group2);
                    #endregion

                    var list = GetData().Result;

                    List<User> userList = new List<User>();
                    List<User_Role> user_RoleList = new List<User_Role>();
                    #region Users
                    for (int i = 0; i < list.Count; i++)
                    {
                        User user = new User()
                        {
                            Id = i,
                            UserName = "UserName" + i,
                            Name = list[i].Name.Split(" ")[0],
                            SurName = list[i].Name.Split(" ")[1],
                            Password = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(i.ToString())),
                            Status = 1
                        };
                        userList.Add(user);

                        User_Role user_Role = new User_Role()
                        {
                            UserId = i,
                            RoleId = 1
                        };
                        user_RoleList.Add(user_Role);
                    }
                    context.Users.AddRange(userList);
                    context.User_Roles.AddRange(user_RoleList);
                    #endregion

                    context.SaveChanges();

                    List<Employee> employeeList = new List<Employee>();
                    List<Employee_Group> employee_GroupList = new List<Employee_Group>();

                    #region Employees
                    for (int i = 0; i < list.Count; i++)
                    {
                        Employee employee = new Employee()
                        {
                            Id = i,
                            Name = list[i].Name.Split(" ")[0],
                            Surname = list[i].Name.Split(" ")[1],
                            UserName = list[i].UserName,
                            Email = list[i].Email,
                            Telephone = list[i].Phone
                        };
                        employeeList.Add(employee);
                        Employee_Group employee_Group = new Employee_Group()
                        {
                            EmployeeId = i,
                            GroupId = i < 6 ? 1 : 2
                        };
                        employee_GroupList.Add(employee_Group);
                    }
                    context.Employees.AddRange(employeeList);
                    context.Employee_Groups.AddRange(employee_GroupList);
                    #endregion

                    context.SaveChanges();


                    #region Customers
                    for (int i = 0; i < 100; i++)
                    {
                        list.AddRange(GetData().Result);
                    }

                    List<Customer> customerList = new List<Customer>();
                    List<Address> addressList = new List<Address>();
                    List<Customer_Project> customer_ProjectList = new List<Customer_Project>();

                    for (int i = 1; i < list.Count; i++)
                    {
                        Customer customer = new Customer()
                        {
                            Name = list[i].Name.Split(" ")[0],
                            Surname = list[i].Name.Split(" ")[1],
                            Email = "email" + i + "@email.com",
                            Telephone = list[i].Phone,
                            UserName = "UserName" + i,
                            RecordBase = Utils.RecordBase.Crm
                        };
                        customerList.Add(customer);                        

                        Address address1 = new Address()
                        {
                            Name = "Home",
                            City = "Dubai",
                            Country = "UAE",
                            CustomerId = i,
                            Detail = "Address Detail " + i
                        };

                        Address address2 = new Address()
                        {
                            Name = "Office",
                            City = "Dubai",
                            Country = "UAE",
                            CustomerId = i,
                            Detail = "Office Detail " + i
                        };
                        addressList.Add(address1);
                        addressList.Add(address2);                       

                        Customer_Project customer_Project1 = new Customer_Project()
                        {
                            ProjectId = 1,
                            CustomerId = i,
                        };

                        Customer_Project customer_Project2 = new Customer_Project()
                        {
                            ProjectId = 2,
                            CustomerId = i,
                        };

                        customer_ProjectList.Add(customer_Project1);
                        customer_ProjectList.Add(customer_Project2);                       
                    }
                    context.Customers.AddRange(customerList);
                    context.Address.AddRange(addressList);
                    context.Customer_Projects.AddRange(customer_ProjectList);

                    context.SaveChanges();
                    #endregion
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return true;
        }
        private string getUserName()
        {
            return HttpContext.Session.GetString("UserName");
        }

        [HttpPost("ReadExcel")]
        public async Task<bool> ReadExcel()
        {
            string con = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\Users\User\Downloads\Cubic.xlsx;" +
                          @"Extended Properties='Excel 8.0;HDR=Yes;'";
            using (OleDbConnection connection = new OleDbConnection(con))
            {
                connection.Open();
                OleDbCommand command = new OleDbCommand("select * from [Sheet1$]", connection);
                using (OleDbDataReader dr = command.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var row1Col0 = dr[0];
                        Console.WriteLine(row1Col0);
                    }
                }
            }
            return true;
        }
    }


}
