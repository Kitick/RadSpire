using System;
using Godot;
using Systems.JSON;

namespace Network {
	public interface INetworkData : IJSONData;

	public interface INetworkable<T> : IJSONable<T> where T : INetworkData {
		event Action? OnStateChanged;
	}

	public sealed partial class Network : Node {
		public static readonly bool Debug = true;

		private const int Port = 8080;
		private const int MaxPlayers = 10;

		private ENetMultiplayerPeer? Peer;
		public int CurrentPeerId { get; private set; } = -1;
		public bool IsNetworkConnected => Peer != null && Multiplayer.MultiplayerPeer != null;
		public bool IsHost => IsNetworkConnected && Multiplayer.IsServer();

		public event Action? OnHostStarted;
		public event Action? OnJoinedServer;
		public event Action? OnServerDisconnected;
		public event Action<int>? OnPeerConnected;
		public event Action<int>? OnPeerDisconnected;

		public override void _Ready() {
			Multiplayer.PeerConnected += id => OnPeerConnected?.Invoke((int) id);
			Multiplayer.PeerDisconnected += id => OnPeerDisconnected?.Invoke((int) id);
			Multiplayer.ConnectedToServer += () => {
				CurrentPeerId = Multiplayer.GetUniqueId();
				OnJoinedServer?.Invoke();
			};
			Multiplayer.ServerDisconnected += () => {
				if(Debug) { GD.Print("Network: Server disconnected"); }
				Disconnect();
				OnServerDisconnected?.Invoke();
			};
		}

		public void Host() {
			if(Debug) { GD.Print("Network: Starting server"); }

			Peer = new ENetMultiplayerPeer();
			var result = Peer.CreateServer(Port, MaxPlayers);

			if(result != Error.Ok) {
				GD.PrintErr($"Network: Failed to start server: {result}");
				return;
			}

			Multiplayer.MultiplayerPeer = Peer;
			CurrentPeerId = Multiplayer.GetUniqueId();
			if(Debug) { GD.Print($"Network: Server started (Peer ID: {CurrentPeerId})"); }
			OnHostStarted?.Invoke();
		}

		public void Join(string address) {
			if(Debug) { GD.Print($"Network: Joining server at {address}:{Port}"); }

			Peer = new ENetMultiplayerPeer();
			var result = Peer.CreateClient(address, Port);

			if(result != Error.Ok) {
				GD.PrintErr($"Network: Failed to join server: {result}");
				return;
			}

			Multiplayer.MultiplayerPeer = Peer;
			if(Debug) { GD.Print("Network: Attempting to join server..."); }
		}

		public void Disconnect() {
			if(Peer == null) return;

			if(Debug) { GD.Print("Network: Disconnecting"); }
			Peer.Close();
			Peer = null;
			Multiplayer.MultiplayerPeer = null;
			CurrentPeerId = -1;
		}
	}
}