using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.DbModels
{
    public class BaseModel
    {
        public int Id { get; set; }

        private DateTime createdDate = DateTime.UtcNow;
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
            }
        }

        private DateTime updateDate = DateTime.UtcNow;
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
            }
        }
    }
}

