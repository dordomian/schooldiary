using Newtonsoft.Json;
using SchoolDiary.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;

namespace SchoolDiary.Services
{
    public class LogService
    {
        const int USER_ANONYMOUS_ID = 1;
        const string USER_ANONYMOUS = "anonymous";

        public Log prepareLogWithData(DbEntityEntry entry)
        {
            int userId = LogService.USER_ANONYMOUS_ID;
            string userName = LogService.USER_ANONYMOUS;
            byte[] arrayNow = BitConverter.GetBytes(System.DateTime.Now.ToBinary());

            Dictionary<string, string> originalObjectValues = new Dictionary<string, string>();
            Dictionary<string, string> currentObjectValues = new Dictionary<string, string>();

            foreach (var property in entry.Entity.GetType().GetProperties())
            {
                try { originalObjectValues.Add(property.Name, entry.OriginalValues[property.Name].ToString()); } catch { }
                try { currentObjectValues.Add(property.Name, entry.CurrentValues[property.Name].ToString()); } catch { }
            }

            Dictionary<string, Dictionary<string, string>> dictValues = new Dictionary<string, Dictionary<string, string>>();

            dictValues.Add("original_values", originalObjectValues);
            dictValues.Add("current_values", currentObjectValues);

            string changes = JsonConvert.SerializeObject(dictValues);

            string entityName = entry.Entity.GetType().FullName;
            string entityStatus = entry.State.ToString();

            int rowId = 0;

            try { rowId = Convert.ToInt32(entry.Property("ID").CurrentValue.ToString()); } catch { };

            return new Log
            {
                UserId = userId,
                UserName = userName,
                EntityName = entityName,
                EntityStatus = entityStatus,
                RowId = rowId,
                Changes = changes,
                Timestamp = arrayNow
            };
        }
    }
}