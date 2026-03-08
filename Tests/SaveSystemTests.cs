namespace Radspire.Tests {
	using System.IO;
	using Components;
	using NUnit.Framework;
	using Services;

	public class SaveSystemTests {
		private string TempDir = null!;

		[SetUp]
		public void Setup() {
			TempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(TempDir);
			SaveService.SaveDirOverride = TempDir;
		}

		[TearDown]
		public void TearDown() {
			SaveService.SaveDirOverride = null;
			if(Directory.Exists(TempDir)) { Directory.Delete(TempDir, recursive: true); }
		}

		[Test]
		public void TestSaveAndLoadHealth() {
			const string fileName = "test_health";
			const int maxHealth = 100;
			const int damageTaken = 40;

			var health = new Health(maxHealth);
			health.Current -= damageTaken;

			var exported = health.Export();
			exported.Save(fileName);

			if(!SaveService.Exists(fileName)) {
				Assert.Fail("Save file was not created.");
				return;
			}

			var loaded = SaveService.Load<HealthData>(fileName);

			if(loaded.Current != exported.Current || loaded.Max != exported.Max) {
				Assert.Fail($"Loaded health ({loaded.Current}/{loaded.Max}) does not match saved health ({exported.Current}/{exported.Max}).");
			}
			else {
				Assert.Pass($"Health saved and loaded correctly: {loaded.Current}/{loaded.Max}.");
			}
		}
	}
}