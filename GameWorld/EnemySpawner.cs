namespace Root {
	using Character;
	using Core;
	using Godot;
	using Services;

	public sealed class EnemySpawner {
		private static readonly LogService Log = new(nameof(EnemySpawner), enabled: true);

		public int MaxEnemies = 5;
		public float MinSpawnInterval = 1f;
		public float MaxSpawnInterval = 6f;
		public float MinSpawnRadius = 20f;
		public float MaxSpawnRadius = 35f;
		public float SpawnHeightOffset = 0.25f;

		public int Count { get; private set; }

		private readonly Node Parent;
		private readonly PackedScene EnemyScene;
		private Node3D? Target;
		private float SpawnTimer = 5f;

		public EnemySpawner(Node parent, PackedScene enemyScene) {
			Parent = parent;
			EnemyScene = enemyScene;
		}

		public void SetTarget(Node3D target) {
			Target = target;
		}

		public void Update() {
			SpawnTimer -= 0.015f;

			if(SpawnTimer > 0f || Count >= MaxEnemies) { return; }

			SpawnTimer = (float) GD.RandRange(MinSpawnInterval, MaxSpawnInterval);
			SpawnEnemy();
		}

		public void Reset() {
			Count = 0;
			SpawnTimer = 5f;
			Target = null;
		}

		private void SpawnEnemy() {
			if(Target == null || !GodotObject.IsInstanceValid(Target)) {
				Log.Warn("Cannot spawn enemy: no valid target set.");
				return;
			}

			var enemy = Parent.AddScene<Enemy>(EnemyScene);
			enemy.GlobalPosition = GetRandomSpawnNearTarget();
			enemy.SetTarget(Target);
			Count += 1;

			Log.Info($"Enemy spawned. Total: {Count}/{MaxEnemies}");
		}

		private Vector3 GetRandomSpawnNearTarget() {
			var pos = Target!.GlobalPosition;
			float angle = (float) GD.RandRange(0f, Mathf.Tau);
			float radius = (float) GD.RandRange(MinSpawnRadius, MaxSpawnRadius);
			return pos + new Vector3(
				Mathf.Cos(angle) * radius,
				SpawnHeightOffset,
				Mathf.Sin(angle) * radius
			);
		}
	}
}
