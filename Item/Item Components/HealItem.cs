namespace Components;

using Godot;
using ItemSystem;
using Services;
public interface IHealItem { HealItem Heal { get; set; } }

public sealed class HealItem : Component<HealItemData>, IItemComponent, IItemUseable {
	public int priority { get; init; } = 0;
	private static readonly LogService Log = new(nameof(HealItem), enabled: true);
	public int HealAmount {
		get => Data.HealAmount;
		set => SetData(Data with { HealAmount = value });
	}

	public string[] getComponentDescription() {
		string componentDescription = $"+{HealAmount} Healing";
		return new string[] { componentDescription };
	}

	public bool Use<TEntity>(TEntity user) {
		Log.Info($"Attempt using HealItem with HealAmount {HealAmount} on user");
		if(user is IHealth healthEntity) {
			Log.Info("User is IHealth, attempting to heal.");
			if(healthEntity.IsHurt()) {
				Log.Info($"Health is hurt, healing for {HealAmount}.");
				healthEntity.Heal(HealAmount);
				return true;
			}
			else {
				Log.Info("Health is not hurt, cannot heal.");
			}
		}
		return false;
	}

	public HealItem(HealItemData data) : base(data) { }
	public HealItem(int amount) : base(new HealItemData { HealAmount = amount }) { }
}

public readonly record struct HealItemData : Services.ISaveData {
	public int HealAmount { get; init; }
}
