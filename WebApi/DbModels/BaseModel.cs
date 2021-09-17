using Newtonsoft.Json;
using System;

namespace WebApi.DbModels
{
    public class BaseModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }       
        private DateTime createdDate = DateTime.UtcNow;
        [JsonProperty("cd")]
        public DateTime CreatedDate
        {
            get
            {
                if (createdDate == DateTime.MinValue)
                {
                    createdDate = DateTime.UtcNow;
                }
                return createdDate;
            }
            set
            {
                if (value == DateTime.MinValue)
                {
                    createdDate = DateTime.UtcNow;
                }
                else
                {
                    createdDate = value;
                }
            }
        }

        private DateTime updateDate = DateTime.UtcNow;
        [JsonProperty("ud")]
        public DateTime UpdateDate
        {
            get
            {
                if (updateDate == DateTime.MinValue)
                {
                    updateDate = DateTime.UtcNow;
                }
                return updateDate;
            }
            set
            {
                if (value == DateTime.MinValue)
                {
                    updateDate = DateTime.UtcNow;
                }
                else
                {
                    updateDate = value;
                }
            }
        }
    }
}

