using System.IO;
using System.Linq;
using System.Text.Json;

namespace SaveSystem {
	static class SaveService {
		private const string SaveDirName = "saves";
		private const string FileExt = ".save";
		private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

		private static DirectoryInfo GetSaveDir() {
			var path = Path.Combine(Godot.OS.GetUserDataDir(), SaveDirName);
			return Directory.CreateDirectory(path);
		}

		private static string GetFilePath(string fileName) {
			return Path.Combine(GetSaveDir().FullName, fileName + FileExt);
		}

		public static void Save<T>(string fileName, T data) where T : class {
			var path = GetFilePath(fileName);
			var json = JsonSerializer.Serialize(data, JsonOptions);

			File.WriteAllText(path, json);
		}

		public static T? Load<T>(string fileName) where T : class {
			var path = GetFilePath(fileName);

			if(!File.Exists(path)) { return null; }

			var json = File.ReadAllText(path);
			var data = JsonSerializer.Deserialize<T>(json);

			return data;
		}

		public static string[] GetSaves() {
			var files = GetSaveDir().GetFiles("*" + FileExt);

			return files.Select(file => Path.GetFileNameWithoutExtension(file.Name)).ToArray();
		}
	}
}