using System;
using System.Text.Json;
using Godot;
using Systems.JSON;

namespace Network {
	/// <summary>
	/// Handles network synchronization for any INetworkable object.
	/// Flow: Client sends state to Host -> Host validates -> Host broadcasts to all clients.
	/// The INetworkable object only cares about Serialize/Deserialize, not the network model.
	/// </summary>
	public sealed partial class NetworkSync<T> : Node where T : struct, INetworkData {
		private readonly INetworkable<T> SyncObject;
		private readonly int OwnerPeerId;

		private static readonly JsonSerializerOptions NetJsonOptions = new JsonSerializerOptions();

		/// <summary>
		/// Optional validation function. Host uses this to validate incoming data before broadcasting.
		/// Returns the validated (potentially modified) data, or null to reject.
		/// </summary>
		public Func<T, T?>? Validator { get; set; }

		/// <summary>
		/// Creates a NetworkSync for the given object.
		/// </summary>
		/// <param name="syncObject">The object implementing INetworkable to sync</param>
		/// <param name="ownerPeerId">The peer ID that owns/controls this object</param>
		public NetworkSync(INetworkable<T> syncObject, int ownerPeerId) {
			SyncObject = syncObject;
			OwnerPeerId = ownerPeerId;
		}

		public override void _Ready() {
			// Only subscribe to state changes if we own this object
			if(Multiplayer.GetUniqueId() == OwnerPeerId) {
				SyncObject.OnStateChanged += OnLocalStateChanged;
			}
		}

		public override void _ExitTree() {
			SyncObject.OnStateChanged -= OnLocalStateChanged;
		}

		/// <summary>
		/// Called when OUR local state changes. Send it to the host for validation.
		/// </summary>
		private void OnLocalStateChanged() {
			if(!IsInsideTree()) return;

			var data = SyncObject.Serialize();
			var json = JSON.Serialize(data, NetJsonOptions);

			if(Multiplayer.IsServer()) {
				// We ARE the host, validate and broadcast directly
				ProcessAndBroadcast(OwnerPeerId, json);
			} else {
				// Send to host for validation
				RpcId(1, nameof(SendToHost), OwnerPeerId, json);
			}
		}

		/// <summary>
		/// Called on HOST when a client sends their state update.
		/// </summary>
		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
		private void SendToHost(int senderPeerId, string json) {
			if(!Multiplayer.IsServer()) return;

			// Verify the sender is who they claim to be
			var actualSender = Multiplayer.GetRemoteSenderId();
			if(actualSender != senderPeerId) {
				GD.PrintErr($"NetworkSync: Peer {actualSender} tried to spoof as {senderPeerId}");
				return;
			}

			ProcessAndBroadcast(senderPeerId, json);
		}

		/// <summary>
		/// Host-side: Validate the data and broadcast to all clients.
		/// </summary>
		private void ProcessAndBroadcast(int senderPeerId, string json) {
			var data = JSON.Deserialize<T>(json, NetJsonOptions);

			// Run validation if provided
			if(Validator != null) {
				var validated = Validator(data);
				if(validated == null) {
					GD.Print($"NetworkSync: Rejected update from peer {senderPeerId}");
					return;
				}
				data = validated.Value;
				json = JSON.Serialize(data, NetJsonOptions);
			}

			// Broadcast to all clients (including sender and host)
			Rpc(nameof(ReceiveFromHost), senderPeerId, json);
		}

		/// <summary>
		/// Called on ALL clients (including host) when host broadcasts validated state.
		/// </summary>
		[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
		private void ReceiveFromHost(int senderPeerId, string json) {
			// Only apply if this sync belongs to the sender
			if(OwnerPeerId != senderPeerId) return;

			var data = JSON.Deserialize<T>(json, NetJsonOptions);
			SyncObject.Deserialize(data);
		}
	}
}
