using Godot;

public class GameOver : Control
{
	private AnimationPlayer _animationPlayer; 
	
	public void _on_GameOverMenu_tree_entered()
	{
		_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		_animationPlayer.Play("Show_Game_Over");
	}

	public void _on_ContinueButton_pressed()
	{
		GetTree().ReloadCurrentScene();
	}

	public void _on_MainMenuButton_pressed()
	{
		GetTree().ChangeScene("res://scenes/menu/MainMenu.tscn");
	}
}
