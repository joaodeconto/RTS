#pragma strict
var target : GameObject ;
var aniMax : int ;
var aniMin : int ;

function OnMouseDown () {
if(this.name == "rightButton" ) target.GetComponent(AnimationManager).NextAnim();
else target.GetComponent(AnimationManager).PrevAnim();

if (target.GetComponent(Animator).GetInteger("animer") == 0 )
	this.guiTexture.enabled = false;
if (target.GetComponent(Animator).GetInteger("animer") > 0){
	print (target.GetComponent(Animator).GetInteger("animer"));
	GameObject.Find("leftButton").guiTexture.enabled = true;
	}

if (target.GetComponent(Animator).GetInteger("animer") > 3 )
	GameObject.Find("rightButton").guiTexture.enabled = false;
if (target.GetComponent(Animator).GetInteger("animer") < 4 )
	GameObject.Find("rightButton").guiTexture.enabled = true;


}
