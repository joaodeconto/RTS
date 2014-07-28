using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScoreRow : MonoBehaviour 
{
	public UISprite rowBorder;

	public UILabel playerName;
	public UILabel playerTeam;

	public UISprite playerAvatar;

	public UISlider ressourceGold;
	public UISlider ressourceMana;
	public UISlider ressourceSpent;

	public UISlider unitsBuild;
	public UISlider unitsLost;
	public UISlider unitsDestroyed;

	public UISlider StructuresBuild;
	public UISlider StructuresLost;
	public UISlider StructuresDestroyed;

	public UILabel gameResult;
	public UILabel playerScoreModifier;
	public UISprite gameResultFeedback;

	public UILabel rankLadder;
	public UISprite rankLadderSignal;
	public UILabel playerNewRank;

}
