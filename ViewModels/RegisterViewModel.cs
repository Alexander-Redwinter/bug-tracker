using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BugTracker.ViewModels
{
    public class RegisterViewModel
    {


        [Required]
        [EmailAddress]
        [Remote(action:"IsEmailInUse","Account")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

    }
}
