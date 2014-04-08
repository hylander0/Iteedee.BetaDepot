using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Iteedee.BetaDepot.Models
{
    public class Administration
    {
        public class IndexViewModel
        {
        }

        public class UsersViewModel
        {
            public UsersViewModel()
            {
                AppUsers = new List<Users>();
            }
            public List<Users> AppUsers { get; set; }
            public List<String> Roles { get; set; }


            public class Users
            {
                public String GravitarUrl { get; set; }
                public String Name { get; set; }
                public String UserName { get; set; }
                public String EmailAddress { get; set; }
                public String SystemRole { get; set; }
                public int AppMemberCount { get; set; }

            }
        }
    }

}