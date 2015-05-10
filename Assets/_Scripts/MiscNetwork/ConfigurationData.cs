using UnityEngine;
using System.Collections;

public class ConfigurationData
{
	public const string VERSION 	= "3.0.2";
	
	public static bool Logged 		= false;
	public static bool Offline 		= false;
	public static bool InGame 		= false;
	public static bool multiPass 	= false;
	public static bool addPass 		= false;
	
	public static Model.Player player;
	public static Model.Battle battle;
}