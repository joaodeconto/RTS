using UnityEngine;
using System.Collections;

public abstract class IStats : Photon.MonoBehaviour
{
	[System.Serializable]
	public class RendererTeamColor
	{
		public Material materialToApplyColor;
		
		public void SetColorInMaterial (Transform transform, int teamID)
		{
			MeshRenderer[] renderers = transform.GetComponentsInChildren<MeshRenderer>();
			if (renderers.Length != 0)
			{
				for (int i = 0; i != renderers.Length; i++)
				{
					for (int k = 0; k != renderers[i].materials.Length; k++)
					{
						if (renderers[i].materials[k].name.Equals (materialToApplyColor.name + " (Instance)"))
						{
							renderers[i].materials[k].color = Visiorama.ComponentGetter.Get<GameplayManager>().GetColorTeam (teamID);
						}
					}
				}
			}
			
			SkinnedMeshRenderer[] skinnedMeshRenderers = transform.GetComponentsInChildren<SkinnedMeshRenderer>();
			if (skinnedMeshRenderers.Length != 0)
			{
				for (int i = 0; i != skinnedMeshRenderers.Length; i++)
				{
					for (int k = 0; k != skinnedMeshRenderers[i].materials.Length; k++) {
						if (skinnedMeshRenderers[i].materials[k].name.Equals (materialToApplyColor.name + " (Instance)"))
						{
							skinnedMeshRenderers[i].materials[k].color = Visiorama.ComponentGetter.Get<GameplayManager>().GetColorTeam (teamID);
						}
					}
				}
			}
		}
	}
	
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

	public GameObject model;
	public abstract void SetVisible(bool visible);
	public abstract bool IsVisible { get; }
}
