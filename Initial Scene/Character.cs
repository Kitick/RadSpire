using Godot;
using System;
using SaveSystem;

public abstract partial class Character : CharacterBody3D, ISaveable<CharacterData> {
    [Export] public string CharacterName { get; protected set; } = "Unnamed";
    [Export] public float MaxHealth { get; protected set { CurrentHealth = MaxHealth;} } = 100f;
    [Export] public bool IsInvincible { get; protected set; } = false;
    [Export] public float Speed { get; protected set; } = 2.0f;
    [Export] public float SpeedModifier { get; protected set; } = 1.0f;
    [Export] public  float RotationSpeed { get; protected set; } = 5.0f;
    [Export] public float FallAcceleration { get; protected set; } = 9.8f;
    [Export] public float JumpForce { get; protected set; } = 6.0f;
    [Export] public string Type { get; protected set; } = "Neutral";
    [Export] public bool UseGravity { get; protected set; } = true;
    public float CurrentHealth { get; private set; }
    public bool IsAlive { get; private set; }
    public Vector3 MoveDirection { get; protected set; } = Vector3.Zero;
    public Vector3 FaceDirection { get; protected set; } = Vector3.Zero;
    public bool InAir { get; private set; } = false;
    public bool Moving { get{ return isMoving(); } private set; } = false;

    [Signal] public delegate void TookDamageEventHandler(float amount, float newHealth);
    [Signal] public delegate void HealedEventHandler(float amount, float newHealth);
    [Signal] public delegate void DiedEventHandler();
    [Signal] public delegate void ReviveEventHandler(float newHealth);
    [Signal] public delegate void MoveStartEventHandler();
    [Signal] public delegate void MoveStoppedEventHandler();
    [Signal] public delegate void JumpedEventHandler();
    [Signal] public delegate void LandedEventHandler();
    [Signal] public delegate void FallingEventHandler();

    public override void _Ready() {
        CurrentHealth = MaxHealth;
        IsAlive = true;
        Velocity = Vector3.Zero;
    }

    public override void _PhysicsProcess(double delta) {
        float dt = (float)delta;
        ApplyGravity(dt);
        MatchRotationToDirection(dt);
        MoveCharacter(dt);
    }

    protected virtual void ApplyGravity(float delta) {
        if(UseGravity) {
            if(!IsOnFloor()) {
                Velocity = new Vector3(Velocity.X, Velocity.Y - FallAcceleration * delta, Velocity.Z);
                EmitSignal(SignalName.Falling);
            }
            else if(Velocity.Y < 0) {
                Velocity = new Vector3(Velocity.X, 0, Velocity.Z);
            }    
        }
    }

    protected virtual void MoveCharacter(float delta) {
        if(IsAlive) {
            if(MoveDirection.LengthSquared() > 0.1f) {
                if(!Moving) {
                    Moving = true;
                    EmitSignal(SignalName.MoveStart);
                }
            }
            else if(Moving) {
                Moving = false;
                EmitSignal(SignalName.MoveStopped);
            }
            Vector3 newVelocity = Vector3.Zero;
            newVelocity.X = MoveDirection.X * Speed * SpeedModifier;
            newVelocity.Y = Velocity.Y;
            newVelocity.Z = MoveDirection.Z * Speed * SpeedModifier;
            Velocity = newVelocity;
            MoveAndSlide();
        }
    }
    
    protected virtual void MatchRotationToDirection(float delta) {
        if(IsAlive) {
            if(MoveDirection.LengthSquared() > 0.1f) {
                Vector3 newRotationVec = Vector3.Zero;
                newRotationVec.Y = Mathf.RadToDeg(Mathf.Atan2(FaceDirection.X, FaceDirection.Z));
                Transform3D newRotation = Transform;
                newRotation.Basis = new Basis(Vector3.Up, Mathf.DegToRad(newRotationVec.Y));
                Quaternion newRotationQ = new Quaternion(newRotation.Basis);
                Transform3D curRotation = Transform;
                Quaternion curRotationQ = new Quaternion(curRotation.Basis);
                float weight = 1f - Mathf.Exp(-RotationSpeed * delta);
                curRotationQ = curRotationQ.Slerp(newRotationQ, weight);
                curRotation.Basis = new Basis(curRotationQ);
                Transform = curRotation;
            }
        }
    }

    protected virtual void Jump() {
        if(IsAlive) {
            if(IsOnFloor()) {
                Velocity = new Vector3(Velocity.X, JumpForce, Velocity.Z);
                InAir = true;
                EmitSignal(SignalName.Jumped);
            }
            if(InAir && IsOnFloor()) {
                InAir = false;
                EmitSignal(SignalName.Landed);
            }
        }

    }

    public virtual void TakeDamage(float amount) {
        if(IsInvincible || !IsAlive) {
            return;
        }
        else {
            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            EmitSignal(SignalName.TookDamage, amount, CurrentHealth);
            if(CurrentHealth == 0) {
                Die();
            }   
        }

    }

    public virtual void RestoreDamage(float amount) {
        if(!IsAlive || IsInvincible) {
            return;
        }
        else {
            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
            EmitSignal(SignalName.Healed, amount, CurrentHealth);
        }
    }

    public virtual bool ReviveCharacter(float newHealth) {
        if(IsAlive || IsInvincible) {
            return false;
        }
        else {
            IsAlive = true;
            CurrentHealth = newHealth;
            EmitSignal(SignalName.Revive, CurrentHealth);
            EmitSignal(SignalName.Healed, newHealth, newHealth);
            return true;
        }
    }

    public virtual bool Die() {
        if(IsAlive && !IsInvincible) {
            IsAlive = false;
            EmitSignal(SignalName.Died);
            return true;
        }
        return false;
    }

    public virtual bool isMoving() {
        if(IsAlive) {
            if(MoveDirection.LengthSquared() > 0.1f) {
                return true;
            }
            return false;
        }
        else {
            return false;
        }
    }

    // ISaveable implementation
    public CharacterData Serialize() {
        return new CharacterData {
            CharacterName = CharacterName,
            CurrentHealth = CurrentHealth,
            MaxHealth = MaxHealth,
            IsInvincible = IsInvincible,
            IsAlive = IsAlive,
            Position = GlobalPosition,
            Rotation = GlobalRotation,
            Velocity = Velocity,
            Speed = Speed,
            SpeedModifier = SpeedModifier,
            RotationSpeed = RotationSpeed,
            FallAcceleration = FallAcceleration,
            JumpForce = JumpForce,
            Type = Type,
            UseGravity = UseGravity,
            MoveDirection = MoveDirection,
            FaceDirection = FaceDirection,
            InAir = InAir,
            Moving = Moving
        };
    }

	public void Deserialize(in CharacterData data) {
        CharacterName = data.CharacterName;
        CurrentHealth = data.CurrentHealth;
        MaxHealth = data.MaxHealth;
        IsInvincible = data.IsInvincible;
        IsAlive = data.IsAlive;
        GlobalPosition = data.Position;
        GlobalRotation = data.Rotation;
        Velocity = data.Velocity;
        Speed = data.Speed;
        SpeedModifier = data.SpeedModifier;
        RotationSpeed = data.RotationSpeed;
        FallAcceleration = data.FallAcceleration;
        JumpForce = data.JumpForce;
        Type = data.Type;
        UseGravity = data.UseGravity;
        MoveDirection = data.MoveDirection;
        FaceDirection = data.FaceDirection;
        InAir = data.InAir;
        Moving = data.Moving;
	}
}