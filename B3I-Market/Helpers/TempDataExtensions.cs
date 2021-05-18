using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace B3I_Market.Helpers
{
    public static class TempDataExtensions
    {
        public static void AddOrUpdate(this ITempDataDictionary tempData,string key, string value)
        {
            if (tempData.Keys.Contains(key))
            {
                tempData.Remove(key);
            }
            tempData.Add(key, value);
        }
    }
}