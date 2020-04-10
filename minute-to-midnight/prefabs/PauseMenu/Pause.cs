using Godot;
using System;

public class Pause : Control
{
	private bool _pauseState;
	private Sprite _menu;
	private Sprite _opMenu;
	private ColorRect _screen;
	private float _masterVolume;
	private float _monsterVolume;
	private float _musicVolume;
	
	public override void _Ready()
	{
		Visible = false;
		
		_menu = GetNode<Sprite>("Menu/MenuBackground");
		_menu.Visible = false;
		
		_opMenu = GetNode<Sprite>("OptionMenu/OptionBackground");
		_opMenu.Visible = false;
		
		_screen = GetNode<ColorRect>("PauseMenu/ScreenOverlay");
		_screen.Visible = false;
	}
	
	public override void _Input(InputEvent e)
	{
		if(e.IsActionPressed("Pause") )
		{
			_pauseState = !GetTree().Paused;
			GD.Print("Pause State: " + _pauseState);
			GetTree().Paused = _pauseState;
			Visible = _pauseState;
			_screen.Visible = _pauseState;
			_menu.Visible = _pauseState;
			
			if(_opMenu.Visible == true)
				_opMenu.Visible = false;
		}
	}

	private void _on_Save_button_down()
	{

	}
	
	private void _on_Load_button_down()
	{

	}
	
	
	private void _on_Options_button_down()
	{
		_opMenu.Visible = true;
	}
	
	private void _on_Cancel_button_down()
	{
		_opMenu.Visible = false;
		_masterVolume = SettingsData.MasterVolume;
		_musicVolume = SettingsData.MusicVolume;
		_monsterVolume = SettingsData.MonsterVolume;
	}
	
	private void _on_Apply_button_down()
	{
		_opMenu.Visible = false;
		SettingsData.MasterVolume = _masterVolume;
		SettingsData.MusicVolume = _musicVolume; 
		SettingsData.MonsterVolume = _monsterVolume;
		
		foreach(AudioStreamPlayer node in GetTree().GetNodesInGroup("Ao-Music"))
			node.VolumeDb = _musicVolume;
			
		foreach(AudioStreamPlayer2D node in GetTree().GetNodesInGroup("Ao-Monsters"))
			node.VolumeDb = _monsterVolume;
			
		foreach(AudioStreamPlayer2D node in GetTree().GetNodesInGroup("Ao-Master"))
			node.VolumeDb = _masterVolume;
	}
	
	private void _on_Quit_button_down()
	{
		GetTree().Quit();
	}
	
	private void _on_MasterScroll_value_changed(float value)
	{
		_masterVolume = value;
	}
	
	private void _on_MusicScroll2_value_changed(float value)
	{
		_musicVolume = value;
	}
	
	private void _on_MonsterScroll3_value_changed(float value)
	{
		_monsterVolume = value;
	}
}
