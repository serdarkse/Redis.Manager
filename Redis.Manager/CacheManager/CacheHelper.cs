using Newtonsoft.Json;
using System.Text;

namespace Redis.Manager.CacheManager
{
    public class CacheHelper
    {
        protected virtual byte[] Serialize(object item)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(item));
        }

        protected virtual T Deserialize<T>(string[] serializedObject)
        {
            if (serializedObject == null)
                return default(T);
            string str1 = "";
            if (serializedObject[0][0] != '[')
                str1 += "[";
            foreach (string str2 in serializedObject)
                str1 = str1 + str2 + ",";
            if (str1.Length > 0)
                str1 = str1.Substring(0, str1.Length - 1);
            if (serializedObject[serializedObject.Length - 1][serializedObject[serializedObject.Length - 1].Length - 1] != ']')
                str1 += "]";
            return JsonConvert.DeserializeObject<T>(str1);
        }
    }
}
