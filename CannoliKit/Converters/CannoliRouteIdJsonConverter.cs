using CannoliKit.Modules.Routing;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CannoliKit.Converters
{
    internal class CannoliRouteIdJsonConverter : JsonConverter<CannoliRouteId>
    {
        public override CannoliRouteId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var routeId = reader.GetString()!;

            return new CannoliRouteId(routeId);
        }

        public override void Write(Utf8JsonWriter writer, CannoliRouteId value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }
}
