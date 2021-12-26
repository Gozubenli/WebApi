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
using System.Diagnostics;
using WebApi.Controllers.Aplus;

namespace WebApi.Aplus.Controllers
{
    [Route("aplus/[controller]")]
    [ApiController]
    public class UserApiController : BaseApiController
    {
        private readonly ILogger<AddressApiController> _logger;
        private readonly IDbContextFactory<CrmDbContext> _contextFactory;
        private DbLogger _dbLogger;

        private static readonly Random random = new Random();
        private static readonly object syncLock = new object();


        public UserApiController(ILogger<AddressApiController> logger, IDbContextFactory<CrmDbContext> contextFactory)
        {
            _logger = logger;
            _contextFactory = contextFactory;
            _dbLogger = new DbLogger(_contextFactory);
        }

        /*
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
                await _dbLogger.logInfo(message, GetUserName());
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
                await _dbLogger.logInfo(message, GetUserName());
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
                await _dbLogger.logInfo(message, GetUserName());
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
                        await _dbLogger.logInfo(message, GetUserName());
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
        */

        [HttpPost("GetData")]
        public async Task<List<TestData>> GetData()
        {
            List<TestData> list = new List<TestData>();
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com");

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            // List data response.
            HttpResponseMessage response = client.GetAsync("/users").Result;  // Blocking call! Program will wait here until a response is received or a timeout occurs.
            if (response.IsSuccessStatusCode)
            {
                list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TestData>>(response.Content.ReadAsStringAsync().Result);                
            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }
            client.Dispose();
            return list;
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
                    //context.Database.ExecuteSqlRaw("TRUNCATE TABLE aplus.Users");
                    context.Database.ExecuteSqlRaw("TRUNCATE TABLE aplus.Customers");
                    context.Database.ExecuteSqlRaw("TRUNCATE TABLE aplus.Address");
                    context.Database.ExecuteSqlRaw("TRUNCATE TABLE aplus.Customer_Projects");
                    context.Database.ExecuteSqlRaw("TRUNCATE TABLE aplus.Employees");
                    context.Database.ExecuteSqlRaw("TRUNCATE TABLE aplus.Groups");
                    context.Database.ExecuteSqlRaw("TRUNCATE TABLE aplus.Employee_Groups");
                    context.Database.ExecuteSqlRaw("TRUNCATE TABLE aplus.Works");
                    context.Database.ExecuteSqlRaw("TRUNCATE TABLE aplus.Employee_Works");
                    context.Database.ExecuteSqlRaw("TRUNCATE TABLE aplus.Titles");
                    context.Database.ExecuteSqlRaw("TRUNCATE TABLE aplus.Employee_Roles");
                    context.Database.ExecuteSqlRaw("TRUNCATE TABLE aplus.Categories");
                    context.SaveChanges();

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
                    Project project3 = new Project()
                    {
                        Name = "Project23",
                        Description = "Description3"
                    };
                    Project project4 = new Project()
                    {
                        Name = "Project4",
                        Description = "Description4"
                    };
                    Project project5 = new Project()
                    {
                        Name = "Project5",
                        Description = "Description5"
                    };
                    context.Projects.Add(project1);
                    context.Projects.Add(project2);
                    context.Projects.Add(project3);
                    context.Projects.Add(project4);
                    context.Projects.Add(project5);
                    #endregion

                    #region Groups
                    Group group0 = new Group()
                    {
                        Name = "Undefined"
                    };
                    Group group1 = new Group()
                    {
                        Name = "Outsource Team"
                    };
                    Group group2 = new Group()
                    {
                        Name = "Inhouse Team"
                    };
                    context.Groups.Add(group0);
                    context.Groups.Add(group1);
                    context.Groups.Add(group2);

                    #endregion
                    context.SaveChanges();
                    var list = GetData().Result;

                    /*
                    List<User> userList = new List<User>();
                    List<User_Role> user_RoleList = new List<User_Role>();

                    #region Users
                    for (int i = 0; i < list.Count; i++)
                    {
                        User user = new User()
                        {
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
                    context.SaveChanges();
                    context.User_Roles.AddRange(user_RoleList);
                    context.SaveChanges();
                    #endregion                    
                    */

                    List<Employee> employeeList = new List<Employee>();
                    List<Employee_Group> employee_GroupList = new List<Employee_Group>();
                    List<Employee_Role> employee_RoleList = new List<Employee_Role>();

                    int id = 1;

                    #region Titles
                    string[] titles = { "Undefined", "Electirician", "Cleaner", "Sales Manager", "Customer Services", "AC Mechanic", "Carpenter" };
                    List<Title> titleList = new List<Title>();
                    foreach (var item in titles)
                    {
                        titleList.Add(new Title() { Name = item });
                    }
                    context.Titles.AddRange(titleList);
                    context.SaveChanges();

                    #endregion

                    #region Employees
                    for (int j = 1; j < 4; j++)
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            id++;
                            Employee employee = new Employee()
                            {
                                Id = id,
                                Name = list[i].Name.Split(" ")[0],
                                Surname = list[i].Name.Split(" ")[1],
                                Password = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(id.ToString())),
                                Email = list[i].Email,
                                Telephone = list[i].Phone,
                                ImageName = GetRandomImage(),
                                TitleId = GetRandomTitleId(titles)
                            };
                            employeeList.Add(employee);
                            Employee_Group employee_Group = new Employee_Group()
                            {
                                EmployeeId = id,
                                GroupId = id < (list.Count * 2) ? 2 : 3
                            };

