using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable ()]
public class Achieve
{
	// activation rules
	public const string ACTIVE_IF_GREATER_THAN = ">";
	public const string ACTIVE_IF_LESS_THAN    = "<";
	public const string ACTIVE_IF_EQUALS_TO    = "==";

	private Dictionary<string, AchievementProperty> m_Properties;
	private Dictionary<string, Achievement> m_Achievements;

	public void DefineProperty (string name, int initialValue, string activationMode, int value)
	{
		AchievementProperty ap = new AchievementProperty(name,
														 initialValue,
														 activationMode,
														 value);

		if (this.m_Properties.ContainsKey (name))
			this.m_Properties[name] = ap;
		else
			this.m_Properties.Add (name, ap);
	}

	public void DefineAchievement (string name, List<string> relatedProps)
	{
		Achievement a = new Achievement (name, relatedProps);

		if (m_Achievements.ContainsKey (name))
			this.m_Achievements[name] = a;
		else
			this.m_Achievements.Add(name, a);
	}

	public int GetValue (string propertyName)
	{
		return m_Properties[propertyName].Value;
	}

	private void SetValue (string propertyName, int value)
	{
		m_Properties[propertyName].Value = value;
	}

	public void AddValue (List<string> properties, int value)
	{
		for (int i = 0; i != properties.Count; i++)
		{
			string propertyName = properties[i];
			SetValue(propertyName, GetValue(propertyName) + value);
		}
	}

	public List<Achievement> UnlockedAchievements
	{
		get
		{
			List<Achievement> achievements = new List<Achievement>();

			foreach (KeyValuePair<string,Achievement> de in m_Achievements)
			{
				Achievement achievement = de.Value;

				if (achievement.Unlocked)
					continue;

				int nActivedProperties = 0;

				for (int i = 0; i != achievement.Properties.Count; ++i)
				{
					AchievementProperty property = m_Properties[achievement.Properties[i]];

					if (property.IsActive)
					{
						++nActivedProperties;
					}
				}

				if (nActivedProperties == achievement.Properties.Count)
				{
					achievement.Unlocked = true;
					achievements.Add (achievement);
				}
			}

			return achievements;
		}
	}
}
