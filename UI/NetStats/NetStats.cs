using Godot;
using System;

public partial class NetStats : Control {
    
    Label lblPing, lblLoss, lblFPS;

    private double pingSendTimer = 0.0;
    private const double PingSendInterval = 0.5;
    private double smoothedPingMs = -1.0;
    private const double PingSmoothingAlpha = 0.15;

    // Main
    public override void _Process(double delta) {
        SetCallbacks();

        pingSendTimer += delta;
        if (pingSendTimer >= PingSendInterval)
        {
            pingSendTimer = 0.0;
            SendPing();
        }

            lblFPS.Text = $"FPS: {Engine.GetFramesPerSecond()}";
    }

    // Ping
    private void SendPing() {
        ulong sentUsec = Godot.Time.GetTicksUsec();
        int myId = GetMultiplayer().GetUniqueId();
        
        
        RpcId(1, "ServerEchoPing", sentUsec);
    }

    [Rpc]
    public void ServerEchoPing(ulong clientTimestampUsec, int clientId) {
        RpcId(clientId, "ClientReceivePong", clientTimestampUsec);
    }
    
    [Rpc]
    public void ClientReceivePong(ulong clientTimestampUsec) {
        ulong nowUsec = Godot.Time.GetTicksUsec();
        double rttMs = (nowUsec - clientTimestampUsec) / 1000.0;

        if (smoothedPingMs < 0) {
            smoothedPingMs = rttMs;
        }
            
        else {
            smoothedPingMs = smoothedPingMs * (1.0 - PingSmoothingAlpha) + rttMs * PingSmoothingAlpha;
        }      

        lblPing.Text = $"Ping: {smoothedPingMs:F1} ms";
    }

    // Callbacks
    public void SetCallbacks() {
        lblPing = GetNode<Label>("NetStatsPanel/LblPing");
        lblLoss = GetNode<Label>("NetStatsPanel/LblLoss");
        lblFPS = GetNode<Label>("NetStatsPanel/LblFPS");
    }
}