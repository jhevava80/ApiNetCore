using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ApplicationApi.Dtos
{
    public class UserForRegisterDto
    {
        [Required]
        public string username { get; set; }
        [Required]
        [StringLength(12,MinimumLength =4,ErrorMessage ="You must specifiy between 4 to 12 characters")]
        public string password { get; set; }
    }
}
