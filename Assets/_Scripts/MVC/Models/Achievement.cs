using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Achievement : MonoBehaviour
{
	private string m_Name;             // achievement name
	private List<string> m_Properties; // array of related properties
	private bool m_Unlocked;           // achievement is unlocked or not

	public Achievement (string name, List<string> relatedProps)
	{
		this.m_Name       = name;
		this.m_Properties = relatedProps;
		this.m_Unlocked   = false;
	}

	public bool Unlocked
	{
		get { return m_Unlocked; }
		set { m_Unlocked = value; }
	}

	public List<string> Properties
	{
		get { return m_Properties; }
		set { m_Properties = value; }
	}

	public string Name
	{
		get { return m_Name; }
		set { m_Name = value; }
	}
}
