extends Node2D

func _ready():
	var os_name = OS.get_name()
	print("Running on:", os_name)
	var pet_res

	match os_name:
		"Windows":
			pet_res = load("res://src/Windows/WindowsPet.tscn")
		"Linux":
			pet_res = load("res://src/Linux/LinuxPet.tscn")
		"macOS":
			# macOS and Linux currently use same Godot native implementation
			pet_res = load("res://src/Linux/LinuxPet.tscn")
		_:
			push_error("Failed to Initialize OS: " + os_name)
			return

	var pet = pet_res.instantiate()
	pet.name = "Pet"
	add_child(pet);
