using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JETWebApplication.Controllers
{
    public class Payload
    {
        //使用者資訊
        public User info { get; set; }
        //過期時間
        public int exp { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        
        public string UserId { get; set; }

        public string UserName { get; set; }
    }
}
