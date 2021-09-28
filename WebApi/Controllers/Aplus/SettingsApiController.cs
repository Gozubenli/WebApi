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

namespace WebApi.Aplus.Controllers
{
    [Route("aplus/[controller]")]
    [ApiController]
    public class WebSettingsApiController : ControllerBase
    {
        private readonly ILogger<WebSettingsApiController> _logger;
        private readonly IDbContextFactory<CrmDbContext> _contextFactory;
        private DbLogger _dbLogger;
        public WebSettingsApiController(ILogger<WebSettingsApiController> logger, IDbContextFactory<CrmDbContext> contextFactory)
        {
            _logger = logger;
            _contextFactory = contextFactory;
            _dbLogger = new DbLogger(_contextFactory);
        }

        [HttpPost("GetWebSettings")]
        public async Task<WebSettings> GetWebSettings([FromBody] JObject param)
        {
            WebSettings ws = new WebSettings();
            try
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    ws = await (from m in context.WebSettings select m).FirstOrDefaultAsync();
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
                await _dbLogger.logInfo(message, getUserName());

            }
            _logger.LogInformation("UpdateWebSettings Result:" + result);
            return result;
        }
        private string getUserName()
        {
            return HttpContext.Session.GetString("UserName");
        }
    }
}
