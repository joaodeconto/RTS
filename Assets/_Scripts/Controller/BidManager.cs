using UnityEngine;
using System.Collections;

using Visiorama;

public class BidManager : MonoBehaviour
{
	[Range(0.1f,1.0f)]
	public float TaxFactor = 1.0f;
	public bool WonPlayerGainsTheBidOfEachPlayerInTheGame = false;
	
	//Orichalcum => Orichal
	//Passos
	//OK Ao fazer a partida ao criar a sala e adicionado a sala a aposta da partida
	//Quando entram na partida no GamePlay Manager a aposta e feita, com decrescimo dos cristais
	  //OK Primeiro e adicionada aposta nos valores da partida (como e a battle)
	  //Depois descontada no servidor na conta do jogador (DataScore de crystais)
	  //No final da partida o jogador que vence ganha o que foi apostado menos o fator de vitoria 
	  //Que esta no banco de dados

	void Start () {
		DontDestroyOnLoad (this);
	}
	
	public void PayTheBid ()
	{
		PhotonWrapper pw = ComponentGetter.Get <PhotonWrapper> ();
		
		int currentBid = (int)pw.GetPropertyOnRoom ("bid");
		
		Score.SubtractScorePoints (DataScoreEnum.CurrentCrystals, currentBid);
	}

	public void WonTheGame ()
	{
		PhotonWrapper pw = ComponentGetter.Get <PhotonWrapper> ();
		
		float currentBid = (int)pw.GetPropertyOnRoom ("bid");	

		if (WonPlayerGainsTheBidOfEachPlayerInTheGame)
		{
			Room room = pw.GetCurrentRoom ();
			currentBid *= (float)room.maxPlayers;
		}
		else
		{
			currentBid *= 2.0f;
		}
		
		Debug.Log ("currentBid :" + currentBid);
		Debug.Log ("TaxFactor: "  + TaxFactor);
		
		Score.AddScorePoints (DataScoreEnum.CurrentCrystals, (int)(currentBid * TaxFactor));
	}
}
