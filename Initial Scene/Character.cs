using Godot;
using System;
using SaveSystem;
using System.Data.Common;

public partial class Character : CharacterBody3D, ISaveable<CharacterData> {
    [Export] private string characterName = "Unnamed";
    [Export] private float maxHealth = 100f;
    [Export] private bool isInvincible = false;
    private float currentHealth;
    private bool isAlive;

    [Signal] public delegate void HealthChangeEventHandler(float amount, float newValue);
    [Signal] public delegate void DiedEventHandler();
    [Signal] public delegate void ReviveEventHandler(float newHealth);

	public override void _Ready() {
        currentHealth = maxHealth;
        isAlive = true;
    }

    public virtual void TakeDamage(float amount) {
        if(isInvincible || !isAlive) {
            return;
        }
        else {
            currentHealth = Mathf.Max(0, currentHealth - amount);
            EmitSignal(SignalName.HealthChange, -amount, currentHealth);
            if(currentHealth == 0) {
                Die();
            }   
        }

    }

    public virtual void RestoreDamage(float amount) {
        if(!isAlive || isInvincible) {
            return;
        }
        else {
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            EmitSignal(SignalName.HealthChange, amount, currentHealth);
        }
    }

    public virtual bool ReviveCharacter(float newHealth) {
        if(isAlive || isInvincible) {
            return false;
        }
        else {
            isAlive = true;
            currentHealth = newHealth;
            EmitSignal(SignalName.Revive, currentHealth);
            EmitSignal(SignalName.HealthChange, newHealth, newHealth);
            return true;
        }
        
    }

    public virtual bool Die() {
        if(isAlive && !isInvincible) {
            isAlive = false;
            EmitSignal(SignalName.Died);
            return true;
        }
        return false;
    }

    protected void setCharacterName(string name) {
        characterName = name;
    }

    public string getCharacterName() {
        return characterName;
    }

    protected void setMaxHealth(float max) {
        maxHealth = max;
        currentHealth = maxHealth;
    }

    public float getMaxHealth() {
        return maxHealth;
    }

    protected void setIsInvincible(bool inv) {
        isInvincible = inv;
    }

    public bool getIsInvincible() {
        return isInvincible;
    }

    public float getCurrentHealth() {
        return currentHealth;
    }

    public bool getIsAlive() {
        return isAlive;
    }

    // ISaveable implementation
	public CharacterData Serialize() {
		return new CharacterData {
            CharacterName = characterName,
            CurrentHealth = currentHealth,
            MaxHealth = maxHealth,
            IsInvincible = isInvincible,
            IsAlive = isAlive
        };
	}

	public void Deserialize(in CharacterData data) {
        characterName = data.CharacterName;
        currentHealth = data.CurrentHealth;
        maxHealth = data.MaxHealth;
        isInvincible = data.IsInvincible;
        isAlive = data.IsAlive;
        
	}
}