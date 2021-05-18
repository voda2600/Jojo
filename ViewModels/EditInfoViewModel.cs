using System.ComponentModel.DataAnnotations;

namespace ViewModels
{
    public class EditInfoViewModel
    {
        [EmailAddress(ErrorMessage = "This should be an email")]
        public string NewEmail { get; set; }
        public string UserName { get; set; }
    }
}