using Godot;
using System;
using SaveSystem;

public abstract partial class Character : CharacterBody3D, ISaveable<CharacterData> {
    [Export] private string characterName = "Unnamed";
    [Export] private float maxHealth = 100f;
    [Export] private bool isInvincible = false;
    [Export] private float speed = 2.0f;
    [Export] private float speedModifier = 1.0f;
    [Export] private float rotationSpeed = 5.0f;
    [Export] private float fallAcceleration = 9.8f;
    [Export] private float jumpForce = 6.0f;
    [Export] private string type = "Neutral";
    [Export] private bool useGravity = true;
    private float currentHealth;
    private bool isAlive;
    private Vector3 moveDirection = Vector3.Zero;
    private Vector3 faceDirection = Vector3.Zero;
    private bool inAir = false;
    private bool moving = false;

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
        currentHealth = maxHealth;
        isAlive = true;
        Velocity = Vector3.Zero;
    }

    public override void _PhysicsProcess(double delta) {
        float dt = (float)delta;
        ApplyGravity(dt);
        MatchRotationToDirection(dt);
        MoveCharacter(dt);
    }

    protected virtual void ApplyGravity(float delta) {
        if(useGravity) {
            if(!IsOnFloor()) {
                Velocity = new Vector3(Velocity.X, Velocity.Y - fallAcceleration * delta, Velocity.Z);
                EmitSignal(SignalName.Falling);
            }
            else if(Velocity.Y < 0) {
                Velocity = new Vector3(Velocity.X, 0, Velocity.Z);
            }    
        }
    }

    protected virtual void MoveCharacter(float delta) {
        if(isAlive) {
            if(moveDirection.LengthSquared() > 0.1f) {
                if(!moving) {
                    moving = true;
                    EmitSignal(SignalName.MoveStart);
                }
            }
            else if(moving) {
                moving = false;
                EmitSignal(SignalName.MoveStopped);
            }
            Vector3 newVelocity = Vector3.Zero;
            newVelocity.X = moveDirection.X * speed * speedModifier;
            newVelocity.Y = Velocity.Y;
            newVelocity.Z = moveDirection.Z * speed * speedModifier;
            Velocity = newVelocity;
            MoveAndSlide();
        }
    }
    
    protected virtual void MatchRotationToDirection(float delta) {
        if(isAlive) {
            if(moveDirection.LengthSquared() > 0.1f) {
                Vector3 newRotationVec = Vector3.Zero;
                newRotationVec.Y = Mathf.RadToDeg(Mathf.Atan2(faceDirection.X, faceDirection.Z));
                Transform3D newRotation = Transform;
                newRotation.Basis = new Basis(Vector3.Up, Mathf.DegToRad(newRotationVec.Y));
                Quaternion newRotationQ = new Quaternion(newRotation.Basis);
                Transform3D curRotation = Transform;
                Quaternion curRotationQ = new Quaternion(curRotation.Basis);
                float weight = 1f - Mathf.Exp(-rotationSpeed * delta);
                curRotationQ = curRotationQ.Slerp(newRotationQ, weight);
                curRotation.Basis = new Basis(curRotationQ);
                Transform = curRotation;
            }
        }
    }

    protected virtual void Jump() {
        if(isAlive) {
            if(IsOnFloor()) {
                Velocity = new Vector3(Velocity.X, jumpForce, Velocity.Z);
                inAir = true;
                EmitSignal(SignalName.Jumped);
            }
            if(inAir && IsOnFloor()) {
                inAir = false;
                EmitSignal(SignalName.Landed);
            }
        }

    }

    public virtual void TakeDamage(float amount) {
        if(isInvincible || !isAlive) {
            return;
        }
        else {
            currentHealth = Mathf.Max(0, currentHealth - amount);
            EmitSignal(SignalName.TookDamage, amount, currentHealth);
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
            EmitSignal(SignalName.Healed, amount, currentHealth);
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
            EmitSignal(SignalName.Healed, newHealth, newHealth);
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

    public virtual bool isMoving() {
        if(isAlive) {
            if(moveDirection.LengthSquared() > 0.1f) {
                return true;
            }
            return false;   
        }
        else {
            return false;
        }
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

    protected void setSpeed(float s) {
        speed = s;
    }

    public float getSpeed() {
        return speed;
    }

    protected void setSpeedModifier(float f) {
        speedModifier = f;
    }

    public float getSpeedModifier() {
        return speedModifier;
    }

    protected void setRotationSpeed(float r) {
        rotationSpeed = r;
    }

    public float getRotationSpeed() {
        return rotationSpeed;
    }

    protected void setFallAcceleration(float f) {
        fallAcceleration = f;
    }

    public float getFallAcceleration() {
        return fallAcceleration;
    }

    protected void setJumpForce(float j) {
        jumpForce = j;
    }

    public float getJumpForce() {
        return jumpForce;
    }

    protected void setType(string t) {
        type = t;
    }

    public string getType() {
        return type;
    }

    protected void setUseGravity(bool u) {
        useGravity = u;
    }

    public bool getUseGravity() {
        return useGravity;
    }

    protected void setMoveDirection(Vector3 d) {
        moveDirection = d;
    }

    public Vector3 getMoveDirection() {
        return moveDirection;
    }

    protected void setFaceDirection(Vector3 f) {
        faceDirection = f;
    }

    public Vector3 getFaceDirection() {
        return faceDirection;
    }

    protected void setInAir(bool i) {
        inAir = i;
    }
    
    public bool getInAir() {
        return inAir;
    }

    // ISaveable implementation
    public CharacterData Serialize() {
        return new CharacterData {
            CharacterName = characterName,
            CurrentHealth = currentHealth,
            MaxHealth = maxHealth,
            IsInvincible = isInvincible,
            IsAlive = isAlive,
            Position = GlobalPosition,
            Rotation = GlobalRotation,
            Velocity = Velocity,
            Speed = speed,
            SpeedModifier = speedModifier,
            RotationSpeed = rotationSpeed,
            FallAcceleration = fallAcceleration,
            JumpForce = jumpForce,
            Type = type,
            UseGravity = useGravity,
            MoveDirection = moveDirection,
            FaceDirection = faceDirection,
            InAir = inAir,
            Moving = moving
        };
    }

	public void Deserialize(in CharacterData data) {
        characterName = data.CharacterName;
        currentHealth = data.CurrentHealth;
        maxHealth = data.MaxHealth;
        isInvincible = data.IsInvincible;
        isAlive = data.IsAlive;
        GlobalPosition = data.Position;
        GlobalRotation = data.Rotation;
        Velocity = data.Velocity;
        speed = data.Speed;
        speedModifier = data.SpeedModifier;
        rotationSpeed = data.RotationSpeed;
        fallAcceleration = data.FallAcceleration;
        jumpForce = data.JumpForce;
        type = data.Type;
        useGravity = data.UseGravity;
        moveDirection = data.MoveDirection;
        faceDirection = data.FaceDirection;
        inAir = data.InAir;
        moving = data.Moving;
	}
}