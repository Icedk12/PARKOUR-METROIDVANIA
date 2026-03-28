extends Node2D

func _ready():
	$WalkTrigger.body_entered.connect(_on_walk_trigger)
	$JumpTrigger.body_entered.connect(_on_jump_trigger)

func _on_walk_trigger(body):
	if body.name == "testplayer":
		show_hint("Use A and D to walk")

func _on_jump_trigger(body):
	if body.name == "testplayer":
		show_hint("Press Space to jump")

func show_hint(text):
	$HUD/HintLabel.text = text
	$HUD/HintLabel.visible = true
	await get_tree().create_timer(3.0).timeout
	$HUD/HintLabel.visible = false
