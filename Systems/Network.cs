using System;
using Godot;

namespace Network {
	public sealed partial class Network : Node {
		public static readonly bool Debug = true;

		private const int Port = 8080;

		public void Host() {
			if(Debug) { GD.Print("Network: Starting server"); }

			var peer = new ENetMultiplayerPeer();
			var result = peer.CreateServer(Port, 10);

			if(result != Error.Ok) {
				GD.PrintErr($"Network: Failed to start server: {result}");
				return;
			}

			Multiplayer.MultiplayerPeer = peer;
			if(Debug) { GD.Print("Network: Server started"); }

			Multiplayer.PeerConnected += id => {
				if(Debug) { GD.Print($"Network: Peer connected: {id}"); }
			};
		}

		public void Join(string address) {
			if(Debug) { GD.Print($"Network: Joining server at {address}:{Port}"); }

			var peer = new ENetMultiplayerPeer();
			var result = peer.CreateClient(address, Port);

			if(result != Error.Ok) {
				GD.PrintErr($"Network: Failed to join server: {result}");
				return;
			}

			Multiplayer.MultiplayerPeer = peer;
			if(Debug) { GD.Print("Network: Joining..."); }

			Multiplayer.ConnectedToServer += () => {
				if(Debug) { GD.Print($"Network: Connected to server: {address}:{Port}"); }
			};
		}
	}
}