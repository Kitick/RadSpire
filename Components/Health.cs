using System;
using SaveSystem;

public class HealthComponent {
	public enum Status { Alive, Dead }

	public int MaxHealth {
		get;
		set {
			field = Math.Max(0, value);
			CurrentHealth = CurrentHealth; // Call setter
		}
	}

	public int CurrentHealth {
		get;
		set {
			value = Math.Clamp(value, 0, MaxHealth);
			if(field == value){ return; }

			HealthChanged?.Invoke(field, value);

			if(field == 0){ StatusChanged?.Invoke(Status.Alive); }
			else if(value == 0){ StatusChanged?.Invoke(Status.Dead); }

			field = value;
		}
	}

	public Status State {
		get => field = CurrentHealth > 0 ? Status.Alive : Status.Dead;
		set {
			if(field == value) { return; }

			CurrentHealth = value switch {
				Status.Alive => MaxHealth,
				Status.Dead => 0,
				_ => CurrentHealth,
			};
		}
	}

	public event Action<int, int>? HealthChanged;
	public event Action<Status>? StatusChanged;

	public HealthComponent(int maxHealth) {
		MaxHealth = maxHealth;
		CurrentHealth = maxHealth;
	}
}