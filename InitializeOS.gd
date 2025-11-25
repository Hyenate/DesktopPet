extends Node2D

func _ready():
	var os_name = OS.get_name()
	print("Running on:", os_name)
	
	var pet = load("res://pet.tscn")
	var pet_instance = pet.instantiate()
	pet_instance.name = "Pet"

	match os_name:
		"Windows":
			pet_instance.set_script(load("res://Windows/WindowsPet.cs"))
			add_child(pet_instance);
			var overlay_res = load("res://Windows Pet/TransparentOverlay.tscn")
			var overlay = overlay_res.instantiate()
			add_child(overlay);
		"Linux":
			pet_instance.set_script(load("res://Linux/LinuxPet.cs"))
			add_child(pet_instance);
		"macOS":
			print("macOS-specific not implemented")
			push_error("Failed to Initialize OS: Incompatible OS")
		_:
			push_error("Failed to Initialize OS: " + os_name)
			pass

	# Load and instantiate the scene
		
