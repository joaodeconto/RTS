using UnityEngine;
using System.Collections;

public class InternalMainMenu : MonoBehaviour
{
	[System.Serializable]
	public class Menu
	{
		public string name;
		public GameObject goMenu;
	}

	public Menu[] Menus;
	protected Menu GetMenu(string name)
	{
		name = name.ToLower();
		foreach (Menu m in Menus)
		{
			if (m.name.ToLower().Equals(name))
				return m;
		}
		return null;
	}

	public void Init ()
	{

	}
}
