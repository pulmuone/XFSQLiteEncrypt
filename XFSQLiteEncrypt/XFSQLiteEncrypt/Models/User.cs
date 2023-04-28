using System;
using System.Collections.Generic;
using System.Text;

namespace XFSQLiteEncrypt.Models
{
    public class User
    {
        public int Id { get; set; }
        public string UserId{ get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
