using System.Text.Json;
using Godot;
using Systems.JSON;

namespace Network {
	public sealed partial class NetworkSync<T> : Node where T : struct, INetworkData {
		private readonly INetworkable<T> SyncObject;
		private readonly string SyncPath;

		private static readonly JsonSerializerOptions NetJsonOptions = new JsonSerializerOptions();

		public NetworkSync(INetworkable<T> syncObject, string syncPath) {
			SyncObject = syncObject;
			SyncPath = syncPath;
			SyncObject.OnStateChanged += OnStateChanged;

			AddToGroup(syncPath);
		}

		private void OnStateChanged() {
			var data = SyncObject.Serialize();
			var json = JSON.Serialize(data, NetJsonOptions);

			BroadcastStateUpdate(json);
		}

		private void BroadcastStateUpdate(string json) {
			if(!IsNodeReady()) return;

			GetTree().CallGroup(SyncPath, nameof(ReceiveStateUpdate), json);
		}

		[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
		public void ReceiveStateUpdate(string json) {
			var data = JSON.Deserialize<T>(json, NetJsonOptions);
			SyncObject.Deserialize(data);
		}
	}
}
