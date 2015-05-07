using UnityEngine;
using System.Collections;
using Visiorama;

public class MarketModelButton : MonoBehaviour {

	private UILabel  itemName;
	private UILabel  itemDescription;
	private UILabel	 itemPrice;
	private UILabel	 itemClass;
	private UISprite itemSprite;
	public int price;
	public bool itemAvailability; 
	private Transform infoPanel;
	public GameObject itemModel;
	public enum ItemType{unit,factory,upgrade,item}
	public ItemType itemType;

	#region Info Declares
	private Transform infoUnit;
	private Transform infoFactory;
	private Transform infoUpgrade;
	private Transform infoResources;
	private Transform infoQuali;
	private Transform infoReq;	
	private UILabel attackLabel;
	private UILabel hpLabel;
	private UILabel speedLabel;
	private UILabel timeLabel;
	private UILabel unitsLabel;
	private UILabel defLabel;
	private UILabel descriptLabel;
	private UILabel stats1;
	private UILabel stats2;
	private UILabel stats1Text;
	private UILabel stats2Text;
	private UILabel reqLabel;
	private UILabel skillLabel;
//  private UILabel enableLabel;
	private UILabel goldLabel;
	private UILabel manaLabel;
	private bool wasInitialized = false;


	#endregion

	void OnEnable()
	{
		if(!wasInitialized)	Init();
	}
	public void Init()	
	{
		wasInitialized = true;

		infoPanel 		= transform.FindChild("Panel Info Box");
		itemName 		= transform.FindChild("itemName").GetComponent<UILabel>();
		itemDescription = transform.FindChild("itemDescription").GetComponent<UILabel>();
		itemPrice 		= transform.FindChild("itemPrice").GetComponent<UILabel>();
		itemSprite 		= transform.FindChild("itemSprite").GetComponent<UISprite>();
		itemClass       = transform.FindChild("itemClass").GetComponent<UILabel>();
		itemPrice.text 	= price.ToString();

		infoUpgrade 	= infoPanel.FindChild ("Info Upgrade");
		infoFactory 	= infoPanel.FindChild ("Info Factory");
		infoUnit 		= infoPanel.FindChild ("Info Unit");
		infoQuali 		= infoPanel.FindChild ("Info Qualities");
		infoReq 		= infoPanel.FindChild ("Info Require");		
		infoResources 	= infoPanel.FindChild ("Info Resources");

		switch (itemType)
		{
			case ItemType.unit:			
				Unit unitModel = itemModel.GetComponent<Unit>();
				itemName.text = unitModel.category;				
				itemDescription.text = unitModel.description;
				itemSprite.spriteName = unitModel.guiTextureName;
				itemClass.text = "Unit";
				ShowInfoUnit(unitModel);
				InstanceModel(itemModel);
				break;

			case ItemType.factory:
				FactoryBase factoryModel = itemModel.GetComponent<FactoryBase>(); 
				itemName.text = factoryModel.category;
				itemDescription.text = factoryModel.description;
				itemSprite.spriteName = factoryModel.guiTextureName;
				itemClass.text = "Structure";
				ShowInfoFactory(factoryModel);
				break;

			case ItemType.upgrade:
				Upgrade upgradeModel = itemModel.GetComponent<Upgrade>() as Upgrade;
				itemName.text = upgradeModel.upgradeName;
				itemDescription.text = upgradeModel.description;
				itemSprite.spriteName = upgradeModel.guiTextureName;
				itemClass.text = "Upgrade";
				ShowInfoUpgrade(upgradeModel);
				break;

			case ItemType.item:	// TODO Implementar
				break;
		}
	}

