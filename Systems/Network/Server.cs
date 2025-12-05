using System;
using Godot;
using Systems.JSON;

namespace Network {
	public interface INetworkData : IJSONData;

	public interface INetworkable<T> : IJSONable<T> where T : INetworkData {
		event Action? OnStateChanged;
	}

	public sealed partial class Server : Node {
		public static Server Instance { get; private set; } = null!;

		private static readonly Logger Log = new(nameof(Server), enabled: true);

		private const int Port = 8080;
		private const int MaxPlayers = 10;

		private ENetMultiplayerPeer? Peer;
		public int PeerId => Multiplayer.GetUniqueId();
		public int RemoteSenderId => Multiplayer.GetRemoteSenderId();

		public bool IsNetworkConnected => Peer != null && Multiplayer.MultiplayerPeer != null;
		public bool IsHost => IsNetworkConnected && Multiplayer.IsServer();
		public bool IsOwner(int peerId) => PeerId == peerId;

		public event Action? OnHostStarted;
		public event Action? OnJoinedServer;
		public event Action? OnServerDisconnected;
		public event Action<int>? OnPeerConnected;
		public event Action<int>? OnPeerDisconnected;

		public override void _Ready() {
			Instance = this;
			SetCallbacks();
		}

		private void SetCallbacks() {
			Multiplayer.PeerConnected += id => OnPeerConnected?.Invoke((int) id);
			Multiplayer.PeerDisconnected += id => OnPeerDisconnected?.Invoke((int) id);
			Multiplayer.ConnectedToServer += () => OnJoinedServer?.Invoke();
			Multiplayer.ServerDisconnected += () => {
				Disconnect();
				OnServerDisconnected?.Invoke();
			};
		}

		public Error Host() {
			Log.Info("Starting server");

			Peer = new ENetMultiplayerPeer();

			Error result = Peer.CreateServer(Port, MaxPlayers);

			if(result != Error.Ok) {
				Log.Error($"Failed to start server: {result}");
				return result;
			}

			Multiplayer.MultiplayerPeer = Peer;

			Log.Info($"Server started (Peer ID: {PeerId})");
			OnHostStarted?.Invoke();

			return result;
		}

		public Error Join(string address) {
			Log.Info($"Joining server at {address}:{Port}");

			Peer = new ENetMultiplayerPeer();
			Error result = Peer.CreateClient(address, Port);

			if(result != Error.Ok) {
				Log.Error($"Failed to join server: {result}");
				return result;
			}

			Multiplayer.MultiplayerPeer = Peer;
			Log.Info("Attempting to join server...");

			return result;
		}

		public void Disconnect() {
			if(Peer == null) { return; }

			Log.Info("Disconnecting");
			Peer.Close();
			Peer = null;
			Multiplayer.MultiplayerPeer = null;
		}
	}
}