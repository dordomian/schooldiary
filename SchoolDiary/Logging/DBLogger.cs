using Newtonsoft.Json;
using SchoolDiary.DAL;
using SchoolDiary.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using SchoolDiary.Services;
using System.Data.Entity.Infrastructure;
using System.Web;

namespace SchoolDiary.Logging
{
    public class DBLogger
    {
        private SchoolContext dbContext = null;
        private LogService _logService = null;

        public DBLogger(SchoolContext dbContext)
        {
            this.dbContext = dbContext;
            this._logService = new LogService();
        }

        public async Task LoggingProcess()
        {
            await LogChanges();
        }

        private async Task LogChanges()
        {
            var modifiedEntities = dbContext.ChangeTracker.Entries().Where(x => x.State != EntityState.Unchanged).ToList();

            foreach (var entry in modifiedEntities)
            {
                dbContext.Logs.Add(_logService.prepareLogWithData(entry));
                dbContext.baseSaveChanges();
            }

        }
      
    }
}