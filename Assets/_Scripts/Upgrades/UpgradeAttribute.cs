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
		sight,
		projectile
	}

	public override void Init()
	{
		base.Init();

		statsController = ComponentGetter.Get<StatsController>();		

		switch(_attribute)														//Faz o swhitch entre o tipo de atributo
		{
			case AttributeBonus.defense:
			{
					if(allUnits)												//Todos do tipo unidade
					{
						foreach (IStats stat in statsController.myStats)
						{
							if (stat is Unit)
							{
								Unit u = stat as Unit;							
								u.bonusDefense += _bonus;							
							}
						}
					}

					if(allFactories)												//Todos do tipo factory
					{
						foreach (IStats stat in statsController.myStats)
						{
							if (stat is FactoryBase)
							{
								FactoryBase fb = stat as FactoryBase;
								fb.bonusDefense += _bonus;
							}
						}						
					}

					else
					{
						foreach (IStats stat in statsController.myStats)
						{
							if (stat is Unit)
							{
								Unit u = stat as Unit;	
								if(_subCategory != null || _subCategory != null)									// chama por subcategoria
								{									
									if(_subCategory == u.subCategory)u.bonusDefense += _bonus;
									else if (u.category == _category)u.bonusDefense += _bonus;
								}
							}
							

							else if(stat is FactoryBase)
							{
								FactoryBase fb = stat as FactoryBase;
								if(_subCategory != null || _subCategory != null)
								{								
									
									if(_subCategory == fb.subCategory)fb.bonusDefense += _bonus;
									else if (fb.category == _category)fb.bonusDefense += _bonus;
								}
							}
						}
					}									
				}				
				break;

			case AttributeBonus.force:
			{
				if(allUnits)												//Todos do tipo unidade
				{
					foreach (IStats stat in statsController.myStats)
					{
						if (stat is Unit)
						{
							Unit u = stat as Unit;							
							u.bonusForce += _bonus;						
						}
					}
				}

				else
				{
					foreach (IStats stat in statsController.myStats)
					{
						if (stat is Unit)
						{
							Unit u = stat as Unit;	
							if(_subCategory != null || _subCategory != null)									// chama por subcategoria
							{									
							if(_subCategory == u.subCategory)u.bonusForce += _bonus;
							else if (u.category == _category)u.bonusForce += _bonus;
							}
						}
					}
				}				
			}
			break;

			case AttributeBonus.speed:
			{
				if(allUnits)												//Todos do tipo unidade
				{
					foreach (IStats stat in statsController.myStats)
					{
						if (stat is Unit)
						{
							Unit u = stat as Unit;							
							u.bonusSpeed += _bonus;						
						}
					}
				}
				
				else
				{
					foreach (IStats stat in statsController.myStats)
					{
						if (stat is Unit)
						{
							Unit u = stat as Unit;	
							if(_subCategory != null || _subCategory != null)									// chama por subcategoria
							{									
								if(_subCategory == u.subCategory)u.bonusSpeed += _bonus;
								else if (u.category == _category)u.bonusSpeed += _bonus;
							}
						}
					}
				}		
			}
				break;

			case AttributeBonus.sight:
			{
				if(allUnits)												//Todos do tipo unidade
				{
					foreach (IStats stat in statsController.myStats)
					{
						if (stat is Unit)
						{
							Unit u = stat as Unit;							
							u.bonusSight += _bonus;							
						}
					}
				}
				
				if(allFactories)												//Todos do tipo factory
				{
					foreach (IStats stat in statsController.myStats)
					{
						if (stat is FactoryBase)
						{
							FactoryBase fb = stat as FactoryBase;
							fb.bonusSight += _bonus;
						}
					}						
				}
				
				else
				{
					foreach (IStats stat in statsController.myStats)
					{
						if (stat is Unit)
						{
							Unit u = stat as Unit;	
							if(_subCategory != null || _subCategory != null)									// chama por subcategoria
							{									
								if(_subCategory == u.subCategory)u.bonusSight += _bonus;
								else if (u.category == _category)u.bonusSight += _bonus;
							}
						}						
						
						else if(stat is FactoryBase)
						{
							FactoryBase fb = stat as FactoryBase;
							if(_subCategory != null || _subCategory != null)
							{			
								
								if(_subCategory == fb.subCategory)fb.bonusSight += _bonus;
								else if (fb.category == _category)fb.bonusSight += _bonus;
							}
						}
					}
				}							
			}
				break;

			case AttributeBonus.projectile:
			{
				if(allUnits)												//Todos do tipo unidade
				{
					foreach (IStats stat in statsController.myStats)
					{
						if (stat is RangeUnit)
						{
							RangeUnit u = stat as RangeUnit;							
							u.bonusProjectile += _bonus;						
						}
					}
				}
				
				else
				{
					foreach (IStats stat in statsController.myStats)
					{
						if (stat is RangeUnit)
						{
							RangeUnit u = stat as RangeUnit;	
							if(_subCategory != null || _subCategory != null)									// chama por subcategoria
							{									
								if(_subCategory == u.subCategory)u.bonusProjectile += _bonus;
								else if (u.category == _category)u.bonusProjectile += _bonus;
							}
						}
					}
				}		
			}
			break;
		}

		string attribBonusString = "bonus"+_attribute.ToString();
		
		if(allUnits)
		{ 
			foreach (Unit u in techTreeController.prefabUnit)
			{
				techTreeController.AttributeModifier(u.category,attribBonusString,_bonus);
			}
		}
		if(allFactories)
		{
			foreach (FactoryBase fb in techTreeController.prefabFactory)
			{
				techTreeController.AttributeModifier(fb.category, attribBonusString, _bonus);
			}
		}

		else
		{
			techTreeController.AttributeModifier(_category,attribBonusString,_bonus);
		}
	}
}
