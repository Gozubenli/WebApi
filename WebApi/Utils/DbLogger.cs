using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Data;
using WebApi.DbModels;

namespace WebApi.Utils
{
    public class DbLogger
    {
        private readonly IDbContextFactory<CrmDbContext> _contextFactory;
        public DbLogger(IDbContextFactory<CrmDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task logInfo(string message, string userName)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                await context.Logs.AddAsync(new Log() { Message = message, UserName = userName, Level = "Info" });
                await context.SaveChangesAsync();
            }
        }
    }
}
