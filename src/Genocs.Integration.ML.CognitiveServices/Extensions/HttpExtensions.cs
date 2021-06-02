using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace Genocs.Integration.ML.CognitiveServices.Extensions
{
    public static class HttpExtensions
    {
        public static StringContent AsJson(this object o)
            => new(JsonConvert.SerializeObject(o), Encoding.UTF8, "application/json");
    }
}
