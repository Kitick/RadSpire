using System;
using System.Text.Json;
using Godot;

namespace Services.Network {
	public enum TransferMode { Reliable, Unreliable }

	public sealed partial class NetworkSync<T> : Node where T : struct, INetworkData {
		private static readonly LogService Log = new(nameof(NetworkSync<>), enabled: true);

		private readonly INetworkable<T> SyncObject;
		private readonly TransferMode Mode;
		private readonly int OwnerPeerId;

		private string LogPrefix => $"{{Peer {OwnerPeerId}}}:";

		private bool HasPendingStateChange = false;

		private static readonly JsonSerializerOptions NetJsonOptions = new();

		public Func<T, T?>? Validator { get; set; }

		private bool IsOwner => Server.Instance.IsOwner(OwnerPeerId);
		private bool IsServer => Multiplayer.IsServer();
		private string SerializeState() => JsonService.Serialize(SyncObject.Serialize(), NetJsonOptions);

		public NetworkSync(INetworkable<T> syncObject, int ownerPeerId, string syncId = "sync", TransferMode mode = TransferMode.Reliable) {
			SyncObject = syncObject;
			OwnerPeerId = ownerPeerId;
			Mode = mode;
			Name = $"NetworkSync_{syncId}_{ownerPeerId}";
		}

		public override void _Ready() {
			if(IsOwner) {
				SyncObject.OnStateChanged += OnLocalStateChanged;

				if(IsServer) {
					CallDeferred(nameof(BroadcastCurrentState));
				}
			}
			else if(!IsServer) {
				CallDeferred(nameof(RequestCurrentState));
			}
		}

		public override void _ExitTree() {
			SyncObject.OnStateChanged -= OnLocalStateChanged;
		}

		public override void _Process(double delta) {
			if(!Server.Instance.IsNetworkConnected) { return; }
			if(HasPendingStateChange && IsOwner) {
				HasPendingStateChange = false;
				SendStateUpdate();
			}
		}

		private void OnLocalStateChanged() {
			if(!IsInsideTree()) { return; }
			HasPendingStateChange = true;
		}

		private void SendStateUpdate() {
			var json = SerializeState();

			Log.Info($"{LogPrefix} Sending state update to host");

			if(IsServer) {
				ProcessAndBroadcast(OwnerPeerId, json);
				return;
			}

			if(Mode == TransferMode.Unreliable) {
				RpcId(1, nameof(SendToHostUnreliable), json);
			}
			else {
				RpcId(1, nameof(SendToHostReliable), json);
			}

		}

		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
		private void SendToHostReliable(string json) => HandleSendToHost(json);

		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
		private void SendToHostUnreliable(string json) => HandleSendToHost(json);

		private void HandleSendToHost(string json) {
			if(!IsServer) return;

			var actualSender = Server.Instance.RemoteSenderId;
			Log.Info($"{LogPrefix} Host received update from peer {actualSender}");

			if(actualSender != OwnerPeerId) {
				Log.Error($"{LogPrefix} Peer {actualSender} tried to update object owned by {OwnerPeerId}");
				return;
			}

			ProcessAndBroadcast(OwnerPeerId, json);
		}

		private void ProcessAndBroadcast(int senderPeerId, string json) {
			var data = JsonService.Deserialize<T>(json, NetJsonOptions);

			if(Validator != null) {
				var validated = Validator(data);
				if(validated == null) {
					Log.Warn($"{LogPrefix} Rejected update from peer {senderPeerId}");
					return;
				}
				data = validated.Value;
				json = JsonService.Serialize(data, NetJsonOptions);
			}

			Log.Info($"{LogPrefix} Host broadcasting to all clients");

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
			if(IsOwner) { return; }

			Log.Info($"{LogPrefix} Received broadcast, applying to SyncObject");
			var data = JsonService.Deserialize<T>(json, NetJsonOptions);
			SyncObject.Deserialize(data);
		}

		private void RequestCurrentState() {
			if(!IsInsideTree()) { return; }

			Log.Info($"{LogPrefix} Requesting current state from host");
			RpcId(1, nameof(SendCurrentStateTo), Multiplayer.GetUniqueId());
		}

		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
		private void SendCurrentStateTo(int requesterId) {
			if(!IsServer) { return; }

			Log.Info($"{LogPrefix} Sending current state to peer {requesterId}");
			RpcId(requesterId, nameof(ReceiveFromHostReliable), SerializeState());
		}

		private void BroadcastCurrentState() {
			if(!IsInsideTree() || !IsServer) { return; }

			Log.Info($"{LogPrefix} Broadcasting initial state to all clients");
			Rpc(nameof(ReceiveFromHostReliable), SerializeState());
		}
	}
}
