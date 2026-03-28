using Godot;
using System;
using static Godot.TextServer;

public partial class Player : CharacterBody2D
{
    [ExportGroup("Movement")]
    [Export] public float Speed = 800.0f;
    [Export] public float Acceleration = 1500.0f;
    [Export] public float Friction = 3000.0f;
    [Export] public float AirResistance = 1000.0f;
    [Export] public float BufferWindow = 0.5f; // How long the speed lags behind

    [ExportGroup("Jump")]
    [Export] public float JumpVelocity = -1000.0f;
    [Export] public float AirJumpVelocity = -800.0f;
    [Export] public int MaxAirJumps = 0;
    [Export] public float Gravity = 2500.0f;

    [ExportGroup("Slide")]
    [Export] public float SlideBoost = 1.0f;
    [Export] public float SlideCooldown = 1.5f; // Seconds between slides
    [Export] public float UltraSlideBoost = 1.3f;

    [ExportGroup("Slam")]
    [Export] public float SlamVelocity = 1500.0f;
    [Export] public float SlamCooldown = 2.0f;

    // Privates
    private int _airJumpsReached = 0;
    private bool isSliding = false;
    private float _slideCooldownTimer = 0.0f;
    private float _slamCooldownTimer = 0.0f;

    // Nodes
    private Label _label;
    private Sprite2D _sprite;

    // Velocity Buffer
    private float _velocityBufferY = 0.0f;
    private float _bufferDecayTimer = 0.0f;


    public override void _Ready()
    {
        _label = GetNode<Label>("Label");
        _sprite = GetNode<Sprite2D>("Sprite2D");
    }

    public override void _Process(double delta)
    {
        _label.Text = $"Velocity: {Velocity}, Sliding: {isSliding}";
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;
        float direction = Input.GetAxis("move_left", "move_right");

        // --- Timers ---
        HandleCooldowns(delta);

        // --- Gravity & Jump Reset ---
        if (!IsOnFloor())
        {
            HandleGravity(ref velocity, delta);
        }
        else
        {
            _airJumpsReached = 0;
        }

        // --- Handle Inputs and sprite ---
        HandleSpriteFlip(direction);
        HandleJump(ref velocity);
        HandleSlide(ref velocity, direction);
        HandleSlam(ref velocity);

        // --- Horizontal Movement ---
        if (isSliding)
        {
            MaintainSlideVelocity(ref velocity, direction);
        }
        else
        {
            HandleMovement(ref velocity, direction, delta);
        }

        Velocity = velocity;
        MoveAndSlide();
    }



    // FUNCTIONS
    private void HandleCooldowns(double delta)
    {
        if (_slideCooldownTimer > 0) _slideCooldownTimer -= (float)delta;
        if (_slamCooldownTimer > 0) _slamCooldownTimer -= (float)delta;

        // Decay
        if (_bufferDecayTimer > 0)
        {
            _bufferDecayTimer -= (float)delta;
        }
        else
        {
            //reset the buffer to 0 when the window expires
            _velocityBufferY = Mathf.MoveToward(_velocityBufferY, 0, Friction * (float)delta);
        }
    }
    private void HandleSpriteFlip(float direction)
    {
        if (direction > 0) _sprite.FlipH = false;
        else if (direction < 0) _sprite.FlipH = true;
    }
    private void HandleMovement(ref Vector2 velocity, float direction, double delta)
    {
        if (direction != 0)
        {
            velocity.X = Mathf.MoveToward(velocity.X, direction * Speed, Acceleration * (float)delta);
        }
        else
        {
            float currentFriction = IsOnFloor() ? Friction : AirResistance;
            velocity.X = Mathf.MoveToward(velocity.X, 0, currentFriction * (float)delta);
        }
    }
    private void HandleJump(ref Vector2 velocity)
    {
        if (Input.IsActionJustPressed("jump"))
        {
            if (IsOnFloor())
            {
                velocity.Y = JumpVelocity;
            }
            else if (_airJumpsReached < MaxAirJumps)
            {
                velocity.Y = AirJumpVelocity;
                _airJumpsReached++;
            }
        }
    }
    private void HandleSlide(ref Vector2 velocity, float direction)
    {
        if (Input.IsActionJustPressed("slide") && IsOnFloor() && _slideCooldownTimer <= 0)
        {
            isSliding = true;
            _slideCooldownTimer = SlideCooldown;

            float slideDir = (Math.Abs(velocity.X) > 0.1f) ? Math.Sign(velocity.X) : direction;

            if (slideDir != 0)
            {
                // Base slide speed
                float finalSpeed = Speed * SlideBoost;

                // If WE have a buffer, add a portion of it to horizontal speed
                if (_bufferDecayTimer > 0)
                {
                    // take the slam velocity and convert it to horizontal (im calling this ultraslide)
                    finalSpeed += _velocityBufferY * UltraSlideBoost;
                    GD.Print("ULTRA SLIDE");
                    _bufferDecayTimer = 0; // Consume the buffer so you can't double dip
                }

                velocity.X = slideDir * finalSpeed;
            }
        }

        if (Input.IsActionJustReleased("slide"))
        {
            isSliding = false;
        }
    }
    private void MaintainSlideVelocity(ref Vector2 velocity, float direction)
    {
        float slideDir = (Math.Abs(velocity.X) > 0.1f) ? Math.Sign(velocity.X) : direction;
        float baseSlideSpeed = Speed * SlideBoost;

        // Fix for ultraslide
        if (Math.Abs(velocity.X) < baseSlideSpeed)
        {
            velocity.X = slideDir * baseSlideSpeed;
        }

        velocity.X = Mathf.MoveToward(velocity.X, slideDir * baseSlideSpeed, Friction * 0.2f * (float)GetPhysicsProcessDeltaTime());
    }
    private void HandleGravity(ref Vector2 velocity, double delta)
    {
        velocity.Y += Gravity * (float)delta;
    }
    private void HandleSlam(ref Vector2 velocity)
    {
        if (Input.IsActionJustPressed("slam") && !IsOnFloor() && _slamCooldownTimer <= 0)
        {
            velocity.Y = SlamVelocity;
            _slamCooldownTimer = SlamCooldown;

            // Record the slam velocity into our buffer immediately
            _velocityBufferY = SlamVelocity;
            _bufferDecayTimer = BufferWindow;
        }
    }
}