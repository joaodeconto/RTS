using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Visiorama;

public class Tutorial_Basic : MonoBehaviour {

	[System.Serializable]
	public class Tutorial
	{
		public int order;
		public GameObject tutorialItem;
		public float timeScaleItem;
		public string tutorialTitleText;
		public string tutorialBoxText;
		public UIButton nextTutorialBtn;


	}


	public UIButton nextBtn;
	public UILabel tutorialBox;
	public UILabel tutorialTitle;
	public Tutorial[] tutorial;
	public int tutorialScreenSelected;
	public UILabel tutorialIndex;
	public GameObject tutorialMainBg;
	int i = -1;

	public void OnEnable ()
	{
		Init ();

	}
	
	public void OnDisable ()
	{
		Close ();
	}


	public void Init ()
	
	{

		tutorialIndex.text = tutorialScreenSelected+1 + " / " + tutorial.Length;

		tutorialMainBg.SetActive (true);

		Invoke("PauseTime",2);
		Invoke("ShowNextTutorialItem",2);


	}


		
	
	public void ShowNextTutorialItem ()
	
	{
		if (tutorialScreenSelected < tutorial.Length)
		{
			i++;
			tutorialScreenSelected = i;

		}

		foreach (Tutorial tu in tutorial)
		{

			if (tu.tutorialItem != null)
			{

			tu.tutorialItem.SetActive (false);

			}

			if (tu.order == tutorialScreenSelected)
			{
				if (tu.timeScaleItem != null)
				{

				Time.timeScale = tu.timeScaleItem;
				
				}

				if (tu.tutorialItem != null)
				{

				tu.tutorialItem.SetActive (true);
				
				}

				if (tu.tutorialTitleText != null)
				{

				tutorialTitle.text = tu.tutorialTitleText;
				
				}

				if (tu.tutorialBoxText != null)
				{

				tutorialBox.text = tu.tutorialBoxText;
				
				}

				nextBtn = tu.nextTutorialBtn;

				tutorialIndex.text = tutorialScreenSelected+1 + " / " + tutorial.Length;

			}



		}


	}

	public void ShowPrevTutorialItem ()
		
	{
		if (tutorialScreenSelected > 0)
		{
			i--;
			tutorialScreenSelected = i;

		}

		foreach (Tutorial tu in tutorial)
		{
			
			if (tu.tutorialItem != null)
			{
				
				tu.tutorialItem.SetActive (false);
				
			}
			
			if (tu.order == tutorialScreenSelected)
			{
				if (tu.timeScaleItem != null)
				{
					
					Time.timeScale = tu.timeScaleItem;
					
				}
				
				if (tu.tutorialItem != null)
				{
					
					tu.tutorialItem.SetActive (true);
					
				}
				
				if (tu.tutorialTitleText != null)
				{
					
					tutorialTitle.text = tu.tutorialTitleText;
					
				}
				
				if (tu.tutorialBoxText != null)
				{
					
					tutorialBox.text = tu.tutorialBoxText;
					
				}
				
				nextBtn = tu.nextTutorialBtn;

				tutorialIndex.text = tutorialScreenSelected + 1 + " / " + tutorial.Length;

				
			}
			
			
			
		}
		
		
	}

	public void PauseTime()
	{
		Time.timeScale = 0.0f;
	}

	public void Close ()
	{
		if (tutorialMainBg != null)
		{
		tutorialMainBg.SetActive (false);
		}
	}
	

}
