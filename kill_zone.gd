extends Area2D

func _ready():
	body_entered.connect(_on_body_entered)

func _on_body_entered(body):
	if body.name == "testplayercandelete":
		get_tree().reload_current_scene()
