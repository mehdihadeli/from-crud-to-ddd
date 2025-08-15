using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartCharging.ServiceDefaults.SystemTextSerializer;

public static class SystemTextJsonSerializerOptions
{
    public static JsonSerializerOptions DefaultSerializerOptions { get; } = CreateDefaultSerializerOptions();

    public static JsonSerializerOptions CreateDefaultSerializerOptions(bool camelCase = true, bool indented = false)
    {
        var options = new JsonSerializerOptions
        {
            IncludeFields = true,
            PropertyNameCaseInsensitive = true,
            // Equivalent to ReferenceLoopHandling.Ignore
            ReferenceHandler = ReferenceHandler.IgnoreCycles,

            WriteIndented = indented,
            PropertyNamingPolicy = camelCase ? JsonNamingPolicy.CamelCase : null,
        };

        options.Converters.Add(new JsonStringEnumConverter());
        // For DateOnly support (similar to your DateOnlyConverter)
        options.Converters.Add(new JsonDateOnlyConverter());

        return options;
    }

    public static JsonSerializerOptions SetDefaultOptions(JsonSerializerOptions jsonSerializerOptions)
    {
        // Copy the settings from DefaultSerializerOptions
        var defaultOptions = DefaultSerializerOptions;

        jsonSerializerOptions.IncludeFields = defaultOptions.IncludeFields;
        jsonSerializerOptions.PropertyNameCaseInsensitive = defaultOptions.PropertyNameCaseInsensitive;
        jsonSerializerOptions.WriteIndented = defaultOptions.WriteIndented;
        jsonSerializerOptions.ReferenceHandler = defaultOptions.ReferenceHandler;
        jsonSerializerOptions.PropertyNamingPolicy = defaultOptions.PropertyNamingPolicy;

        // Add converters from DefaultSerializerOptions
        foreach (var converter in defaultOptions.Converters)
        {
            jsonSerializerOptions.Converters.Add(converter);
        }

        return jsonSerializerOptions;
    }
}
