extends Node

func _process(delta):
	if get_node_or_null("/root/GodotTestRunner") == null:
		var runner = load("res://addons/GodotXUnit/GodotTestRunner.cs").new()
		runner.name = "GodotTestRunner"
		get_tree().root.add_child(runner)
	set_process(false)
