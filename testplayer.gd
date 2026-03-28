extends CharacterBody2D

const SPEED = 400.0
const JUMP_VELOCITY = -500.0
const GRAVITY = 1200.0

func _physics_process(delta):
	# Add gravity
	if not is_on_floor():
		velocity.y += GRAVITY * delta

	# Jump
	if Input.is_action_just_pressed("ui_accept") and is_on_floor():
		velocity.y = JUMP_VELOCITY

	# Left and right movement
	var direction = Input.get_axis("ui_left", "ui_right")
	if direction != 0:
		velocity.x = direction * SPEED
	else:
		velocity.x = move_toward(velocity.x, 0, SPEED)

	move_and_slide()
