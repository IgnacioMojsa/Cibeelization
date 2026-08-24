extends Control



func _on_play_button_pressed() -> void:
	print("El boton play ha sido presionado");
	get_tree().change_scene_to_file("res://Scenes/escenaPrueba.tscn")

func _on_exit_button_pressed() -> void:
	get_tree().quit()
