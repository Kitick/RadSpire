using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Network {
	/// <summary>
	/// Example scene demonstrating networked multiplayer with multiple players.
	/// Each player controls their own character, sends position to host, host validates and broadcasts.
	/// </summary>
	public sealed partial class NetworkExample : Node2D {
		private readonly Dictionary<int, PlayerVisual> Players = new();
		private Network Network = null!;

		public override void _Ready() {
			// Get or create Network manager
			Network = GetNodeOrNull<Network>("/root/Network") ?? new Network();
			if(Network.GetParent() == null) {
				GetTree().Root.CallDeferred("add_child", Network);
			}

			// Connect to multiplayer signals
			Multiplayer.PeerConnected += OnPeerConnected;
			Multiplayer.PeerDisconnected += OnPeerDisconnected;
			Multiplayer.ConnectedToServer += OnConnectedToServer;
			Multiplayer.ServerDisconnected += OnServerDisconnected;

			GD.Print("NetworkExample: Ready. Press H to Host, J to Join localhost");
		}

		public override void _ExitTree() {
			Multiplayer.PeerConnected -= OnPeerConnected;
			Multiplayer.PeerDisconnected -= OnPeerDisconnected;
			Multiplayer.ConnectedToServer -= OnConnectedToServer;
			Multiplayer.ServerDisconnected -= OnServerDisconnected;
		}

		public override void _Process(double delta) {
			// Host/Join controls
			if(Input.IsKeyPressed(Key.H) && !_hostPressed) {
				_hostPressed = true;
				if(Network.IsNetworkConnected) {
					GD.Print("NetworkExample: Already connected, cannot host again");
				} else {
					GD.Print("NetworkExample: H pressed, hosting...");
					Network.Host();
					SpawnLocalPlayer(Multiplayer.GetUniqueId());
				}
			}
			if(!Input.IsKeyPressed(Key.H)) { _hostPressed = false; }

			if(Input.IsKeyPressed(Key.J) && !_joinPressed) {
				_joinPressed = true;
				if(Network.IsHost) {
					GD.Print("NetworkExample: You are the host, cannot join yourself");
				} else if(Network.IsNetworkConnected) {
					GD.Print("NetworkExample: Already connected to a server");
				} else {
					GD.Print("NetworkExample: J pressed, joining...");
					Network.Join("127.0.0.1");
				}
			}
			if(!Input.IsKeyPressed(Key.J)) { _joinPressed = false; }

			// Disconnect control
			if(Input.IsKeyPressed(Key.Escape) && !_escPressed) {
				_escPressed = true;
				if(Network.IsNetworkConnected) {
					GD.Print("NetworkExample: ESC pressed, disconnecting...");
					DisconnectAndCleanup();
				}
			}
			if(!Input.IsKeyPressed(Key.Escape)) { _escPressed = false; }

			// Update local player movement (only if connected)
			if(Network.IsNetworkConnected) {
				var myId = Multiplayer.GetUniqueId();
				if(Players.TryGetValue(myId, out var localPlayer)) {
					localPlayer.Controller.Update((float)delta);
				}
			}
		}

		private bool _hostPressed = false;
		private bool _joinPressed = false;
		private bool _escPressed = false;

		private void DisconnectAndCleanup() {
			// Remove all players
			foreach(var peerId in Players.Keys.ToArray()) {
				RemovePlayer(peerId);
			}
			Network.Disconnect();
			GD.Print("NetworkExample: Disconnected. Press H to Host, J to Join");
		}

		private void OnConnectedToServer() {
			GD.Print($"NetworkExample: Connected to server as peer {Multiplayer.GetUniqueId()}");

			// Request existing players from host FIRST, then spawn ourselves
			// This ensures the host creates our NetworkSync before we start sending
			RpcId(1, nameof(RequestSpawnOnHost));
		}

		private void OnPeerConnected(long peerId) {
			GD.Print($"NetworkExample: Peer {peerId} connected");

			// Host spawns the remote player immediately when they connect
			// This ensures the NetworkSync exists before the client sends data
			if(Multiplayer.IsServer() && peerId != 1) {
				SpawnRemotePlayer((int)peerId);

				// Tell ALL existing clients about the new player
				// This ensures everyone has the NetworkSync node before broadcasts happen
				Rpc(nameof(SpawnRemotePlayer), (int)peerId);

				// Tell the new peer about all existing players
				foreach(var existingPeerId in Players.Keys) {
					if(existingPeerId != peerId) {
						RpcId(peerId, nameof(SpawnRemotePlayer), existingPeerId);
					}
				}

				// Tell the new peer to spawn their local player now that host is ready
				RpcId(peerId, nameof(SpawnYourLocalPlayer));
			}
		}

		private void OnPeerDisconnected(long peerId) {
			GD.Print($"NetworkExample: Peer {peerId} disconnected");
			RemovePlayer((int)peerId);
		}

		private void OnServerDisconnected() {
			GD.Print("NetworkExample: Server disconnected, cleaning up...");
			DisconnectAndCleanup();
		}

		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
		private void RequestSpawnOnHost() {
			if(!Multiplayer.IsServer()) return;

			var requesterId = Multiplayer.GetRemoteSenderId();
			GD.Print($"NetworkExample: Host received spawn request from peer {requesterId}");

			// Spawn their remote representation on host if not already done
			SpawnRemotePlayer(requesterId);

			// Send them all existing players
			foreach(var existingPeerId in Players.Keys) {
				if(existingPeerId != requesterId) {
					RpcId(requesterId, nameof(SpawnRemotePlayer), existingPeerId);
				}
			}

			// Tell them to spawn their local player
			RpcId(requesterId, nameof(SpawnYourLocalPlayer));
		}

		[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false)]
		private void SpawnYourLocalPlayer() {
			GD.Print($"NetworkExample: Host told us to spawn local player");
			SpawnLocalPlayer(Multiplayer.GetUniqueId());
		}

		private void SpawnLocalPlayer(int peerId) {
			if(Players.ContainsKey(peerId)) return;

			var spawnPos = GetSpawnPosition(peerId);
			var color = GetPlayerColor(peerId);

			var visual = new ColorRect {
				Size = new Vector2(50, 50),
				Color = color,
				Position = spawnPos
			};
			AddChild(visual);

			var controller = new PlayerController(visual);
			var sync = new NetworkSync<PlayerPositionData>(controller, peerId, "player") {
				Validator = ValidateMovement
			};
			AddChild(sync);

			Players[peerId] = new PlayerVisual(visual, controller, sync);
			GD.Print($"NetworkExample: Spawned LOCAL player {peerId} at {spawnPos}");

			// If we're the host, tell all clients about this new player
			if(Multiplayer.IsServer()) {
				Rpc(nameof(SpawnRemotePlayer), peerId);
			}
			// Clients don't need to notify - the host already spawned us in OnPeerConnected/RequestSpawnOnHost
		}

		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
		private void SpawnRemotePlayer(int peerId) {
			if(Players.ContainsKey(peerId)) return;
			if(peerId == Multiplayer.GetUniqueId()) return; // Don't spawn ourselves as remote

			var spawnPos = GetSpawnPosition(peerId);
			var color = GetPlayerColor(peerId);

			var visual = new ColorRect {
				Size = new Vector2(50, 50),
				Color = color,
				Position = spawnPos
			};
			AddChild(visual);

			var controller = new PlayerController(visual);
			var sync = new NetworkSync<PlayerPositionData>(controller, peerId, "player");
			AddChild(sync);

			Players[peerId] = new PlayerVisual(visual, controller, sync);
			GD.Print($"NetworkExample: Spawned REMOTE player {peerId} at {spawnPos}");
		}

		private void RemovePlayer(int peerId) {
			if(!Players.TryGetValue(peerId, out var player)) return;

			player.Visual.QueueFree();
			player.Sync.QueueFree();
			Players.Remove(peerId);
			GD.Print($"NetworkExample: Removed player {peerId}");
		}

		/// <summary>
		/// Host-side validation. For now, just returns the input unchanged.
		/// In a real game, you'd check for teleportation, speed hacks, etc.
		/// </summary>
		private PlayerPositionData? ValidateMovement(PlayerPositionData data) {
			// TODO: Add actual validation logic
			// e.g., check if movement distance is reasonable, check bounds, etc.
			return data;
		}

		private static Vector2 GetSpawnPosition(int peerId) {
			// Spread players out based on peer ID
			var index = peerId % 10;
			return new Vector2(100 + (index * 80), 200);
		}

		private static Color GetPlayerColor(int peerId) {
			// Different color per player
			return peerId switch {
				1 => Colors.Blue,      // Host is always peer 1
				_ => new Color((peerId * 0.3f) % 1f, (peerId * 0.5f) % 1f, (peerId * 0.7f) % 1f)
			};
		}

		private readonly record struct PlayerVisual(ColorRect Visual, PlayerController Controller, NetworkSync<PlayerPositionData> Sync);
	}
}
