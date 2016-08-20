using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebapiStarter.Models
{
    public class UserModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public bool HasPassword { get; set; }
    }
}
