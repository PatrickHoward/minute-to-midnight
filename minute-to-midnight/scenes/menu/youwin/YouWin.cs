using Godot;

public class YouWin : Control
{
    private AnimationPlayer _animationPlayer;

    public void _on_GameOverMenu_tree_entered()
    {
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _animationPlayer.Play("Show_Game_Over");
    }

    public void _on_Exit_Game_pressed()
    {
        GetTree().Quit();
    }

    public void _on_MainMenuButton_pressed()
    {
        GetTree().ChangeScene("res://scenes/menu/MainMenu.tscn");
    }
}