                            Employee_Role user_Role = new Employee_Role()
                            {
                                EmployeeId = i,
                                RoleId = 1
                            };
                            employee_RoleList.Add(user_Role);
                            employee_GroupList.Add(employee_Group);
                        }
                    }
                    context.Employees.AddRange(employeeList);
                    context.Employee_Groups.AddRange(employee_GroupList);
                    context.Employee_Roles.AddRange(employee_RoleList);
                    context.SaveChanges();
                    #endregion                    

                    #region Customers
                    for (int i = 1; i < 101; i++)
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
                        Customer_Project customer_Project3 = new Customer_Project()
                        {
                            ProjectId = 3,
                            CustomerId = i,
                        };
                        Customer_Project customer_Project4 = new Customer_Project()
                        {
                            ProjectId = 4,
                            CustomerId = i,
                        };
                        Customer_Project customer_Project5 = new Customer_Project()
                        {
                            ProjectId = 5,
                            CustomerId = i,
                        };

                        if (i % 2 == 0)
                        {
                            customer_ProjectList.Add(customer_Project1);
                        }
                        else if (i % 3 == 0)
                        {
                            customer_ProjectList.Add(customer_Project3);
                        }
                        else if (i % 4 == 0)
                        {
                            customer_ProjectList.Add(customer_Project4);
                        }
                        else if (i % 5 == 0)
                        {
                            customer_ProjectList.Add(customer_Project5);
                        }
                        else
                        {
                            customer_ProjectList.Add(customer_Project2);
                        }
                    }
                    context.Customers.AddRange(customerList);
                    context.Address.AddRange(addressList);
                    context.Customer_Projects.AddRange(customer_ProjectList);

                    context.SaveChanges();
                    #endregion

                    #region Categories
                    List<Category> categorieList = new List<Category>();
                    for (int i = 1; i <= 10; i++)
                    {
                        Category category = new Category()
                        {
                            Name = "Category" + i,
                            ParentId = 0
                        };
                        categorieList.Add(category);

                        for (int j = 1; j < 5; j++)
                        {
                            Category sub = new Category()
                            {
                                Name = "Sub Category" + j,
                                ParentId = i
                            };
                            categorieList.Add(sub);
                        }
                    }
                    context.Categories.AddRange(categorieList);
                    context.SaveChanges();

                    #endregion

                    #region WorkList

                    List<Work> workList = new List<Work>();
                    List<Employee_Work> employee_WorkList = new List<Employee_Work>();
                    int workId = 1;

                    for (int i = 0; i < employeeList.Count; i++)
                    {
                        for (int j = 0; j < 5; j++)
                        {
                            Work work = GetRandomWork(workId++, employeeList);
                            workList.Add(work);
                            Employee_Work employee_Work = new Employee_Work()
                            {
                                EmployeeId = employeeList[i].Id,
                                WorkId = work.Id,
                            };
                            employee_WorkList.Add(employee_Work);
                        }
                    }
                    context.Works.AddRange(workList);
                    context.Employee_Works.AddRange(employee_WorkList);
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

        private Work GetRandomWork(int id, List<Employee> list)
        {
            Employee usr = list[RandomNumber(1, 10)];
            return new Work()
            {
                Id = id,
                CategoryId = RandomNumber(1, 5),
                CustomerId = RandomNumber(1, 11),
                ProjectId = RandomNumber(1, 2),
                Title = usr.Name + " " + usr.Surname + "'s Work Order " + usr.Telephone,
                Detail = "Detail about work order " + usr.Telephone + " ...",
                EmergencyStatus = id % 7 == 0 ? EmergencyStatus.Emergency : EmergencyStatus.Normal,
                WorkStatus = GetRandomWorkStatus(),
                WorkTime = GetRandomWorkTime(),
                WorkType = id % 5 == 0 ? WorkType.Recurring : WorkType.Regular
            };
        }

        private WorkStatus GetRandomWorkStatus()
        {
            int i = RandomNumber(1, 5);
            switch (i)
            {
                case 1:
                    return WorkStatus.New;
                case 2:
                    return WorkStatus.Completed;
                case 3:
                    return WorkStatus.InProgress;
                case 4:
                    return WorkStatus.Planned;
                default:
                    return WorkStatus.Completed;
            }
        }

        private WorkTime GetRandomWorkTime()
        {
            int i = RandomNumber(1, 5);
            switch (i)
            {
                case 1:
                    return WorkTime.T_09_10;
                case 2:
                    return WorkTime.T_12_13;
                case 3:
                    return WorkTime.T_13_14;
                case 4:
                    return WorkTime.T_17_18;
                default:
                    return WorkTime.T_09_10;
            }
        }

        private int GetRandomTitleId(string[] arr)
        {
            var i = RandomNumber(0, arr.Length);
            return i;
        }

        private string GetRandomImage()
        {
            var i = RandomNumber(1, 4) + 1;
            return i + (i < 3 ? ".jpg" : ".png");
        }

        public static int RandomNumber(int min, int max)
        {
            lock (syncLock)
            {
                var ran = random.Next(min, max);
                Debug.WriteLine(ran);
                return ran;
            }
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

    /// <summary>
    /// Test Data
    /// </summary>
    public class TestData
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }
    }



}
