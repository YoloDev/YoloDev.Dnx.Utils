namespace YoloDev.Dnx.Json
{
    internal static class JsonExtensions
    {
        public static JsonArray AsArray(this JsonValue value) => value as JsonArray;
        public static bool? AsBoolean(this JsonValue value) => (value as JsonBoolean)?.Value;
        public static double? AsNumber(this JsonValue value) => (value as JsonNumber)?.Double;
        public static JsonObject AsObject(this JsonValue value) => value as JsonObject;
        public static string AsString(this JsonValue value) => (value as JsonString)?.Value;
        public static bool IsNull(this JsonValue value) => value is JsonNull;
    }
}
