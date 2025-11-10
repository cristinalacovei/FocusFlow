using Microsoft.AspNetCore.Identity;
using System.Diagnostics;

namespace FocusFlow.Api.Models
{
   
    public class AppUser : IdentityUser
    {
      

        public ICollection<Activity> Activities { get; set; } = new List<Activity>();
    }
}