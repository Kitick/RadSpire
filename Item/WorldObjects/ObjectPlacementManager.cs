namespace Objects {
    using Character;
    using Godot;
    using Services;
    using ItemSystem;
	using Root;

	public partial class ObjectPlacementManager : Node {
        private static readonly LogService Log = new(nameof(ObjectPlacementManager), enabled: true);
        private const float DefaultPlaceDistance = 2.0f;

        public WorldObjectManager? WorldObjectManager { get; private set; }
        public InventoryManager? InventoryManager { get; private set; }
        public GameManager? GameManager { get; private set; }
        public bool Initalized => WorldObjectManager != null && InventoryManager != null && GameManager != null;

        public void Initialize(WorldObjectManager worldObjectManager, InventoryManager inventoryManager, GameManager gameManager) {
            WorldObjectManager = worldObjectManager;
            InventoryManager = inventoryManager;
            GameManager = gameManager;
        }

        public bool PlaceObjectInFrontOfPlayer(Player player, string itemId, float distance = DefaultPlaceDistance) {
            if(!Initalized) {
                Log.Error("ObjectPlacementManager is not initialized.");
                return false;
            }
            if(player == null || !GodotObject.IsInstanceValid(player)) {
                Log.Error("PlaceObjectInFrontOfPlayer failed: player is invalid.");
                return false;
            }
            if(string.IsNullOrWhiteSpace(itemId)) {
                Log.Error("PlaceObjectInFrontOfPlayer failed: itemId is empty.");
                return false;
            }

            Vector3 position = GetPositionInFrontOfPlayer(player, distance);
            Vector3 rotation = player.GlobalRotation;

            bool created = WorldObjectManager!.CreateWorldObject(itemId, position, rotation);
            if(!created) {
                Log.Error($"PlaceObjectInFrontOfPlayer failed to create world object for ItemId '{itemId}'.");
            }
            return created;
        }

        public Vector3 GetPositionInFrontOfPlayer(Player player, float distance = DefaultPlaceDistance) {
            if(player == null || !GodotObject.IsInstanceValid(player)) {
                Log.Error("Player is invalid.");
                return player!.GlobalPosition;
            }
            float placeDistance = Mathf.Max(0.5f, distance);
            Vector3 forward = -player.GlobalBasis.Z;
            forward.Y = 0;
            if(forward.LengthSquared() < 0.0001f) {
                forward = -Vector3.Forward;
            }
            forward = forward.Normalized();
            Vector3 position = player.GlobalPosition + (forward * placeDistance);
            Vector3 groundPosition = GetPositionOnGround(position);
            return groundPosition;
        }

        public Vector3 GetPositionOnGround(Vector3 position){

        }
    }
}