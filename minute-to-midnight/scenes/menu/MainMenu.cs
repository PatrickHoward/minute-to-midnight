using Godot;
using System;
using Godot.Collections;

public class MainMenu : Control
{
	private AnimationPlayer _animationPlayer;
	private AudioStreamPlayer _music;
	private AudioStreamPlayer2D _player;
	private AudioStreamPlayer _ambience;
	private AudioStreamPlayer2D _bell;
	
	public override void _Ready()
	{
		_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		_music = GetNode<AudioStreamPlayer>("MusicPlayer");
		_player = GetNode<AudioStreamPlayer2D>("World/Player/AudioStreamPlayer2D");
		_ambience = GetNode<AudioStreamPlayer>("Ambience");
		_bell = GetNode<AudioStreamPlayer2D>("ChurchBell");
		
		var audio = SettingsData.Settings["audio"] as Dictionary;
		_music.VolumeDb = (float)audio["music"];
		_bell.VolumeDb = (float)audio["master"];
		_player.VolumeDb = (float)audio["master"];
		_ambience.VolumeDb = (float)audio["master"];
	}

	public void Quit()
	{
		GetTree().Quit();
	}

	public void StartGame()
	{
		GetTree().ChangeScene("res://scenes/levels/Level1/Level1.tscn");
	}
	
	public void _on_Start_Game_Trigger_body_entered(Node body)
	{
		if (body.Name == "Player")
		{
			var music = GetNodeOrNull<AudioStreamPlayer>("AudioStreamPlayer");
			music?.Stop();
			
			_animationPlayer.Play("Fade_Into_Game");
		}
	}
	
	public void _on_Exit_Game_Trigger_body_entered(Node body)
	{
		if (body.Name == "Player")
		{
			var music = GetNodeOrNull<AudioStreamPlayer>("AudioStreamPlayer");
			music?.Stop();
			_animationPlayer.Play("Fade_Exit");
		}
	}
	
	private void _on_PauseMenu_visibility_changed()
	{
		var audio = SettingsData.Settings["audio"] as Dictionary;
		_music.VolumeDb = (float)audio["music"];
		_bell.VolumeDb = (float)audio["master"];
		_player.VolumeDb = (float)audio["master"];
		_ambience.VolumeDb = (float)audio["master"];
	}
}