	public void ShowInfoUnit (Unit unit)
	{	
		infoUnit.gameObject.SetActive(true);

		reqLabel 			= infoReq.FindChild ("require-label").GetComponent<UILabel>();
		attackLabel 		= infoUnit.FindChild ("attack-label").GetComponent<UILabel> ();
		hpLabel	    		= infoUnit.FindChild ("hp-label").GetComponent<UILabel> ();
		speedLabel  		= infoUnit.FindChild ("speed-label").GetComponent<UILabel> ();
		unitsLabel  		= infoUnit.FindChild ("house-label").GetComponent<UILabel> ();
		timeLabel   		= infoUnit.FindChild ("time-label").GetComponent<UILabel> ();
		defLabel    		= infoUnit.FindChild ("def-label").GetComponent<UILabel> ();
		skillLabel 			= infoUnit.FindChild ("skill-label").GetComponent<UILabel> ();
//		enableLabel 		= infoReq.FindChild ("enable-label").GetComponent<UILabel> ();
		goldLabel			= infoResources.FindChild("gold-label").GetComponent<UILabel>();
		manaLabel			= infoResources.FindChild("mana-label").GetComponent<UILabel>();

		reqLabel.text 		= unit.requisites;
		skillLabel.text 	= unit.unitSkill.ToString();					
		attackLabel.text 	= unit.force.ToString ();
		defLabel.text 		= unit.defense.ToString ();				
		hpLabel.text 		= unit.maxHealth.ToString ();
		speedLabel.text 	= unit.normalSpeed.ToString();
		unitsLabel.text 	= unit.numberOfUnits.ToString ();
		timeLabel.text 		= unit.timeToSpawn.ToString ()+"s";
		goldLabel.text 		= unit.costOfResources.Rocks.ToString();
		manaLabel.text 		= unit.costOfResources.Mana.ToString();
	}
	
	public void ShowInfoFactory (FactoryBase factory)
	{
		infoFactory.gameObject.SetActive(true);

		goldLabel			= infoResources.FindChild("gold-label").GetComponent<UILabel>();
		manaLabel			= infoResources.FindChild("mana-label").GetComponent<UILabel>();
		defLabel 			= infoFactory.FindChild ("def-label").GetComponent<UILabel> ();
		hpLabel 			= infoFactory.FindChild ("hp-label").GetComponent<UILabel> ();
		reqLabel 			= infoReq.FindChild("require-label").GetComponent<UILabel>();

		reqLabel.text 		= factory.requisites;
		hpLabel.text 		= factory.MaxHealth.ToString();
		defLabel.text 		= factory.defense.ToString();
		goldLabel.text 		= factory.costOfResources.Rocks.ToString();
		manaLabel.text 		= factory.costOfResources.Mana.ToString();

	}	
	
	public void ShowInfoUpgrade (Upgrade upgrade)
	{		
		infoUpgrade.gameObject.SetActive(true);
		timeLabel 			= infoUpgrade.FindChild ("time-label").GetComponent<UILabel>();
		stats2Text 			= infoUpgrade.FindChild ("stats2-text").GetComponent<UILabel>();
		stats2 				= infoUpgrade.FindChild ("stats2-label").GetComponent<UILabel>();
		stats1Text 			= infoUpgrade.FindChild ("stats1-text").GetComponent<UILabel>();
		stats1 				= infoUpgrade.FindChild ("stats1-label").GetComponent<UILabel>();
		reqLabel		 	= infoReq.FindChild("require-label").GetComponent<UILabel>();
		goldLabel			= infoResources.FindChild("gold-label").GetComponent<UILabel>();
		manaLabel			= infoResources.FindChild("mana-label").GetComponent<UILabel>();

		reqLabel.text 		= upgrade.requisites;
		stats1.text 		= upgrade.stats1Value;
		stats1Text.text 	= upgrade.stats1Text;
		stats2.text 		= upgrade.stats2Value;
		stats2Text.text 	= upgrade.stats2Text;
		timeLabel.text 		= upgrade.timeToSpawn.ToString()+"s";
		goldLabel.text 		= upgrade.costOfResources.Rocks.ToString();
		manaLabel.text 		= upgrade.costOfResources.Mana.ToString();

	}	

	public void InstanceModel(GameObject model)
	{

		Transform parentPos = transform.FindChild("refInstance");
		GameObject instance = Instantiate(Resources.Load(model.name, typeof(GameObject)), parentPos.position, parentPos.rotation) as GameObject;
		instance.transform.parent = parentPos;
		instance.transform.localScale = new Vector3(1f,1f,1f);

		Worker w = itemModel.GetComponent<Worker>(); if(w != null) w.DisableResourceTools();
		FactoryBase fb = itemModel.GetComponent<FactoryBase>();
		if(fb != null) foreach(GameObject go in fb.buildingObjects.desactiveObjectsWhenInstance){ go.SetActive(false);} 

		MonoBehaviour[] comps = instance.GetComponents<MonoBehaviour>();		
		foreach(MonoBehaviour c in comps){	c.enabled = false;}
		NavMeshAgent na = instance.GetComponent<NavMeshAgent>(); if (na != null) na.enabled = false;
		Collider co = instance.GetComponent<Collider>(); if (co != null) co.enabled = false;
//		instance.AddComponent("AnimateInQueue");
//		instance.GetComponent<AnimateInQueue>().Init();
	}

	// Update is called once per frame
	void Update () {
	
	}
}
