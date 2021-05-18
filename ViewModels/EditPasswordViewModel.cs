using System.ComponentModel.DataAnnotations;

namespace ViewModels
{
    public class EditPasswordViewModel
    {
        public string UserName { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "New password REQUIRED!")]
        public string NewPassword { get; set; }
        [DataType(DataType.Password), Required(ErrorMessage = "You have to confirm password")]
        [Compare("NewPassword", ErrorMessage = "Passwords don't match")]
        public string ConfirmPassword { get; set; }
    }
}