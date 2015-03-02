using UnityEngine;
using Visiorama;
using System.Collections;
using System;

public class UpgradeAttribute : Upgrade
{
	public int _bonus;
	public AttributeBonus _attribute;
	public bool allUnits;
	public bool allFactories;
	public string _category;
	public string _subCategory;
	protected StatsController statsController;

	[System.Serializable]
	public enum AttributeBonus
	{
		defense,
		force,
		speed,
		sight
	}

	public void Init()
	{
		statsController = ComponentGetter.Get<StatsController>();		

		switch(_attribute)														//Faz o swhitch entre o tipo de atributo
		{
			case AttributeBonus.defense:
			{
				foreach (IStats stat in statsController.myStats)
				{
					if(allUnits)												//Todos do tipo unidade
					{
						if (stat.GetType() == typeof(Unit))
						{
							Unit u = stat as Unit;							
							u.bonusDefense += _bonus;							
						}
					}

					if(_subCategory != null)									// chama por subcategoria
					{
						Unit u = stat as Unit;		
						if(_subCategory == u.subCategory)u.bonusDefense += _bonus;
					}

					else if (stat.category == _category)
					{
						Unit u = stat as Unit;
						u.bonusDefense += _bonus;
					}
				}
				
				foreach (IStats stat in statsController.myStats)				//Factories sao chamadas apenas no defence(por agora)
				{
				if(allFactories)												//Todos do tipo factory
					{
						if (stat.GetType() == typeof(FactoryBase))
						{
							FactoryBase fb = stat as FactoryBase;
							fb.bonusDefense += _bonus;
						}
						else if (stat.category == _category)
						{
							FactoryBase fb = stat as FactoryBase;
							fb.bonusDefense += _bonus;
						}
					}
				}
			}
				break;

			case AttributeBonus.force:
			{
				foreach (IStats stat in statsController.myStats)
				{
					if(allUnits)
					{
						if (stat.GetType() == typeof(Unit))
						{
							Unit u = stat as Unit;
							u.bonusForce += _bonus;
						}
					}
					if(_subCategory != null)
						{
							Unit u = stat as Unit;		
							if(_subCategory == u.subCategory)u.bonusForce += _bonus;
						}
					else if (stat.category == _category)
					{
						Unit u = stat as Unit;
						u.bonusForce += _bonus;
					}
				}
			}
				break;

			case AttributeBonus.speed:
			{
				foreach (IStats stat in statsController.myStats)
				{
					if(allUnits)
					{
						if (stat.GetType() == typeof(Unit))
						{
							Unit u = stat as Unit;
							u.bonusSpeed += _bonus;
						}
					}
						if(_subCategory != null)
						{
							Unit u = stat as Unit;		
							if(_subCategory == u.subCategory)u.bonusSpeed += _bonus;
						}

					else if (stat.category == _category)
					{
						Unit u = stat as Unit;
						u.bonusSpeed += _bonus;
					}
				}
			}
				break;

			case AttributeBonus.sight:
			{
						foreach (IStats stat in statsController.myStats)
				{
					if(allUnits)
					{
						if (stat.GetType() == typeof(Unit))
						{
							Unit u = stat as Unit;
							u.bonusSight += _bonus;
						}
					}

					if(_subCategory != null)
					{
						Unit u = stat as Unit;		
						if(_subCategory == u.subCategory)u.bonusSight += _bonus;
					}

					else if (stat.category == _category)
					{
						Unit u = stat as Unit;
						u.bonusSight += _bonus;
					}
				}
			}
				break;
		}

		if(allUnits)
		{ 
			foreach (Unit u in techTreeController.prefabUnit)
			{
				techTreeController.AttributeModifier(u.category,("bonus"+_attribute.ToString()),_bonus);
			}
		}
		if(allFactories)
		{
			foreach (FactoryBase fb in techTreeController.prefabFactory)
			{
				techTreeController.AttributeModifier(fb.category,("bonus"+_attribute.ToString()),_bonus);
			}
		}

		else
		{
			techTreeController.AttributeModifier(_category,("bonus"+_attribute.ToString()),_bonus);
		}
	}
}
