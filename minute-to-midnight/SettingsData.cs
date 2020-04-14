using Godot;
using System;
using Godot.Collections;

public class SettingsData : Node
{
	static public Dictionary Settings = new Dictionary();
	static public String SavePath = "res://Settings.cfg";
	static private ConfigFile _conf = new ConfigFile();
	static public bool FileFound = false;
	
	public override void _Ready()
	{
		Dictionary Audio = new Dictionary();
		Audio.Add("master", 1f);
		Audio.Add("music", 1f);
		Audio.Add("monster", 1f);
		
		Dictionary Keys = new Dictionary();
		Keys.Add("Move-Left", 65);
		Keys.Add("Move-Right", 68);
		Keys.Add("Jump",32);
		Keys.Add("Action",16777237);
		Keys.Add("Pause", 80);
		Keys.Add("Okay", 16777221);
		
		Settings.Add("audio", Audio.Duplicate());
		Settings.Add("keys", Keys.Duplicate());
		
		var Err = _conf.Load(SavePath);
		if(Err == Error.FileNotFound)
		{
			GD.Print("Settings File Could Not Be Loaded!");
			return;
		}
		FileFound = true;
		GD.Print("Settings File Was Found!");
		
		Load();
	}
	
	static public void Load()
	{
		var audio = Settings["audio"] as Dictionary;
		var keys = Settings["keys"] as Dictionary;
		
		audio["master"] = _conf.GetValue("audio", "master");
		audio["music"] = _conf.GetValue("audio", "music");
		audio["monster"] = _conf.GetValue("audio", "monster");
		
		keys["Move-Left"] = _conf.GetValue("keys", "Move-Left");
		keys["Move-Right"] = _conf.GetValue("keys", "Move-Right");
		keys["Jump"] = _conf.GetValue("keys", "Jump");
		keys["Action"] = _conf.GetValue("keys", "Action");
		keys["Pause"] = _conf.GetValue("keys", "Pause");
		keys["Okay"] = _conf.GetValue("keys", "Okay");
		
	}
	
	static public void Save()
	{
		var audio = Settings["audio"] as Dictionary;
		var keys = Settings["keys"] as Dictionary;
		_conf.SetValue("audio", "master", audio["master"]);
		_conf.SetValue("audio", "music", audio["music"]);
		_conf.SetValue("audio", "monster", audio["music"]);
		
		_conf.SetValue("keys", "Move-Left", keys["Move-Left"]);
		_conf.SetValue("keys", "Move-Right", keys["Move-Right"]);
		_conf.SetValue("keys", "Jump", keys["Jump"]);
		_conf.SetValue("keys", "Action", keys["Action"]);
		_conf.SetValue("keys", "Pause", keys["Pause"]);
		_conf.SetValue("keys", "Okay", keys["Okay"]);
		_conf.Save(SavePath);
	}
}
