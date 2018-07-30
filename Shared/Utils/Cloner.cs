using Newtonsoft.Json;

namespace SyntetiskTestdataGen.Shared.Utils
{
    public static class Cloner
    {
        public static T Clone<T>(this T x)
        {
            var y = JsonConvert.SerializeObject(x);
            return JsonConvert.DeserializeObject<T>(y);
        }
    }
}
