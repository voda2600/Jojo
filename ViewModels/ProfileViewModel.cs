namespace ViewModels
{
    public class ProfileViewModel
    {
        #region Filters
        public string UserName { get; set; }
        public string SortType { get; set; }
        public string Status { get; set; }

        public int Id { get; set; } 
        #endregion

        #region PageSettings
        public int Page { get; set; }
        public int Qty { get; set; }
        
        #endregion
    }
    
}