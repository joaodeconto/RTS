#pragma strict
public var animationsList : AnimationClip[];
public var currentAnim : int = 0;
public var nextAnim : int;
public var targetModel : GameObject;
function Start () {
	
}

function NextAnim () {
	currentAnim = targetModel.GetComponent(Animator).GetInteger("animer");
	nextAnim = currentAnim+1;
	targetModel.GetComponent(Animator).SetInteger("animer", nextAnim);
}

function PrevAnim () {

	currentAnim = targetModel.GetComponent(Animator).GetInteger("animer");
	nextAnim = currentAnim-1;
	targetModel.GetComponent(Animator).SetInteger("animer", nextAnim);
	
}