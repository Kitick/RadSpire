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
		public static string? SaveDirOverride { get; set; } = null;

		private static readonly JsonSerializerOptions FileJsonOptions = new() {
			IncludeFields = true,
			WriteIndented = true,
			IndentCharacter = '\t',
			IndentSize = 1,
		};

		private static DirectoryInfo GetSaveDir() {
			string root = SaveDirOverride ?? Godot.OS.GetUserDataDir();
			string path = Path.Combine(root, SaveDirName);

			if(!Directory.Exists(path)) { Directory.CreateDirectory(path); }

			return new DirectoryInfo(path);
		}

		private static FileInfo GetFile(string fileName) {
			string path = Path.Combine(GetSaveDir().FullName, fileName + ".json");
			return new FileInfo(path);
		}

		private static void Write<TData>(this FileInfo file, TData data) {
			string json = JsonService.Serialize(data, FileJsonOptions);
			File.WriteAllText(file.FullName, json);
		}

		private static TData Read<TData>(this FileInfo file) {
			if(!file.Exists) { throw new FileNotFoundException("Save file not found", file.Name); }

			string json = File.ReadAllText(file.FullName);
			TData data = JsonService.Deserialize<TData>(json, FileJsonOptions);

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
			TData data = Load<TData>(fileName);
			saveable.Import(data);
		}

		public static bool Exists(string fileName) {
			return GetFile(fileName).Exists;
		}

		public static void Delete(string fileName) {
			FileInfo file = GetFile(fileName);
			if(file.Exists) { file.Delete(); }
		}

		public static string[] List() {
			FileInfo[] files = GetSaveDir().GetFiles("*.json");

			return files.Select(file => Path.GetFileNameWithoutExtension(file.Name)).ToArray();
		}
	}
}