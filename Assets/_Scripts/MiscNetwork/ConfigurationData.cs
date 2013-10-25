using UnityEngine;
using System.Collections;

public class ConfigurationData
{
	public const string VERSION = "0.1";
	
	public static bool Logged = false;
	public static bool InGame = false;
	
	public static Model.Player player;
	public static Model.Battle battle;
}