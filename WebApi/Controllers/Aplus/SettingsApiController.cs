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
using WebApi.Utils;
using Microsoft.AspNetCore.Http;
using WebApi.Controllers.Aplus;

namespace WebApi.Aplus.Controllers
{
    [Route("aplus/[controller]")]
    [ApiController]
    public class WebSettingsApiController : BaseApiController
    {
        private readonly ILogger<WebSettingsApiController> _logger;
        private readonly IDbContextFactory<CrmDbContext> _contextFactory;
        private DbLogger _dbLogger;
        private int defaultType = 0, normalType = 1;

        public WebSettingsApiController(ILogger<WebSettingsApiController> logger, IDbContextFactory<CrmDbContext> contextFactory)
        {
            _logger = logger;
            _contextFactory = contextFactory;
            _dbLogger = new DbLogger(_contextFactory);
        }

        [HttpPost("GetWebSettings")]
        public async Task<WebSettings> GetWebSettings([FromBody] JObject param)
        {
            var settingsType = param["Type"];
            int sType = normalType;
            if (settingsType!=null)
            {
                sType = Convert.ToInt32(settingsType);
            }

            WebSettings ws = new WebSettings();
            try
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    ws = await (from m in context.WebSettings where m.Type == sType select m).FirstOrDefaultAsync();
                }
                _logger.LogInformation("GetWebSettingsList Count:" + JsonConvert.SerializeObject(ws));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return ws;
        }

        [HttpPost("UpdateWebSettings")]
        public async Task<bool> UpdateWebSettings([FromBody] WebSettings ws)
        {
            bool result = false;
            if (ws != null)
            {
                try
                {
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        var existing = context.WebSettings.FirstOrDefault(o => o.Id == ws.Id);
                        if (existing != null)
                        {
                            existing.DefaultPadding = ws.DefaultPadding;
                            existing.TitleColor = ws.TitleColor;
                            existing.MenuColor = ws.MenuColor;
                            existing.PrimaryColor = ws.PrimaryColor;
                            existing.SecondaryColor = ws.SecondaryColor;
                            existing.TextColor = ws.TextColor;
                            existing.UpdateDate = DateTime.UtcNow;
                            int dbResult = await context.SaveChangesAsync();
                            result = dbResult > 0;
                        }
                        else
                        {
                            _logger.LogError("UpdateWebSettings Not Found");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }

                string message = "WebSettings " + (result ? " Updated" : "Could Not Updated");
                _logger.LogInformation("UpdateWebSettings\tParam: " + JsonConvert.SerializeObject(ws) + "\tResult: " + result);
                await _dbLogger.logInfo(message, GetUserName());

            }
            _logger.LogInformation("UpdateWebSettings Result:" + result);
            return result;
        }

        [HttpPost("GetLogList")]
        public async Task<List<Log>> GetLogList([FromBody] JObject param)
        {
            List<Log> resultList = new List<Log>();
            var days = param["Days"];

            DateTime startDate = DateTime.Today.AddDays(-7);
            if (days != null)
            {
                startDate = DateTime.Today.AddDays(-Convert.ToInt32(days));
            }

            try
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    resultList = await (from m in context.Logs where m.CreatedDate > startDate select m).ToListAsync();
                }
                _logger.LogInformation("GetLogList Count:" + resultList.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return resultList;
        }
    }
}
