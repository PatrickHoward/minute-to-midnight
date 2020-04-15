using Godot;
using System;
using Godot.Collections;

public class ControlButton : Label
{
	private Button _right;
	private int _rValue;
	private InputEventKey _rKey = new InputEventKey();
	
	private Button _left;
	private int _lValue;
	private InputEventKey _lKey = new InputEventKey();
	
	private Button _jump;
	private int _jValue;
	private InputEventKey _jKey = new InputEventKey();
	
	private Button _action;
	private int _aValue;
	private InputEventKey _aKey = new InputEventKey();
	
	private Button _pause;
	private int _pValue;
	private InputEventKey _pKey = new InputEventKey();
	
	private Button _okay;
	private int _oValue;
	private InputEventKey _oKey = new InputEventKey();
	
	private String _prompt = "Press Any Key";
	
	public override void _Ready()
	{
		EmptyInputMap();
		var keys = SettingsData.Settings["keys"] as Dictionary;
		
		_right = GetNode<Button>("RightControl");
		_left = GetNode<Button>("LeftControl");
		_jump = GetNode<Button>("JumpControl");
		_action = GetNode<Button>("AttackControl");
		_pause = GetNode<Button>("PauseControl");
		_okay = GetNode<Button>("OkayControl");
		
		RetrieveControls();
		SetControls();
		ApplyControls();
	}
	
	public override void _Input(InputEvent e)
	{
		if(e is InputEventKey eventKey)
		{
			if(_right.Pressed)
			{
				if(DoubleCheck(OS.FindScancodeFromString(e.AsText())))
				{
					_right.Text = e.AsText();
					_rKey = eventKey;
					_right.Pressed = false;
					return;
				}
				
				_right.Text = OS.GetScancodeString((uint)_rValue);
				_right.Pressed = false;
				return;
			}
			else if(_left.Pressed)
			{
				if(DoubleCheck(OS.FindScancodeFromString(e.AsText())))
				{
					_left.Text = e.AsText();
					_lKey = eventKey;
					_left.Pressed = false;
					return;
				}
				
				_left.Text = OS.GetScancodeString((uint)_lValue);
				_left.Pressed = false;
				return;
			}
			else if(_jump.Pressed)
			{
				if(DoubleCheck(OS.FindScancodeFromString(e.AsText())))
				{
					_jump.Text = e.AsText();
					_jKey = eventKey;
					_jump.Pressed = false;
					return;
				}
				
				_jump.Text = OS.GetScancodeString((uint)_jValue);
				_jump.Pressed = false;
				return;
			}
			else if(_action.Pressed)
			{
				if(DoubleCheck(OS.FindScancodeFromString(e.AsText())))
				{
					_action.Text = e.AsText();
					_aKey = eventKey;
					_action.Pressed = false;
					return;
				}
				
				_action.Text = OS.GetScancodeString((uint)_aValue);
				_action.Pressed = false;
				return;
			}
			else if(_pause.Pressed)
			{
				if(DoubleCheck(OS.FindScancodeFromString(e.AsText())))
				{
					_pause.Text = e.AsText();
					_pKey = eventKey;
					_pause.Pressed = false;
					return;
				}
				
				_pause.Text = OS.GetScancodeString((uint)_pValue);
				_pause.Pressed = false;
				return;
			}
			else if(_okay.Pressed)
			{
				if(DoubleCheck(OS.FindScancodeFromString(e.AsText())))
				{
					_okay.Text = e.AsText();
					_oKey = eventKey;
					_okay.Pressed = false;
					return;
				}
				
				_okay.Text = OS.GetScancodeString((uint)_oValue);
				_okay.Pressed = false;
				return;
			}
		}
		return;
	}
	
	private bool DoubleCheck(int button)
	{
		if(button == OS.FindScancodeFromString(_right.Text))
		{
			return false;
		}
		else if(button == OS.FindScancodeFromString(_left.Text))
		{
			return false;
		}
		else if(button == OS.FindScancodeFromString(_jump.Text))
		{
			return false;
		}
		else if(button == OS.FindScancodeFromString(_action.Text))
		{
			return false;
		}
		else if(button == OS.FindScancodeFromString(_pause.Text))
		{
			return false;
		}
		else if(button == OS.FindScancodeFromString(_okay.Text))
		{
			return false;
		}
		return true;
	}
	
	private bool EmptyCheck()
	{
		if(_right.Text == _prompt)
		{
			return true;
		}
		else if(_left.Text == _prompt)
		{
			return true;
		}
		else if(_jump.Text == _prompt)
		{
			return true;
		}
		else if(_action.Text == _prompt)
		{
			return true;
		}
		else if(_pause.Text == _prompt)
		{
			return true;
		}
		else if(_okay.Text == _prompt)
		{
			return true;
		}
		
		return false;
	}
	
	private void _on_OkayControl_pressed()
	{
		_right.Pressed = false;
		_left.Pressed = false;
		_jump.Pressed = false;
		_action.Pressed = false;
		_pause.Pressed = false;
		_okay.Text = _prompt;
	}
	
