using System.Text;
using Newtonsoft.Json;

namespace Shard
{
    /// <summary>
    /// Static class to serialize and deserialize objects
    /// </summary>
    public static class SerializationHelper
    {
        /// <summary>
        /// Serializes the specified object into a byte array.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <returns></returns>
        public static byte[] Serialize(object o)
        {
            var data = JsonConvert.SerializeObject(o);
            return Encoding.UTF8.GetBytes(data);
        }

        /// <summary>
        /// Deserializes the specified bytes back into the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes">The bytes.</param>
        /// <returns></returns>
        public static T Deserialize<T>(byte[] bytes)
        {
            var str = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<T>(str);
        }
    }
}