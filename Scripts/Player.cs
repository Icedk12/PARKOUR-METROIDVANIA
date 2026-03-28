using Godot;
using System;

public partial class Player : CharacterBody2D
{
    [ExportGroup("Movement")]
    [Export] public float Speed = 800.0f;
    [Export] public float Acceleration = 1500.0f;
    [Export] public float Friction = 3000.0f;
    [Export] public float AirResistance = 1000.0f;

    [ExportGroup("Jump")]
    [Export] public float JumpVelocity = -1000.0f;
    [Export] public float AirJumpVelocity = -800.0f;
    [Export] public int MaxAirJumps = 1;
    [Export] public float Gravity = 2500.0f;

    [ExportGroup("Slide")]
    [Export] public float SlideBoost = 2.0f;
    [Export] public float SlideCooldown = 0.5f; // Seconds between slides

    // Privates
    private int _airJumpsReached = 0;
    private bool isSliding = false;
    private float _slideCooldownTimer = 0.0f;

    // Nodes
    private Label _label;
    private Sprite2D _sprite;

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

        // --- 1. Timers ---
        if (_slideCooldownTimer > 0)
            _slideCooldownTimer -= (float)delta;

        // --- 2. Gravity & Jump Reset ---
        if (!IsOnFloor())
        {
            velocity.Y += Gravity * (float)delta;
        }
        else
        {
            _airJumpsReached = 0;
        }

        // --- 3. Flipping the Sprite ---
        if (direction > 0) _sprite.FlipH = false;
        else if (direction < 0) _sprite.FlipH = true;

        // --- 4. Jumping ---
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

        // --- 5. Sliding Logic ---
        if (Input.IsActionJustPressed("slide") && IsOnFloor() && _slideCooldownTimer <= 0)
        {
            isSliding = true;
            _slideCooldownTimer = SlideCooldown;

            float slideDir = (Math.Abs(velocity.X) > 0.1f) ? Math.Sign(velocity.X) : direction;
            if (slideDir != 0) velocity.X = Speed * slideDir * SlideBoost;
        }

        if (Input.IsActionJustReleased("slide"))
        {
            isSliding = false;
        }

        // --- 6. Horizontal Movement ---
        if (isSliding)
        {
            velocity.X = Mathf.MoveToward(velocity.X, 0, Friction * 0.5f * (float)delta);
            if (Math.Abs(velocity.X) < Speed * 0.5f) isSliding = false;
        }
        else if (direction != 0)
        {
            velocity.X = Mathf.MoveToward(velocity.X, direction * Speed, Acceleration * (float)delta);
        }
        else
        {
            float currentFriction = IsOnFloor() ? Friction : AirResistance;
            velocity.X = Mathf.MoveToward(velocity.X, 0, currentFriction * (float)delta);
        }

        Velocity = velocity;
        MoveAndSlide();
    }
}