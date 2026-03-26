namespace Services;

using System.Text.Json;
using System.Text.Json.Serialization;

public static class JsonService {
	private static readonly JsonConverter[] Converters = [
		new Vector3Converter()
	];

	private static JsonSerializerOptions InjectConverters(this JsonSerializerOptions options) {
		var newOptions = new JsonSerializerOptions(options);
		foreach(var converter in Converters) {
			newOptions.Converters.Add(converter);
		}
		return newOptions;
	}

	public static string Serialize<T>(in T data, JsonSerializerOptions options) {
		return JsonSerializer.Serialize(data, options.InjectConverters());
	}

	public static T Deserialize<T>(string json, JsonSerializerOptions options) {
		T? data = JsonSerializer.Deserialize<T>(json, options.InjectConverters());
		return data ?? throw new JsonException("Deserialized data is null");
	}
}
