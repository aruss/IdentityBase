namespace IdentityBase.Extensions
{
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using Newtonsoft.Json;

    public static class TempDataExtensions
    {
        public static bool TryRetrieveData<T>(this ITempDataDictionary tempData, out T tKey)
        {
            tKey = default(T);

            string entryKey = nameof(T);

            if (tempData.ContainsKey(entryKey))
            {
                tKey = JsonConvert.DeserializeObject<T>(tempData[entryKey] as string);

                tempData.Remove(entryKey);

                return true;
            }

            return false;
        }

        public static void StoreData<T>(this ITempDataDictionary tempData, T tKey)
        {
            string entryKey = nameof(T);

            tempData[entryKey] = JsonConvert.SerializeObject(tKey);
        }
    }
}
