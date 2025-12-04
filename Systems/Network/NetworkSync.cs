using System;
using System.Text.Json;
using Godot;
using Systems.JSON;

namespace Network {
	public enum TransferMode { Reliable, Unreliable }

	public sealed partial class NetworkSync<T> : Node where T : struct, INetworkData {
		private readonly INetworkable<T> SyncObject;
		private readonly int OwnerPeerId;
		private readonly TransferMode Mode;
		private bool HasPendingStateChange = false;

		private static readonly JsonSerializerOptions NetJsonOptions = new();

		public Func<T, T?>? Validator { get; set; }

		public NetworkSync(INetworkable<T> syncObject, int ownerPeerId, string syncId = "sync", TransferMode mode = TransferMode.Reliable) {
			SyncObject = syncObject;
			OwnerPeerId = ownerPeerId;
			Mode = mode;
			Name = $"NetworkSync_{syncId}_{ownerPeerId}";
		}

		public override void _Ready() {
			if(Multiplayer.GetUniqueId() == OwnerPeerId) {
				SyncObject.OnStateChanged += OnLocalStateChanged;

				if(Multiplayer.IsServer()) {
					CallDeferred(nameof(BroadcastCurrentState));
				}
			}
			else if(!Multiplayer.IsServer()) {
				CallDeferred(nameof(RequestCurrentState));
			}
		}

		public override void _ExitTree() {
			SyncObject.OnStateChanged -= OnLocalStateChanged;
		}

		public override void _Process(double delta) {
			if(HasPendingStateChange && Multiplayer.GetUniqueId() == OwnerPeerId) {
				HasPendingStateChange = false;
				SendStateUpdate();
			}
		}

		private void OnLocalStateChanged() {
			if(!IsInsideTree()) return;
			HasPendingStateChange = true;
		}

		private void SendStateUpdate() {
			var data = SyncObject.Serialize();
			var json = JSON.Serialize(data, NetJsonOptions);

			GD.Print($"NetworkSync [{Name}]: Sending state update to host");

			if(Multiplayer.IsServer()) {
				// We ARE the host, validate and broadcast directly
				ProcessAndBroadcast(OwnerPeerId, json);
			}
			else {
				// Send to host for validation using appropriate transfer mode
				if(Mode == TransferMode.Unreliable) {
					RpcId(1, nameof(SendToHostUnreliable), json);
				}
				else {
					RpcId(1, nameof(SendToHostReliable), json);
				}
			}
		}

		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
		private void SendToHostReliable(string json) => HandleSendToHost(json);

		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
		private void SendToHostUnreliable(string json) => HandleSendToHost(json);

		private void HandleSendToHost(string json) {
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

		private void ProcessAndBroadcast(int senderPeerId, string json) {
			var data = JSON.Deserialize<T>(json, NetJsonOptions);

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

			if(Mode == TransferMode.Unreliable) {
				Rpc(nameof(ReceiveFromHostUnreliable), json);
			}
			else {
				Rpc(nameof(ReceiveFromHostReliable), json);
			}
		}

		[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
		private void ReceiveFromHostReliable(string json) => HandleReceiveFromHost(json);

		[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
		private void ReceiveFromHostUnreliable(string json) => HandleReceiveFromHost(json);

		private void HandleReceiveFromHost(string json) {
			GD.Print($"NetworkSync [{Name}]: Received broadcast, applying to SyncObject");
			var data = JSON.Deserialize<T>(json, NetJsonOptions);
			SyncObject.Deserialize(data);
		}

		private void RequestCurrentState() {
			if(!IsInsideTree()) return;
			GD.Print($"NetworkSync [{Name}]: Requesting current state from host");
			RpcId(1, nameof(SendCurrentStateTo), Multiplayer.GetUniqueId());
		}

		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
		private void SendCurrentStateTo(int requesterId) {
			if(!Multiplayer.IsServer()) return;

			var data = SyncObject.Serialize();
			var json = JSON.Serialize(data, NetJsonOptions);
			GD.Print($"NetworkSync [{Name}]: Sending current state to peer {requesterId}");
			RpcId(requesterId, nameof(ReceiveFromHostReliable), json);
		}

		private void BroadcastCurrentState() {
			if(!IsInsideTree() || !Multiplayer.IsServer()) return;

			var data = SyncObject.Serialize();
			var json = JSON.Serialize(data, NetJsonOptions);
			GD.Print($"NetworkSync [{Name}]: Broadcasting initial state to all clients");
			Rpc(nameof(ReceiveFromHostReliable), json);
		}
	}
}
