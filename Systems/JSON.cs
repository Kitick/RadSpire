using System.Text.Json;
using System.Text.Json.Serialization;

namespace Systems.JSON {
	public interface IJSONData;

	public interface IJSONable<T> where T : IJSONData {
		T Serialize();
		void Deserialize(in T data);
	}

	public static class JSON {
		private static readonly JsonConverter[] Converters = [
			new Vector3Converter()
		];

		private static JsonSerializerOptions InjectConverters(JsonSerializerOptions options) {
			var newOptions = new JsonSerializerOptions(options);
			foreach(var converter in Converters) {
				newOptions.Converters.Add(converter);
			}
			return newOptions;
		}

		public static string Serialize<T>(in T data, JsonSerializerOptions options) where T : struct, IJSONData {
			return JsonSerializer.Serialize(data, InjectConverters(options));
		}

		public static T Deserialize<T>(string json, JsonSerializerOptions options) where T : struct, IJSONData {
			return JsonSerializer.Deserialize<T>(json, InjectConverters(options));
		}
	}
}