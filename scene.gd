extends Node2D

func _ready():
	var os_name = OS.get_name()
	print("Running on:", os_name)
	
	var pet_scene_path = "res://Linux Pet (Default)/pet.tscn"

	match os_name:
		"Windows":
			pet_scene_path = "res://Windows Pet/WindowsPet.tscn"
		"Linux":
			pet_scene_path = "res://Linux Pet (Default)/pet.tscn"
		"macOS":
			print("macOS-specific not implemented, using default for Linux")
		"Android":
			print("macOS-specific not implemented, using default for Linux")
		"iOS":
			print("macOS-specific not implemented, using default for Linux")
		_:
			# Default already set
			pass

	# Load and instantiate the scene
	if pet_scene_path != "":
		var scene_res = load(pet_scene_path)
		if scene_res:
			var instance = scene_res.instantiate()
			instance.name = "Pet"
			add_child(instance);
			
			var overlay_res = load("res://Windows Pet/TransparentOverlay.tscn")
			var overlay = overlay_res.instantiate()
			add_child(overlay);
		else:
			push_error("Failed to load scene: " + pet_scene_path)
