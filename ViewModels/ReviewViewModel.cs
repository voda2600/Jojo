using System.ComponentModel.DataAnnotations;

namespace ViewModels
{
    public class ReviewViewModel
    {
        public string Review { get; set; }
        [Required]
        public int Qty { get; set; }
        
        public string Product { get; set; }
        
        public string UserName { get; set; }
    }
}