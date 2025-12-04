using System.Collections.Generic;
using Core;
using Godot;
using Network;
using SaveSystem;

public partial class GameManager {
	private readonly Dictionary<int, Player> RemotePlayers = [];
	private readonly Dictionary<int, NetworkSync<MovementData>> PlayerSyncs = [];

	public int LocalPeerId => Server.PeerId;

	private Server Server = null!;
	private NetworkSync<MovementData>? LocalPlayerSync;

	private void InitializeNetwork() {
		Server = Server.Instance;
		SubscribeToNetworkEvents();
	}

	private void CleanupNetwork() {
		UnsubscribeFromNetworkEvents();
		CleanupPlayerSyncs();
	}

	private void CleanupPlayerSyncs() {
		LocalPlayerSync?.QueueFree();
		LocalPlayerSync = null;

		foreach(var sync in PlayerSyncs.Values) {
			sync.QueueFree();
		}
		PlayerSyncs.Clear();
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

		LocalPlayerSync = new NetworkSync<MovementData>(
			LocalPlayer.Movement,
			LocalPeerId,
			$"player_{LocalPeerId}",
			TransferMode.Unreliable
		);
		AddChild(LocalPlayerSync);
	}

	private void OnHostStarted() {
		Log($"Host started, local peer ID: {LocalPeerId}");
		CreateLocalPlayerSync();
	}

	private void OnJoinedServer() {
		Log($"Joined server, local peer ID: {LocalPeerId}");
		CreateLocalPlayerSync();
		RpcId(1, nameof(NotifyHostOfNewPlayer), LocalPeerId);
	}

	private void OnServerDisconnected() {
		Log("Server disconnected, cleaning up remote players");
		CleanupPlayerSyncs();
		foreach(var (peerId, player) in RemotePlayers) {
			player.QueueFree();
		}
		RemotePlayers.Clear();
	}

	private void OnPeerConnected(int peerId) {
		Log($"Peer connected: {peerId}");
	}

	private void OnPeerDisconnected(int peerId) {
		Log($"Peer disconnected: {peerId}");
		if(PlayerSyncs.TryGetValue(peerId, out var sync)) {
			sync.QueueFree();
			PlayerSyncs.Remove(peerId);
		}
		if(RemotePlayers.TryGetValue(peerId, out var player)) {
			player.QueueFree();
			RemotePlayers.Remove(peerId);
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void NotifyHostOfNewPlayer(int newPeerId) {
		if(!Multiplayer.IsServer()) return;

		Log($"Host notified of new player: {newPeerId}");

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

		Log($"Spawning remote player: {peerId}");

		var remotePlayer = this.AddScene<Player>(Scenes.Player);
		remotePlayer.Name = $"Player_{peerId}";
		RemotePlayers[peerId] = remotePlayer;

		// Create NetworkSync for the remote player's movement (they own it)
		var sync = new NetworkSync<MovementData>(
			remotePlayer.Movement,
			peerId,
			$"player_{peerId}",
			TransferMode.Unreliable
		);
		AddChild(sync);
		PlayerSyncs[peerId] = sync;
	}
}
