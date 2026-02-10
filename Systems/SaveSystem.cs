using System.IO;
using System.Linq;
using System.Text.Json;

namespace Services {
	public interface ISaveData;

	public interface ISaveable<T> where T : ISaveData {
		T Export();
		void Import(T data);
	}

	public static class SaveService {
		private const string SaveDirName = "saves";

		private static readonly JsonSerializerOptions FileJsonOptions = new() {
			IncludeFields = true,
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

		private static void Write<TData>(this FileInfo file, TData data) {
			var json = JsonService.Serialize(data, FileJsonOptions);
			File.WriteAllText(file.FullName, json);
		}

		private static TData Read<TData>(this FileInfo file) {
			if(!file.Exists) { throw new FileNotFoundException("Save file not found", file.Name); }

			var json = File.ReadAllText(file.FullName);
			var data = JsonService.Deserialize<TData>(json, FileJsonOptions);

			return data;
		}

		public static void Save<TData>(this TData data, string fileName) where TData : ISaveData {
			GetFile(fileName).Write(data);
		}

		public static TData Load<TData>(string fileName) where TData : ISaveData {
			return GetFile(fileName).Read<TData>();
		}

		public static void Save<TData>(this ISaveable<TData> saveable, string fileName) where TData : ISaveData {
			saveable.Export().Save(fileName);
		}

		public static void Load<TData>(this ISaveable<TData> saveable, string fileName) where TData : ISaveData {
			var data = Load<TData>(fileName);
			saveable.Import(data);
		}

		public static bool Exists(string fileName) {
			return GetFile(fileName).Exists;
		}

		public static void Delete(string fileName) {
			var file = GetFile(fileName);
			if(file.Exists) { file.Delete(); }
		}

		public static string[] List() {
			var files = GetSaveDir().GetFiles("*.json");

			return files.Select(file => Path.GetFileNameWithoutExtension(file.Name)).ToArray();
		}
	}
}