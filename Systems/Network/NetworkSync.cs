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
		private bool _hasPendingStateChange = false;

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
		/// <param name="syncId">Unique identifier for this sync object (e.g., "player")</param>
		public NetworkSync(INetworkable<T> syncObject, int ownerPeerId, string syncId = "sync") {
			SyncObject = syncObject;
			OwnerPeerId = ownerPeerId;
			// Use a unique name so RPCs can find the correct node across instances
			Name = $"NetworkSync_{syncId}_{ownerPeerId}";
		}

		public override void _Ready() {
			// Only subscribe to state changes if we own this object
			if(Multiplayer.GetUniqueId() == OwnerPeerId) {
				SyncObject.OnStateChanged += OnLocalStateChanged;

				// If we're the owner and on the server, broadcast initial state to all
				// This ensures new clients get the current state
				if(Multiplayer.IsServer()) {
					CallDeferred(nameof(BroadcastCurrentState));
				}
			}
			else if(!Multiplayer.IsServer()) {
				// We're a client and this is a remote player - request current state from host
				CallDeferred(nameof(RequestCurrentState));
			}
		}

		public override void _ExitTree() {
			SyncObject.OnStateChanged -= OnLocalStateChanged;
		}

		public override void _Process(double delta) {
			// Send pending state change once per frame (if we own this object)
			if(_hasPendingStateChange && Multiplayer.GetUniqueId() == OwnerPeerId) {
				_hasPendingStateChange = false;
				SendStateUpdate();
			}
		}

		/// <summary>
		/// Called when OUR local state changes. Marks for update next frame.
		/// </summary>
		private void OnLocalStateChanged() {
			if(!IsInsideTree()) return;
			_hasPendingStateChange = true;
		}

		/// <summary>
		/// Actually sends the state update to the host.
		/// </summary>
		private void SendStateUpdate() {
			var data = SyncObject.Serialize();
			var json = JSON.Serialize(data, NetJsonOptions);

			GD.Print($"NetworkSync [{Name}]: Sending state update to host");

			if(Multiplayer.IsServer()) {
				// We ARE the host, validate and broadcast directly
				ProcessAndBroadcast(OwnerPeerId, json);
			}
			else {
				// Send to host for validation
				RpcId(1, nameof(SendToHost), json);
			}
		}

		/// <summary>
		/// Called on HOST when a client sends their state update.
		/// </summary>
		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
		private void SendToHost(string json) {
			if(!Multiplayer.IsServer()) return;

			var actualSender = Multiplayer.GetRemoteSenderId();
			GD.Print($"NetworkSync [{Name}]: Host received update from peer {actualSender}");

			// Verify the sender owns this sync object
			if(actualSender != OwnerPeerId) {
				GD.PrintErr($"NetworkSync [{Name}]: Peer {actualSender} tried to update object owned by {OwnerPeerId}");
				return;
			}

			ProcessAndBroadcast(OwnerPeerId, json);
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
					GD.Print($"NetworkSync [{Name}]: Rejected update from peer {senderPeerId}");
					return;
				}
				data = validated.Value;
				json = JSON.Serialize(data, NetJsonOptions);
			}

			GD.Print($"NetworkSync [{Name}]: Host broadcasting to all clients");
			// Broadcast to all clients (including sender and host)
			Rpc(nameof(ReceiveFromHost), json);
		}

		/// <summary>
		/// Called on ALL clients (including host) when host broadcasts validated state.
		/// </summary>
		[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
		private void ReceiveFromHost(string json) {
			GD.Print($"NetworkSync [{Name}]: Received broadcast, applying to SyncObject");
			var data = JSON.Deserialize<T>(json, NetJsonOptions);
			SyncObject.Deserialize(data);
		}

		/// <summary>
		/// Request current state from host (called by clients for remote players).
		/// </summary>
		private void RequestCurrentState() {
			if(!IsInsideTree()) return;
			GD.Print($"NetworkSync [{Name}]: Requesting current state from host");
			RpcId(1, nameof(SendCurrentStateTo), Multiplayer.GetUniqueId());
		}

		/// <summary>
		/// Host sends current state to a specific peer.
		/// </summary>
		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
		private void SendCurrentStateTo(int requesterId) {
			if(!Multiplayer.IsServer()) return;

			var data = SyncObject.Serialize();
			var json = JSON.Serialize(data, NetJsonOptions);
			GD.Print($"NetworkSync [{Name}]: Sending current state to peer {requesterId}");
			RpcId(requesterId, nameof(ReceiveFromHost), json);
		}

		/// <summary>
		/// Broadcast current state to all clients (called by host when owner's sync is ready).
		/// </summary>
		private void BroadcastCurrentState() {
			if(!IsInsideTree() || !Multiplayer.IsServer()) return;

			var data = SyncObject.Serialize();
			var json = JSON.Serialize(data, NetJsonOptions);
			GD.Print($"NetworkSync [{Name}]: Broadcasting initial state to all clients");
			Rpc(nameof(ReceiveFromHost), json);
		}
	}
}
