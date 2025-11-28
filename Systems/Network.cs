using System;
using Godot;

namespace Network {
	public interface INetworkData;

	public interface INetworkable<T> where T : INetworkData {
		event Action? OnStateChanged;

		T Export();
		void Import(in T data);
	}

	public sealed partial class Network : Node {
		public static readonly bool Debug = true;

		private const int Port = 8080;
		private const int MaxPlayers = 10;

		private ENetMultiplayerPeer? Peer;
		public int CurrentPeerId { get; private set; } = -1;

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
	}
}