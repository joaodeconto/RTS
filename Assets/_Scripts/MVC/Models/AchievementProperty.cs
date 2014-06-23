using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[System.Serializable ()]
public class AchievementProperty
{
	private string m_Name;
	private int m_Value;
	private string m_Activation;
	private int m_ActivationValue;
	private int m_InitialValue;

	public AchievementProperty (string name, int initialValue, string activation, int activationValue)
	{
		this.m_Name            = name;
		this.m_Activation      = activation;
		this.m_ActivationValue = activationValue;
		this.m_InitialValue    = initialValue;
	}

	public bool IsActive
	{
		get
		{
			bool isActive = false;

			switch (m_Activation)
			{
				case Achieve.ACTIVE_IF_GREATER_THAN: isActive = (m_Value > m_ActivationValue); break;
				case Achieve.ACTIVE_IF_LESS_THAN: isActive = (m_Value < m_ActivationValue); break;
				case Achieve.ACTIVE_IF_EQUALS_TO: isActive = (m_Value == m_ActivationValue); break;
			}

			return isActive;
		}
	}

	public int Value
	{
		get { return m_Value; }
		set { m_Value = value; }
	}
}
