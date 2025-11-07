using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace SaveSystem {
	public static class DataConverters {
		public static JsonSerializerOptions CreateOptions() => new() {
			WriteIndented = true,
			Converters = { new Vector3Converter() }
		};
	}

	class Vector3Converter : JsonConverter<Vector3> {
		public override Vector3 Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options) {
			var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			float x = root.GetProperty("X").GetSingle();
			float y = root.GetProperty("Y").GetSingle();
			float z = root.GetProperty("Z").GetSingle();

			return new Vector3(x, y, z);
		}

		public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options) {
			writer.WriteStartObject();
			writer.WriteNumber("X", value.X);
			writer.WriteNumber("Y", value.Y);
			writer.WriteNumber("Z", value.Z);
			writer.WriteEndObject();
		}
	}
}