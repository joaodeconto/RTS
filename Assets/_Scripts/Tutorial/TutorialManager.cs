using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Visiorama;
using I2.Loc;

public class TutorialManager : MonoBehaviour 
{
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
	public UIButton prevBtn;
	public UIButton closeBtn;
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
		tutorialIndex.text = tutorialScreenSelected+1+"/" + tutorial.Length;
		tutorialMainBg.SetActive (true);
		DefaultCallbackButton dcb;

		dcb = nextBtn.gameObject.GetComponent<DefaultCallbackButton>(); 
		dcb.Init ( null, (ht_hud) =>{ 
			ShowNextTutorialItem();
		});

		dcb = prevBtn.gameObject.GetComponent<DefaultCallbackButton>(); 
		dcb.Init ( null, (ht_hud) =>{ 
			ShowPrevTutorialItem();
		});

		dcb = closeBtn.gameObject.GetComponent<DefaultCallbackButton>(); 
		dcb.Init ( null, (ht_hud) => 
		          { 
			Close();
		});
		
		ShowNextTutorialItem();
	}
	
	
	
	
	public void ShowNextTutorialItem ()	
	{
		if (tutorialScreenSelected < tutorial.Length){
			i++;
			tutorialScreenSelected = i;
		}

		foreach (Tutorial tu in tutorial)
		{
			if (tu.tutorialItem != null)	tu.tutorialItem.SetActive (false);

			if (tu.order == tutorialScreenSelected)
			{
				Time.timeScale = tu.timeScaleItem;

				if (tu.tutorialItem != null)		tu.tutorialItem.SetActive (true);
				if (tu.tutorialTitleText != null)	
					tutorialTitle.text =   ScriptLocalization.Get("Tutorial/" + tu.tutorialTitleText);
				if (tu.tutorialBoxText != null){
					tutorialBox.text = ScriptLocalization.Get("Tutorial/" +tu.tutorialBoxText);
					tutorialBox.parent.GetComponent<UIScrollView>().ResetPosition();				
				}
				tutorialIndex.text = tutorialScreenSelected + "/" + tutorial.Length;
			}
		}
	}

	public void ShowPrevTutorialItem ()		
	{
		if (tutorialScreenSelected > 1){
			i--;
			tutorialScreenSelected = i;
		}

		foreach (Tutorial tu in tutorial)
		{			
			if (tu.tutorialItem != null){				
				tu.tutorialItem.SetActive (false);				
			}			
			if (tu.order == tutorialScreenSelected){			
				if (tu.tutorialItem != null)	tu.tutorialItem.SetActive (true);
				if (tu.tutorialTitleText != null)	tutorialTitle.text = tu.tutorialTitleText;		
				if (tu.tutorialBoxText != null)	tutorialBox.text = tu.tutorialBoxText;
				nextBtn = tu.nextTutorialBtn;
				tutorialIndex.text = tutorialScreenSelected + "/" + (tutorial.Length);				
			}			
		}	
	}

	public void PauseTime()
	{
		Time.timeScale = 0.0f;
	}

	public void Open ()
	{
		if (tutorialMainBg != null){
			tutorialMainBg.SetActive (true);
		}
	}

	public void Close ()
	{
		foreach (Tutorial tu in tutorial)
		{			
			if (tu.tutorialItem != null){				
				tu.tutorialItem.SetActive (false);				
			}
		}
		if (tutorialMainBg != null){
			Time.timeScale = 1f;
			tutorialMainBg.SetActive (false);
		}
	}
}
