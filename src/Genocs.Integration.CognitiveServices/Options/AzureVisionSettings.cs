using System.Text.RegularExpressions;

namespace Genocs.Integration.CognitiveServices.Options;

/// <summary>
/// The AzureVisionSettings object.
/// </summary>
public class AzureVisionSettings
{
    /// <summary>
    /// Default Section name.
    /// </summary>
    public static string Position = "AzureVision";

    /// <summary>
    /// The cognitive service root endpoint.
    /// </summary>
    public string Endpoint { get; set; } = default!;

    /// <summary>
    /// The azure cognitive services subscription key.
    /// </summary>
    public string SubscriptionKey { get; set; } = default!;

    /// <summary>
    /// Validator.
    /// </summary>
    /// <param name="settings">instance object to validate.</param>
    /// <param name="throwException"></param>
    /// <returns>true if valid otherwise false.</returns>
    public static bool IsValid(AzureVisionSettings settings, bool throwException = false)
    {
        if (settings is null) return false;

        if (!IsValidEndpoint(settings.Endpoint, throwException)) return false;
        if (!IsValidKey(settings.SubscriptionKey, throwException)) return false;
        return true;
    }

    /// <summary>
    /// Validates the format of the Computer Vision Endpoint URL.
    /// Returns true if the endpoint is valid, false otherwise.
    /// </summary>
    /// <param name="endpoint"></param>
    /// <param name="throwException"></param>
    /// <returns></returns>
    private static bool IsValidEndpoint(string endpoint, bool throwException)
    {
        bool result = true;
        string? errorMessage;
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            errorMessage = "Error: Missing computer vision endpoint.";
            result = BuildResult(errorMessage, throwException);
        }

        if (result && !Regex.IsMatch(endpoint, @"^https://\S+\.cognitiveservices\.azure\.com/?$"))
        {
            errorMessage = $" Error: Invalid value for computer vision endpoint: {endpoint}.";
            errorMessage += " It should be similar to: https://<your-computer-vision-resource-name>.cognitiveservices.azure.com";
            result = BuildResult(errorMessage, throwException);
        }

        if (result && !Uri.IsWellFormedUriString(endpoint, UriKind.Absolute))
        {
            errorMessage = $" Error: Invalid value for computer vision endpoint: {endpoint}.";
            errorMessage += " It should be similar to: https://<your-computer-vision-resource-name>.cognitiveservices.azure.com";
            errorMessage += " The Uri.IsWellFormedUriString return false";
            result = BuildResult(errorMessage, throwException);
        }

        return result;
    }

    // Validates the format of the Computer Vision Key.
    // Returns true if the key is valid, false otherwise.
    private static bool IsValidKey(string key, bool throwException)
    {
        bool result = true;
        string? errorMessage;
        if (string.IsNullOrWhiteSpace(key))
        {
            errorMessage = "Error: Missing computer vision key.";
            result = BuildResult(errorMessage, throwException);
        }

        if (result && !Regex.IsMatch(key, @"^[a-fA-F0-9]{32}$"))
        {
            errorMessage = $" Error: Invalid value for computer vision key: {key}.";
            errorMessage += " It should be a 32-character hexadecimal string.";
            result = BuildResult(errorMessage, throwException);
        }

        return result;
    }

    private static bool BuildResult(string errorMessage, bool throwException)
    {
        if (!string.IsNullOrEmpty(errorMessage))
        {
            if (throwException)
            {
                throw new ArgumentException(errorMessage);
            }
            else
            {
                return false;
            }
        }

        return true;
    }
}