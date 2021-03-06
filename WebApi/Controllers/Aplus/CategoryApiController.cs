using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Data;
using WebApi.DbModels;
using Newtonsoft.Json;
using WebApi.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WebApi.Controllers.Aplus;

namespace WebApi.Aplus.Controllers
{
    [Route("aplus/[controller]")]
    [ApiController]
    public class CategoryApiController : BaseApiController
    {
        private readonly ILogger<CategoryApiController> _logger;
        private readonly IDbContextFactory<CrmDbContext> _contextFactory;
        private DbLogger _dbLogger;
        public CategoryApiController(ILogger<CategoryApiController> logger, IDbContextFactory<CrmDbContext> contextFactory)
        {
            _logger = logger;
            _contextFactory = contextFactory;
            _dbLogger = new DbLogger(_contextFactory);
        }

        [HttpPost("GetCategoryList")]
        public async Task<List<Category>> GetCategoryList([FromBody] JObject param)
        {
            List<Category> list = new List<Category>();
            try
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    list = await (from m in context.Categories select m).ToListAsync();
                }
                _logger.LogInformation("GetCategoryList Count:" + list.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return list;
        }

        [HttpPost("AddCategory")]
        public async Task<bool> AddCategory([FromBody] Category category)
        {
            _logger.LogInformation("Category: " + JsonConvert.SerializeObject(category));
            bool result = false;
            if (category != null)
            {
                try
                {
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        category.CreatedDate = DateTime.UtcNow;
                        category.UpdateDate = DateTime.UtcNow;
                        var dbResult = context.Categories.Add(category);
                        await context.SaveChangesAsync();
                        result = dbResult != null;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                string message = "Category " + category.Name + (result ? " Added" : "Could Not Added");
                _logger.LogInformation("AddCategory\tParam: " + JsonConvert.SerializeObject(category) + "\tResult: " + result);
                await _dbLogger.logInfo(message, GetUserName());
            }
            return result;
        }

        [HttpPost("UpdateCategory")]
        public async Task<bool> UpdateCategory([FromBody] Category category)
        {
            bool result = false;
            if (category != null)
            {
                try
                {
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        var existing = context.Categories.FirstOrDefault(o => o.Id == category.Id);
                        if (existing != null)
                        {
                            existing.ParentId = category.ParentId;
                            existing.Name = category.Name;
                            existing.Description = category.Description;
                            existing.Image = category.Image;
                            existing.Price = category.Price;
                            existing.UpdateDate = DateTime.UtcNow;
                            int dbResult = await context.SaveChangesAsync();
                            result = dbResult > 0;
                        }
                        else
                        {
                            _logger.LogError("UpdateCategory Not Found");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                string message = "Category " + category.Name + (result ? " Updated" : "Could Not Updated");
                _logger.LogInformation("UpdateCategory\tParam: " + JsonConvert.SerializeObject(category) + "\tResult: " + result);
                await _dbLogger.logInfo(message, GetUserName());
            }
            _logger.LogInformation("UpdateCategory Result:" + result);
            return result;
        }

        [HttpPost("DeleteCategory")]
        public async Task<bool> DeleteCategory([FromBody] Category category)
        {
            bool result = false;
            if (category != null)
            {
                try
                {
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        var existing = context.Categories.FirstOrDefault(o => o.Id == category.Id);
                        var existingWorkList = context.Works.Where(o => o.CategoryId == category.Id).ToList();
                        if (existing != null)
                        {
                            foreach (var work in existingWorkList)
                            {
                                work.CategoryId = 0;
                            }

                            context.Categories.Remove(existing);
                            int dbResult = await context.SaveChangesAsync();
                            result = dbResult > 0;
                        }
                        else
                        {
                            _logger.LogError("DeleteCategory Not Found");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                string message = "Category " + category.Name + (result ? " Deleted" : "Could Not Deleted");
                _logger.LogInformation("DeleteCategory\tParam: " + JsonConvert.SerializeObject(category) + "\tResult: " + result);
                await _dbLogger.logInfo(message, GetUserName());
            }
            return result;
        }

        [HttpPost("GetOptionList")]
        public async Task<List<Option>> GetOptionList([FromBody] JObject param)
        {
            List<Option> list = new List<Option>();
            try
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    list = await (from m in context.Options select m).ToListAsync();
                }
                _logger.LogInformation("GetOptionList Count:" + list.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return list;
        }

        //[HttpPost("GetCategoryOptionList")]
        //public async Task<List<Category_Option>> GetCategoryOptionList([FromBody] JObject param)
        //{
        //    List<Category_Option> list = new List<Category_Option>();
        //    try
        //    {
        //        using (var context = _contextFactory.CreateDbContext())
        //        {
        //            list = await (from m in context.Category_Options select m).ToListAsync();
        //        }
        //        _logger.LogInformation("GetCategoryOptionList Count:" + list.Count);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex.Message);
        //    }
        //    return list;
        //}

        [HttpPost("GetCategoryOptionList")]
        public async Task<List<Option>> GetCategoryOptionList([FromBody] JObject param)
        {
            List<Option> list = new List<Option>();
            var cat = param["CategoryId"];
            try
            {
                if (cat != null)
                {
                    int categoryId = Convert.ToInt32(cat);
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        list = await (from m in context.Options
                                      join n in context.Category_Options on m.Id equals n.OptionId
                                      where n.CategoryId == categoryId
                                      select m).ToListAsync();
                    }
                }
                _logger.LogInformation("GetCategoryOptionList Count:" + list.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return list;
        }

        [HttpPost("Cat")]
        public async Task<bool> Cat([FromBody] JObject param)
        {
            string json = "";

            //dynamic h = JsonConvert.DeserializeObject(json);
            JArray array = JsonConvert.DeserializeObject<JArray>(json);

            try
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    foreach (JObject item in array)
                    {
                        var v = item["name"];
                        var existing = context.Categories.FirstOrDefault(o => o.Name == v.ToString());
                        if (existing != null)
                        {
                            existing.Description = item["description"].ToString();
                            int dbResult = await context.SaveChangesAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }


            return true;
        }
    }
}
