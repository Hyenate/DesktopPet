extends Node2D

func _ready():
	var os_name = OS.get_name()
	print("Running on:", os_name)
	
	var pet_res = load("res://scenes/pet.tscn")
	var pet = pet_res.instantiate()
	pet.name = "Pet"
	pet.position = get_viewport_rect().size / 2

	match os_name:
		"Windows":
			pet.set_script(load("res://src/Windows/WindowsPet.cs"))
			add_child(pet);
			
			var overlay_res = load("res://src/Windows/TransparentOverlay.tscn")
			var overlay = overlay_res.instantiate()
			add_child(overlay);

			var throwable_res = Node2D.new()
			var throwable = throwable_res.instantiate()
			throwable.set_script(load("res://src/Windows/ThrowableBehavior.cs"))
			add_child(throwable)
		"Linux":
			pet.set_script(load("res://src/Linux/LinuxPet.cs"))
			add_child(pet);
		"macOS":
			print("macOS-specific not implemented")
			pet.set_script(load("res://src/Pet.cs"))
			add_child(pet);
		_:
			push_error("Failed to Initialize OS: " + os_name)
			pass

	# Load and instantiate the scene
