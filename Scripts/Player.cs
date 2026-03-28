using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export]public float Speed = 300.0f;
    [Export]public float JumpVelocity = -400.0f;

    [Export]public int max_jumps = 1;
    [Export]public float air_jump_velocity = -300.0f;
    [Export] public float gravity = 4500.0f;

    private int jumps;
    private Vector2 velocity = Vector2.Zero;
    private float direction;

    public override void _PhysicsProcess(double delta)
    {
        direction = Input.GetAxis("move_left", "move_right");
        Velocity = new Vector2(direction * Speed, Velocity.Y);

        Velocity = new Vector2(Velocity.X, Velocity.Y + gravity * (float)delta);

        // Jumping
        if (IsOnFloor())
        {
            jumps = 0;
            if (Input.IsActionJustPressed("jump"))
            {
                Velocity = new Vector2(Velocity.X, JumpVelocity);
            }
        }
        else
        {
            if (Input.IsActionJustPressed("jump") && jumps < max_jumps)
            {
                Velocity = new Vector2(Velocity.X, air_jump_velocity);
                jumps++;
            }
        }

        MoveAndSlide();
    }
}
