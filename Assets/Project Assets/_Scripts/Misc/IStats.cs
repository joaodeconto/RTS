using UnityEngine;
using System.Collections;

public class IStats : Photon.MonoBehaviour
{
	public int Health { get; protected set; }
	public int MaxHealth = 200;
	public int Defense;

	public int Team;
	public float RangeView;

	public bool Actived { get; protected set; }

	internal int Group = -1;

	public virtual void Init()
	{
		Health = MaxHealth;
	}

	public virtual void ReceiveAttack (int Damage)
	{
		if (Health == -1) return;

		int newDamage = Mathf.Max (0, Damage - Defense);

		Health -= newDamage;
		Health = Mathf.Clamp (Health, 0, MaxHealth);

		if (Health == 0)
		{
			SendMessage ("OnDie", SendMessageOptions.DontRequireReceiver);
			Health = -1;
		}
	}

}
