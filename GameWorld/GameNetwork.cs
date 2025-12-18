using System.Collections.Generic;
using System.Linq;
using Character;
using Components;
using Core;
using Godot;
using Services.Network;
using Services;

namespace Root {
	/*
	public partial class GameManager {
		private readonly Dictionary<int, (Player Player, NetworkSync<MovementData> MovementSync, NetworkSync<HealthData> HealthSync)> RemotePlayers = [];

		public int LocalPeerId => Server.PeerId;

		private Server Server = null!;
		private (NetworkSync<MovementData> Movement, NetworkSync<HealthData> Health)? LocalPlayerSync;

		private void InitializeNetwork() {
			Server = Server.Instance;
			SubscribeToNetworkEvents();
		}

		private void CleanupNetwork() {
			UnsubscribeFromNetworkEvents();
			RemoveAllPlayers();
			CleanupLocalPlayerSync();
		}

		private void CleanupLocalPlayerSync() {
			if(LocalPlayerSync is var (movement, health)) {
				movement.QueueFree();
				health.QueueFree();
			}
			LocalPlayerSync = null;
		}

		private void SubscribeToNetworkEvents() {
			Server.OnHostStarted += OnHostStarted;
			Server.OnJoinedServer += OnJoinedServer;
			Server.OnServerDisconnected += OnServerDisconnected;
			Server.OnPeerConnected += OnPeerConnected;
			Server.OnPeerDisconnected += OnPeerDisconnected;
		}

		private void UnsubscribeFromNetworkEvents() {
			Server.OnHostStarted -= OnHostStarted;
			Server.OnJoinedServer -= OnJoinedServer;
			Server.OnServerDisconnected -= OnServerDisconnected;
			Server.OnPeerConnected -= OnPeerConnected;
			Server.OnPeerDisconnected -= OnPeerDisconnected;
		}

		private void CreateLocalPlayerSync() {
			if(!Server.IsNetworkConnected) return;

			var movementSync = new NetworkSync<MovementData>(
				LocalPlayer!.Movement,
				LocalPeerId,
				$"player_{LocalPeerId}_movement",
				TransferMode.Unreliable
			);
			AddChild(movementSync);

			var healthSync = new NetworkSync<HealthData>(
				LocalPlayer.Health,
				LocalPeerId,
				$"player_{LocalPeerId}_health",
				TransferMode.Reliable
			);
			AddChild(healthSync);

			LocalPlayerSync = (movementSync, healthSync);
		}

		private void OnHostStarted() {
			Log.Info($"Host started, local peer ID: {LocalPeerId}");
			CreateLocalPlayerSync();
		}

		private void OnJoinedServer() {
			Log.Info($"Joined server, local peer ID: {LocalPeerId}");
			CreateLocalPlayerSync();
			RpcId(1, nameof(NotifyHostOfNewPlayer), LocalPeerId);
		}

		private void OnServerDisconnected() {
			Log.Info("Server disconnected, cleaning up remote players");
			RemoveAllPlayers();
			CleanupLocalPlayerSync();
		}

		private void OnPeerConnected(int peerId) {
			Log.Info($"Peer connected: {peerId}");
		}

		private void OnPeerDisconnected(int peerId) {
			Log.Info($"Peer disconnected: {peerId}");
			RemovePlayer(peerId);
		}

		private void RemovePlayer(int peerId) {
			if(RemotePlayers.TryGetValue(peerId, out var remote)) {
				remote.MovementSync.QueueFree();
				remote.HealthSync.QueueFree();
				remote.Player.QueueFree();
				RemotePlayers.Remove(peerId);
			}
		}

		private void RemoveAllPlayers() {
			foreach(var peerId in RemotePlayers.Keys.ToList()) {
				RemovePlayer(peerId);
			}
		}

		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
		private void NotifyHostOfNewPlayer(int newPeerId) {
			if(!Multiplayer.IsServer()) return;

			Log.Info($"Host notified of new player: {newPeerId}");

			Rpc(nameof(SpawnRemotePlayer), newPeerId);

			foreach(var existingPeerId in RemotePlayers.Keys) {
				RpcId(newPeerId, nameof(SpawnRemotePlayer), existingPeerId);
			}
			RpcId(newPeerId, nameof(SpawnRemotePlayer), LocalPeerId);
		}

		[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
		private void SpawnRemotePlayer(int peerId) {
			bool isLocalPlayer = peerId == LocalPeerId;
			bool alreadySpawned = RemotePlayers.ContainsKey(peerId);
			if(isLocalPlayer || alreadySpawned) return;

			Log.Info($"Spawning remote player: {peerId}");
			CreateNetworkedPlayer(peerId);
		}

		private void CreateNetworkedPlayer(int peerId) {
			var player = this.AddScene<Player>(PlayerScene);

			player.Name = $"Player_{peerId}";
			player.GlobalPosition = PlayerSpawnLocation;

			var movementSync = new NetworkSync<MovementData>(
				player.Movement,
				peerId,
				$"player_{peerId}_movement",
				TransferMode.Unreliable
			);
			AddChild(movementSync);

			var healthSync = new NetworkSync<HealthData>(
				player.Health,
				peerId,
				$"player_{peerId}_health",
				TransferMode.Reliable
			);
			AddChild(healthSync);

			RemotePlayers[peerId] = (player, movementSync, healthSync);
		}
	}
	*/
}