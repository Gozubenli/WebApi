using Microsoft.AspNetCore.Http;
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
        private CrmDbContext _db;
        public DbLogger(CrmDbContext db)
        {
            _db = db;
        }

        public async Task logInfo(string message, string userName)
        {
            await _db.Logs.AddAsync(new Log() { Message=message, UserName=userName, Level="Info"});
            await _db.SaveChangesAsync();
        }
    }
}
