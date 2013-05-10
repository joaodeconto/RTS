using UnityEngine;
using System.Collections;

public class MessagePartial : MonoBehaviour
{
	private const int loaderMaxY =  370;
	private const int loaderMinY = -370;

	private const int loaderMaxX =  630;
	private const int loaderMinX = -630;

	private static Color defaultColor = new Color(0.0f,0.0f,0.0f,1.0f);

	public UILabel MessageLabel;

	private Color cLabelColor;

	public MessagePartial InitPartial ( string message,
										float hPosPercent = 0.0f,
										float vPosPercent = 0.0f)
	{
		return InitPartial (message, -1.0f, hPosPercent, vPosPercent, defaultColor);
	}

	public MessagePartial InitPartial (	string message,
										float duration,
										float hPosPercent,
										float vPosPercent)
	{
		return InitPartial(message,duration,hPosPercent,vPosPercent,defaultColor);
	}

	/**
		@param duration This parameter is in miliseconds. If it is zero the duration will be forever
	*/
	public MessagePartial InitPartial (	string message,
										float duration,
										float hPosPercent,
										float vPosPercent,
										Color color)
	{
		MessageLabel.enabled = true;

		MessageLabel.text  	= message;
		MessageLabel.color 	= color;
		cLabelColor 		= color;

		//Posicionando label da mensagem
		//int xPos = Mathf.FloorToInt(loaderMinX + (float)(loaderMaxX - loaderMinX) * hPosPercent);
		//int yPos = Mathf.FloorToInt(loaderMinY + (float)(loaderMaxY - loaderMinY) * vPosPercent);

		//MessageLabel.transform.localPosition = new Vector3 (xPos, yPos, MessageLabel.transform.localPosition.z);
		//VDebug.Log ("duration: " + duration);
		if (duration > 0)
		{
			InvokeRepeating("Step", duration * 0.01f, duration * 0.01f);
		}

		return this;
	}

	private void Step()
	{
		cLabelColor.a -= 0.01f;
		MessageLabel.color = cLabelColor;
		if (cLabelColor.a < 0.1f)
		{
			Close();
		}
	}

	public MessagePartial Close()
	{
		//VDebug.Log ("terminou de mostrar a mensagem");
		MessageLabel.enabled = false;
		CancelInvoke("Step");

		return this;
	}
}
