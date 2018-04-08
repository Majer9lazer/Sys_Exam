using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App_ClassLibrary
{
    public class User
    {
        public int UserRandomNumber { get; set; }
        public string UserName { get; set; }
        public string UserNumber { get; set; }
        public string AppId { get; set; }

    }
    public class UserComparer : IEqualityComparer<User>
    {
        public bool Equals(User x, User y)
        {
            return x != null && y != null && x.UserRandomNumber.Equals(y.UserRandomNumber);
        }

        public int GetHashCode(User obj)
        {
            return obj.UserRandomNumber.GetHashCode();
        }
    }
}