	private void _on_PauseControl_pressed()
	{
		_right.Pressed = false;
		_left.Pressed = false;
		_jump.Pressed = false;
		_action.Pressed = false;
		_okay.Pressed = false;
		_pause.Text = _prompt;
	}
	
	private void _on_AttackControl_pressed()
	{
		_right.Pressed = false;
		_left.Pressed = false;
		_jump.Pressed = false;
		_pause.Pressed = false;
		_okay.Pressed = false;
		_action.Text = _prompt;
	}
	
	private void _on_JumpControl_pressed()
	{
		_right.Pressed = false;
		_left.Pressed = false;
		_action.Pressed = false;
		_pause.Pressed = false;
		_okay.Pressed = false;
		_jump.Text = _prompt;
	}
	
	private void _on_LeftControl_pressed()
	{
		_right.Pressed = false;
		_jump.Pressed = false;
		_action.Pressed = false;
		_pause.Pressed = false;
		_okay.Pressed = false;
		_left.Text = _prompt;
	}
	
	private void _on_RightControl_pressed()
	{
		_left.Pressed = false;
		_jump.Pressed = false;
		_action.Pressed = false;
		_pause.Pressed = false;
		_okay.Pressed = false;
		_right.Text = _prompt;
	}
	
	private void _on_Apply_button_down()
	{
		if(!EmptyCheck())
		{
			EmptyInputMap();
			
			var keys = SettingsData.Settings["keys"] as Dictionary;
			keys["Move-Left"] = OS.FindScancodeFromString(_lKey.AsText());
			keys["Move-Right"] = OS.FindScancodeFromString(_rKey.AsText());
			keys["Jump"] = OS.FindScancodeFromString(_jKey.AsText());
			keys["Action"] = OS.FindScancodeFromString(_aKey.AsText());
			keys["Pause"] = OS.FindScancodeFromString(_pKey.AsText());
			keys["Okay"] = OS.FindScancodeFromString(_oKey.AsText());
			SettingsData.Settings["keys"] = keys;
			
			ApplyControls();
			SettingsData.Save();
			return;
		}
		GetNode<Popup>("Prompt/Popup").Visible = true;
		RetrieveControls();
		MakeDefault();
		Untoggler();
		return;
	}
	
	private void _on_Cancel_button_down()
	{
		MakeDefault();	
		Untoggler();
	}
	
	private void _on_Okay_button_down()
	{
		GetNode<Popup>("Prompt/Popup").Visible = false;
	}
	
	private void Untoggler()
	{
		_left.Pressed = false;
		_jump.Pressed = false;
		_action.Pressed = false;
		_pause.Pressed = false;
		_okay.Pressed = false;
		_right.Pressed = false;
	}
	
	private void MakeDefault()
	{
		_right.Text = OS.GetScancodeString((uint)_rValue);
		_left.Text = OS.GetScancodeString((uint)_lValue);
		_jump.Text = OS.GetScancodeString((uint)_jValue);
		_action.Text = OS.GetScancodeString((uint)_aValue);
		_pause.Text = OS.GetScancodeString((uint)_pValue);
		_okay.Text = OS.GetScancodeString((uint)_oValue);
	}
	
	private void EmptyInputMap()
	{
		InputMap.ActionEraseEvents("Move-Left");
		InputMap.ActionEraseEvents("Move-Right");
		InputMap.ActionEraseEvents("Jump");
		InputMap.ActionEraseEvents("Action");
		InputMap.ActionEraseEvents("Pause");
		InputMap.ActionEraseEvents("Okay");
	}
	
	private void ApplyControls()
	{
		InputMap.ActionAddEvent("Move-Left", _lKey);
		InputMap.ActionAddEvent("Move-Right", _rKey);
		InputMap.ActionAddEvent("Jump", _jKey);
		InputMap.ActionAddEvent("Action", _aKey);
		InputMap.ActionAddEvent("Pause", _pKey);
		InputMap.ActionAddEvent("Okay", _oKey);
	}
	
	private void RetrieveControls()
	{
		var keys = SettingsData.Settings["keys"] as Dictionary;
		
		_rValue = (int)keys["Move-Right"];
		_lValue = (int)keys["Move-Left"];
		_jValue = (int)keys["Jump"];
		_aValue = (int)keys["Action"];
		_pValue = (int)keys["Pause"];
		_oValue = (int)keys["Okay"];
	}
	
	private void SetControls()
	{
		_rKey.Scancode = (uint)_rValue;
		_right.Text = _rKey.AsText();
		
		_lKey.Scancode = (uint)_lValue;
		_left.Text = _lKey.AsText();
		
		_jKey.Scancode = (uint)_jValue;
		_jump.Text = _jKey.AsText();
		
		_aKey.Scancode = (uint)_aValue;
		_action.Text = _aKey.AsText();
		
		_pKey.Scancode = (uint)_pValue;
		_pause.Text = _pKey.AsText();
		
		_oKey.Scancode = (uint)_oValue;
		_okay.Text = _oKey.AsText();
	}
	
	private void _on_Default_button_down()
	{
		SettingsData.Default();
		EmptyInputMap();
		Untoggler();
		RetrieveControls();
		SetControls();
		ApplyControls();
	}
}

