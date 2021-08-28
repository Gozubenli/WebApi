using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class BaseModel
    {
        public int Id { get; set; }

        private DateTime createdDate;
        public DateTime CreatedDate
        {
            get
            {
                if(createdDate == DateTime.MinValue)
                {
                    createdDate = DateTime.Now;
                }
                return createdDate;
            }
            set
            {
                createdDate = value;
            }
        }

        private DateTime updateDate;
        public DateTime UpdateDate
        {
            get
            {
                if (updateDate == DateTime.MinValue)
                {
                    updateDate = DateTime.Now;
                }
                return updateDate;
            }
            set
            {
                updateDate = value;
            }
        }        
    }
}

