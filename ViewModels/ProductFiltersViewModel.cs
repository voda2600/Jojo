using System.Collections.Generic;

namespace ViewModels
{
    public class ProductFiltersViewModel
    {
        public string Category { get; set; }
        public double? MinPrice { get; set; }
        
        public string SortPrice { get; set; }
        
        public string SortRating { get; set; }

        public string inputSearch { get; set; } = null;
        
        public double? MaxPrice { get; set; }
        
        public double? MinRating { get; set; }
        
        public double? MaxRating { get; set; }
        public List<FilterUnit> Filters { get; set; }
    }

    public class FilterUnit
    {
        public string Type { get; set; }
        public string Name { get; set; }
        
        
        public List<string> Values { get; set; }
        public double? MaxValue { get; set; }
        public double? MinValue { get; set; }
        public bool? CheckBox { get; set; }
    }
}