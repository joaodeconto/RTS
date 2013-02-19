using UnityEngine;
using System.Collections;

public class InputManager : MonoBehaviour {

	/*enum InputKeyboard
	{
		A,
		Alpha0,
		Alpha1,
		Alpha2,
		Alpha3,
		Alpha4,
		Alpha5,
		Alpha6,
		Alpha7,
		Alpha8,
		Alpha9,
		Alt,
		Ctrl,
		D,
		LeftAlt,
		LeftCtrl,
		LeftShift,
		RightAlt,
		RightCtrl,
		RightShift,
		S,
		Shift,
		W,
	}
	
	enum InputJoystick
	{
		
		Button0,
		Button1,
		Button10,
		Button11,
		Button12,
		Button13,
		Button14,
		Button15,
		Button16,
		Button17,
		Button18,
		Button19,
		Button2,
		Button3,
		Button4,
		Button5,
		Button6,
		Button7,
		Button8,
		Button9,
	}
	
	public class ControlInput
	{
		public string name;
		public InputKeyboard keyboard;
		public InputJoystick joystick;
	}
	
	public ControlInput[] inputs;
	
	void Input (string key)
	{
		key = key.ToLower();
		
		foreach (ControlInput input in inputs)
		{
			if (key.Equals(input.name.ToLower()))
			{
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_DASHBOARD_WIDGET || UNITY_STANDALONE_LINUX || UNITY_WEBPLAYER || UNITY_NACL || UNITY_FLASH
				CheckKeyboardInput (input.keyboard);
#elif UNITY_PS3 || UNITY_XBOX360 || UNITY_WII
				CheckJoystickInput (input.joystick);
#endif
			}
		}
	}
	
	void CheckKeyboardInput (InputKeyboard key)
	{
	}
	
	void CheckJoystickInput (InputJoystick joy)
	{
	}*/
}
