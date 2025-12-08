using System.IO;
using System.Linq;
using System.Text.Json;

namespace Services {
	public interface ISaveData : IJSONData;
	public interface ISaveable<T> : IJSONable<T> where T : ISaveData;

	public static class SaveService {
		private const string SaveDirName = "saves";

		private static readonly JsonSerializerOptions FileJsonOptions = new() {
			WriteIndented = true,
			IndentCharacter = '\t',
			IndentSize = 1,
		};

		private static DirectoryInfo GetSaveDir() {
			var path = Path.Combine(Godot.OS.GetUserDataDir(), SaveDirName);

			if(!Directory.Exists(path)) { Directory.CreateDirectory(path); }

			return new DirectoryInfo(path);
		}

		private static FileInfo GetFile(string fileName) {
			var path = Path.Combine(GetSaveDir().FullName, fileName + ".json");
			return new FileInfo(path);
		}

		private static void Write<T>(this FileInfo file, in T data) where T : struct, ISaveData {
			var json = JsonService.Serialize(data, FileJsonOptions);
			File.WriteAllText(file.FullName, json);
		}

		private static T Read<T>(this FileInfo file) where T : struct, ISaveData {
			if(!file.Exists) { throw new FileNotFoundException("Save file not found", file.Name); }

			var json = File.ReadAllText(file.FullName);
			var data = JsonService.Deserialize<T>(json, FileJsonOptions);

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
			var files = GetSaveDir().GetFiles("*.json");

			return files.Select(file => Path.GetFileNameWithoutExtension(file.Name)).ToArray();
		}
	}
}