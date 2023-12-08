using System.Text;
using System.Text.Json;

namespace Genocs.Integration.CognitiveServices.Extensions;

/// <summary>
/// Http Extensions method helper class.
/// </summary>
internal static class HttpExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    public static StringContent AsJson(this object o)
        => new(JsonSerializer.Serialize(o), Encoding.UTF8, "application/json");
}
