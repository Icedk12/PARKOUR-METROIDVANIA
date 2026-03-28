extends Node2D

var player_nearby = false

func _ready():
	$InteractZone.body_entered.connect(_on_body_entered)
	$InteractZone.body_exited.connect(_on_body_exited)

func _on_body_entered(body):
	if body.name == "testplayer":
		player_nearby = true
		$PromptLabel.visible = true

func _on_body_exited(body):
	if body.name == "testplayer":
		player_nearby = false
		$PromptLabel.visible = false
		$SignText.visible = false

func _process(_delta):
	if player_nearby and Input.is_action_just_pressed("Interact"):
		$SignText.visible = !$SignText.visible
