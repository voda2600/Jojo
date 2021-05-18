using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace B3I_Market.Helpers
{

    public static class SessionExtensions
    {
        public static void SetOrUpdate<T>(this ISession session, string key, T value)
        {
            if (session.Keys.Contains(key))
            {
                session.Remove(key);
            }
            var a = JsonSerializer.Serialize<T>(value);
            session.SetString(key, a);
        }
        public static T Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) : JsonSerializer.Deserialize<T>(value);
        }
        


    }
}