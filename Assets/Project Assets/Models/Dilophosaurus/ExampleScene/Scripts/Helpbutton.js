#pragma strict
public var raptor : boolean = true;

function Start () {
GameObject.Find("leftButton").guiTexture.pixelInset = Rect(
		Screen.width*0.2,
		Screen.height*0.1,
		Screen.width*0.05, 
		Screen.width*0.05);
		
GameObject.Find("rightButton").guiTexture.pixelInset = Rect(
		Screen.width*0.8,
		Screen.height*0.1,
		Screen.width*0.05, 
		Screen.width*0.05);	

GameObject.Find("helpbutton").guiTexture.pixelInset = Rect(
		Screen.width*0.9,
		Screen.height*0.8,
		Screen.width*0.06, 
		Screen.width*0.06);	

GameObject.Find("animationList").guiTexture.pixelInset = Rect(
		Screen.width/2-Screen.height*0.5*304/176/2,
		Screen.height*0.5,
		Screen.height*0.5*304/176, 
		Screen.height*0.5);			
		
}

function OnMouseDown () {

if (GameObject.Find("animationList").guiTexture.enabled == false) GameObject.Find("animationList").guiTexture.enabled = true;
else GameObject.Find("animationList").guiTexture.enabled = false;

}