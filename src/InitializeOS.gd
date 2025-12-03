extends Node2D

func load_OS_settings(useOverlay, pet):
	DisplayServer.window_set_flag(DisplayServer.WINDOW_FLAG_ALWAYS_ON_TOP, true)
	var os_name = OS.get_name()
	print("Running on:", os_name)
	
	if os_name == "Windows" && useOverlay:
		var overlay = Node.new()
		var overlay_script = load("res://src/Windows/TransparentOverlay.cs")
		overlay.set_script(overlay_script)
		pet.add_child(overlay)
	else:
		DisplayServer.window_set_mode(DisplayServer.WINDOW_MODE_MAXIMIZED)
		DisplayServer.window_set_flag(DisplayServer.WINDOW_FLAG_BORDERLESS, true)
