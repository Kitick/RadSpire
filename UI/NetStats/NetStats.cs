using Godot;

namespace UI {
	public partial class NetStats : Control {
		Label lblPing = null!;
		Label lblLoss = null!;
		Label lblFPS = null!;

		private ulong _lastPingTime;

		// Main
		public override void _Ready() {
			SetCallbacks();

			lblFPS.Text = $"FPS: {Engine.GetFramesPerSecond()}";
		}

		// Ping
		private void SendPing() {
			_lastPingTime = Time.GetTicksMsec();
			RpcId(1, nameof(ReceivePingRequest), Multiplayer.GetUniqueId());
		}

		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
		public void ReceivePingRequest(int senderId) {
			RpcId(senderId, nameof(ReceivePingReply));
		}

		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
		public void ReceivePingReply() {
			ulong currentTime = Time.GetTicksMsec();
			ulong ping = currentTime - _lastPingTime;
			lblPing.Text = $"Ping: {ping} ms";
		}

		// Callbacks
		public void SetCallbacks() {
			lblPing = GetNode<Label>("NetStatsPanel/LblPing");
			lblLoss = GetNode<Label>("NetStatsPanel/LblLoss");
			lblFPS = GetNode<Label>("NetStatsPanel/LblFPS");
		}
	}
}