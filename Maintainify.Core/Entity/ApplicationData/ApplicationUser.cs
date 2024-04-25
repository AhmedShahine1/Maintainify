using Maintainify.Core.Entity.ProfessionData;
using Maintainify.Core.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maintainify.Core.Entity.ApplicationData
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsAdmin { get; set; } = false; //if true, user is admin
        public bool Status { get; set; } = true; //true=active, false=disactive
        //-------------------------------------------------------------------
        public string FullName { get; set; }
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        public string Description { get; set; }
        public string bankAccountNumber { get; set; }
        public float? Lat { get; set; }
        public float? Lng { get; set; }
        public string RandomCode { get; set; }
        //------------------------------------------------------------------------------------------------
        public Profession profession { get; set; } = new Profession();
        public List<Images> images { get; set; } = new List<Images> ();
        //------------------------------------------------------------------------------------------------
        [NotMapped]
        public List<IFormFile> UserImgFile { get; set; }

    }
}
