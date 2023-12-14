using System.Collections.Generic;
using System.IO;    
using System.Runtime.Serialization.Formatters.Binary;

namespace EasySocket
{
    public class Data
    {
        public Data()
        {
            data = new Dictionary<string, object>();
        }

        public void Put(string key, object value)
        {
            data.Add(key, value);
        }
        public object Read(string key)
        {
            return data[key];
        }
        public int ReadInt(string key)
        {
            return int.Parse(data[key].ToString());
        }
        public ulong ReadUlong(string key)
        {
            return ulong.Parse(data[key].ToString());
        }
        public string ReadString(string key)
        {
            return data[key].ToString();
        }
        public byte[] ReadBytes(string key)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, data[key]);
                return ms.ToArray();
            }
        }

        internal Dictionary<string, object> data;
    }
}
