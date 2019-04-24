using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace jb_core_webapi.Models
{
    public class UserNewPassword
    {
        [MaxLength(64)]
        public string Username { get; set; }

        [MaxLength(128)]
        public string OldPassword { get; set; }

        [MaxLength(128)]
        public string NewPassword { get; set; }
    }
}
