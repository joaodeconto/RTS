using UnityEngine;
using System.Collections;
 
class AnimateTiledTexture : MonoBehaviour
{
    public int columns = 2;
    public int rows = 2;
    public float framesPerSecond = 10f;
	public bool playOnAwake = true;
 	public WrapMode wrapMode;
	
    //the current frame to display
    private int index = 0;
	
	private bool playing = false;
 
    void Start()
    {
        if (playOnAwake) StartCoroutine(UpdateTiling ());
 
        //set the tile size of the texture (in UV units), based on the rows and columns
        Vector2 size = new Vector2(1f / columns, 1f / rows);
        renderer.sharedMaterial.SetTextureScale("_MainTex", size);
    }
	
	public void Play ()
	{
       if (!playing) StartCoroutine(UpdateTiling ());
	}
 
    private IEnumerator UpdateTiling ()
    {
		playing = true;
		if (wrapMode == WrapMode.Loop)
		{
	        while (true)
	        {
	            //move to the next index
	            index++;
	            if (index >= rows * columns)
	                index = 0;
	 
	            //split into x and y indexes
	            Vector2 offset = new Vector2((float)index / columns - (index / columns), //x index
	                                          (index / columns) / (float)rows);          //y index
	 
	            renderer.sharedMaterial.SetTextureOffset("_MainTex", offset);
	 
	            yield return new WaitForSeconds(1f / framesPerSecond);
				
				if (wrapMode != WrapMode.Loop)
				{
					StartCoroutine (UpdateTiling ());
					break;
				}
	        }
		}
		else if (wrapMode == WrapMode.PingPong)
		{
			bool forward = true;
			while (true)
			{
				//move to the next index
				if (forward)
				{
		            index++;
		            if (index >= rows * columns)
					{
						forward = false;
						continue;
					}
				}
				else
				{
		            index--;
		            if (index == 0)
					{
						forward = true;
						continue;
					}
				}
	 
	            //split into x and y indexes
	            Vector2 offset = new Vector2((float)index / columns - (index / columns), //x index
	                                          (index / columns) / (float)rows);          //y index
	 
	            renderer.sharedMaterial.SetTextureOffset("_MainTex", offset);
	 
	            yield return new WaitForSeconds(1f / framesPerSecond);
				
				if (wrapMode != WrapMode.PingPong)
				{
					StartCoroutine (UpdateTiling ());
					break;
				}
			}
		}
		else if (wrapMode == WrapMode.ClampForever)
		{
			while (true)
	        {
				if (!playing) continue;
				
	            //move to the next index
	            index++;
	            if (index >= rows * columns)
				{
					playing = false;
				}
	 
	            //split into x and y indexes
	            Vector2 offset = new Vector2((float)index / columns - (index / columns), //x index
	                                          (index / columns) / (float)rows);          //y index
	 
	            renderer.sharedMaterial.SetTextureOffset("_MainTex", offset);
	 
	            yield return new WaitForSeconds(1f / framesPerSecond);
				
				if (wrapMode != WrapMode.ClampForever)
				{
					StartCoroutine (UpdateTiling ());
					break;
				}
	        }
		}
		else if (wrapMode == WrapMode.Once)
		{
			while (true)
	        {
				if (!playing) continue;
				
	            //move to the next index
	            index++;
	            if (index >= rows * columns)
				{
					index = 0;
					playing = false;
				}
	 
	            //split into x and y indexes
	            Vector2 offset = new Vector2( (float)index / columns - (index / columns), //x index
	                                          (index / columns) / (float)rows);          //y index
	 
	            renderer.sharedMaterial.SetTextureOffset("_MainTex", offset);
	 
	            yield return new WaitForSeconds(1f / framesPerSecond);
				
				if (wrapMode != WrapMode.Once)
				{
					StartCoroutine (UpdateTiling ());
					break;
				}
	        }
		}
		playing = false;
    }
}