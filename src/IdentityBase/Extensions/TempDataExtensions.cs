namespace IdentityBase.Extensions
{
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using Newtonsoft.Json;
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Text;

    public static class TempDataExtensions
    {
        public static bool TryLoadData<T>(
            this ITempDataDictionary tempData,
            out T data)
        {
            data = default(T);

            string entryKey = typeof(T).Name;

            if (tempData.ContainsKey(entryKey))
            {
                // get value from temp data
                string value = tempData[entryKey] as string;

                // decode data
                byte[] bytes = Convert.FromBase64String(value);

                //decompress
                value = Decompress(bytes);

                // deserialize
                data = JsonConvert.DeserializeObject<T>(value);

                // remove entry from temp data
                tempData.Remove(entryKey);

                return true;
            }

            return false;
        }

        public static void StoreData<T>(this ITempDataDictionary tempData, T data)
        {
            if (data == null)
            {
                return;
            }

            string entryKey = data.GetType().Name;

            // serialize data
            string value = JsonConvert.SerializeObject(data);

            // compress the data (it really helps)
            byte[] bytes = Compress(value);

            if (bytes != null)
            {
                // encode data
                value = Convert.ToBase64String(bytes);

                // store value into temp data
                tempData[entryKey] = value;
            }
        }

        private static byte[] Compress(string value)
        {
            if (value == null)
            {
                return null;
            }

            byte[] data = Encoding.UTF8.GetBytes(value);

            using (MemoryStream input = new MemoryStream(data))
            {
                using (MemoryStream output = new MemoryStream())
                {
                    using (Stream cs = new DeflateStream(output, CompressionMode.Compress))
                    {
                        input.CopyTo(cs);
                    }

                    return output.ToArray();
                }
            }
        }

        private static string Decompress(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return null;
            }

            using (MemoryStream input = new MemoryStream(data))
            {
                using (MemoryStream output = new MemoryStream())
                {
                    using (Stream cs = new DeflateStream(input, CompressionMode.Decompress))
                    {
                        cs.CopyTo(output);
                    }

                    byte[] bytes = output.ToArray();

                    string result = Encoding.UTF8.GetString(bytes);

                    return result;
                }
            }
        }
    }
}
