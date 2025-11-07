using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SaveSystem {
	static class SaveService {
		private const string SaveDirName = "saves";
		private const string FileExt = ".json";

		private static readonly JsonSerializerOptions JsonOptions = DataConverters.CreateOptions();

		private static DirectoryInfo GetSaveDir() {
			var path = Path.Combine(Godot.OS.GetUserDataDir(), SaveDirName);

			if(!Directory.Exists(path)) { Directory.CreateDirectory(path); }

			return new DirectoryInfo(path);
		}

		private static FileInfo GetFile(string fileName) {
			var path = Path.Combine(GetSaveDir().FullName, fileName + FileExt);
			return new FileInfo(path);
		}

		private static void Write<T>(this FileInfo file, in T data) where T : struct, ISaveData {
			var json = JsonSerializer.Serialize(data, JsonOptions);
			File.WriteAllText(file.FullName, json);
		}

		private static T Read<T>(this FileInfo file) where T : struct, ISaveData {
			if(!file.Exists) { throw new FileNotFoundException("Save file not found", file.Name); }

			var json = File.ReadAllText(file.FullName);
			var data = JsonSerializer.Deserialize<T>(json, JsonOptions);

			return data;
		}

		public static void Save<T>(string fileName, in T data) where T : struct, ISaveData {
			GetFile(fileName).Write(data);
		}

		public static T Load<T>(string fileName) where T : struct, ISaveData {
			return GetFile(fileName).Read<T>();
		}

		public static bool Exists(string fileName) {
			return GetFile(fileName).Exists;
		}

		public static void Delete(string fileName) {
			var file = GetFile(fileName);
			if(file.Exists) { file.Delete(); }
		}

		public static string[] ListSaves() {
			var files = GetSaveDir().GetFiles("*" + FileExt);

			return files.Select(file => Path.GetFileNameWithoutExtension(file.Name)).ToArray();
		}
	}
}