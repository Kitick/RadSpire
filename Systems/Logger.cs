//This file was developed entirely by the RadSpire Development Team.

using System.Runtime.CompilerServices;
using Godot;

public readonly record struct Logger {
	public readonly string Prefix;
	public readonly bool Enabled;

	public Logger(string prefix, bool enabled = false) {
		Prefix = prefix;
		Enabled = enabled;
	}

	private readonly string FormatMessage(string message, string? method = null) {
		var methodPart = method != null ? $"({method})" : "";
		return $"[{Prefix}] {methodPart} {message}";
	}

	public readonly void Info(string message, [CallerMemberName] string? method = null) {
		if(Enabled) { GD.Print($"{FormatMessage(message, method)}"); }
	}

	public readonly void Warn(string message, [CallerMemberName] string? method = null) {
		if(Enabled) { GD.PrintErr($"{FormatMessage(message, method)}"); }
	}

	public readonly void Error(string message, [CallerMemberName] string? method = null) {
		GD.PrintErr($"{FormatMessage(message, method)}");
	}
}