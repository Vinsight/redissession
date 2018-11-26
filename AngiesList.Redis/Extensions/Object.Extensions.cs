using System.Collections.Generic;

namespace AngiesList.Redis.Extensions
{
    public static class ObjectExtensions
    {

        public static object Get(this IDictionary<string, object> obj, string key)
        {
            return obj.ContainsKey(key) ? obj[key] : null;
        }
        /*public static object Get(this object obj, string key)
        {
            var dictionary = obj.ToDictionary();
            return obj.HasProperty(key) ? dictionary[key] : null;
        }

         * */

        //helper
        public static IDictionary<string, object> ToDictionary(this object obj)
        {
            IDictionary<string, object> result = new Dictionary<string, object>();
            var properties = obj.GetType().GetProperties();
            foreach (var property in properties)
            {
                result.Add(property.Name, property.GetValue(obj));
            }
            return result;
        }
    }

}
