// Enum-value converters in referenced assemblies are file-local; explicit metadata below handles them.
using Soenneker.Enums.Email.Format;
using Soenneker.Messages.Email;
using System.Collections.Generic;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json.Serialization;
using System.Text.Json;
using System;

namespace Soenneker.Email.Senders.Resend;

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, ReadCommentHandling = JsonCommentHandling.Skip, UseStringEnumConverter = true)]
[JsonSerializable(typeof(EmailMessage))]
internal partial class LibraryJsonContext : JsonSerializerContext
{
    internal static JsonTypeInfo<T> Get<T>() =>
        (JsonTypeInfo<T>)MetadataOptionsHolder.Value.GetTypeInfo(typeof(T));

    private static class MetadataOptionsHolder
    {
        internal static readonly JsonSerializerOptions Value = CreateMetadataOptions();
    }

    private static JsonSerializerOptions CreateMetadataOptions()
    {
        var options = new JsonSerializerOptions(Default.Options) { TypeInfoResolver = new MetadataResolver() };
        options.Converters.Add(new EmailFormatMetadataConverter());
        options.Converters.Add(new EmailPriorityMetadataConverter());
        options.MakeReadOnly();
        return options;
    }

    private sealed class MetadataResolver : IJsonTypeInfoResolver
    {
        public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            if (type == typeof(Soenneker.Enums.Email.Format.EmailFormat))
                return JsonMetadataServices.CreateValueInfo<Soenneker.Enums.Email.Format.EmailFormat>(options, new EmailFormatMetadataConverter());
            if (type == typeof(Soenneker.Enums.Email.Priority.EmailPriority))
                return JsonMetadataServices.CreateValueInfo<Soenneker.Enums.Email.Priority.EmailPriority>(options, new EmailPriorityMetadataConverter());
            return ((IJsonTypeInfoResolver)Default).GetTypeInfo(type, options);
        }
    }

}

internal sealed class EmailFormatMetadataConverter : JsonConverter<Soenneker.Enums.Email.Format.EmailFormat>
{
    public override Soenneker.Enums.Email.Format.EmailFormat Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType == JsonTokenType.String && Soenneker.Enums.Email.Format.EmailFormat.TryFromValue(reader.GetString(), out var value) ? value : throw new JsonException("Unknown EmailFormat value.");

    public override void Write(Utf8JsonWriter writer, Soenneker.Enums.Email.Format.EmailFormat value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}

internal sealed class EmailPriorityMetadataConverter : JsonConverter<Soenneker.Enums.Email.Priority.EmailPriority>
{
    public override Soenneker.Enums.Email.Priority.EmailPriority Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType == JsonTokenType.String && Soenneker.Enums.Email.Priority.EmailPriority.TryFromValue(reader.GetString(), out var value) ? value : throw new JsonException("Unknown EmailPriority value.");

    public override void Write(Utf8JsonWriter writer, Soenneker.Enums.Email.Priority.EmailPriority value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
