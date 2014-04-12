using UnityEngine;
using System.Collections;

public class inputHelper : MonoBehaviour {

	public GameObject HayateHolder;
	public GameObject HayateHolder2;
	public GameObject HayateHolder3;
	
	Hayate hayate;
	
	private Rect window;
	
	void Start()
	{
		window = new Rect(0, Screen.height - 150, Screen.width, 150);
		
		hayate = HayateHolder.GetComponent<Hayate>();
	}
	
	void OnGUI(){
		
		window = GUI.Window(0, window, functionKeys, "Turbulence settings");
		
	}
	
	void functionKeys(int windowID)
	{
		GUILayout.Space(5f);
		GUILayout.BeginHorizontal();
		
		GUILayout.Label("Emitter: ", GUILayout.MaxWidth(80));
			
		if(GUILayout.Button ("Plane", GUILayout.MaxWidth(80)))
		{
			HayateHolder.SetActive(true);
			HayateHolder3.SetActive(false);
			HayateHolder2.SetActive (false);
			hayate = HayateHolder.GetComponent<Hayate>();
		}
		
		if(GUILayout.Button ("Sphere", GUILayout.MaxWidth(80)))
		{
			HayateHolder.SetActive(false);
			HayateHolder3.SetActive(false);
			HayateHolder2.SetActive (true);
			hayate = HayateHolder2.GetComponent<Hayate>();
		}
		if(GUILayout.Button ("Line", GUILayout.MaxWidth(80)))
		{
			HayateHolder.SetActive(false);
			HayateHolder3.SetActive(true);
			HayateHolder2.SetActive (false);
			hayate = HayateHolder3.GetComponent<Hayate>();
		}
		
		if(GUILayout.Button ("Velocity relative", GUILayout.MaxWidth(120)))
		{
			hayate.UseRelativeOrAbsoluteValues = Hayate.TurbulenceType.relative;
			hayate.AssignTurbulenceTo = Hayate.AssignTo.velocity;
		}
		
		if(GUILayout.Button ("Velocity absolute", GUILayout.MaxWidth(120)))
		{
			hayate.UseRelativeOrAbsoluteValues = Hayate.TurbulenceType.absolute;
			hayate.AssignTurbulenceTo = Hayate.AssignTo.velocity;
		}
		
		if(GUILayout.Button ("Position relative", GUILayout.MaxWidth(120)))
		{
			hayate.UseRelativeOrAbsoluteValues = Hayate.TurbulenceType.relative;
			hayate.AssignTurbulenceTo = Hayate.AssignTo.position;
		}
		GUILayout.EndHorizontal();
		
		GUILayout.Space(5f);
		GUILayout.BeginHorizontal();
		
			GUILayout.Label("X-Axis: ", GUILayout.MaxWidth(80));
			
			if(GUILayout.Button ("None", GUILayout.MaxWidth(80)))
			{
				hayate.UseCalculationMethodX = Hayate.CalculationMethod.none;
			}
		
			if(GUILayout.Button ("Sine", GUILayout.MaxWidth(80)))
			{
				hayate.UseCalculationMethodX = Hayate.CalculationMethod.sine;
			}
		
			if(GUILayout.Button ("Cosine", GUILayout.MaxWidth(80)))
			{
				hayate.UseCalculationMethodX = Hayate.CalculationMethod.cosine;
			}
		
			if(GUILayout.Button ("Perlin", GUILayout.MaxWidth(80)))
			{
				hayate.UseCalculationMethodX = Hayate.CalculationMethod.perlin;
			}
		
			if(GUILayout.Button ("Texture", GUILayout.MaxWidth(80)))
			{
				hayate.UseCalculationMethodX = Hayate.CalculationMethod.precalculatedTexture;
			}
		
			if(GUILayout.Button ("Audio", GUILayout.MaxWidth(80)))
			{
				hayate.UseCalculationMethodX = Hayate.CalculationMethod.Audio;
			}
		
		GUILayout.Label("Amplitude: ", GUILayout.MaxWidth(80));
		hayate.Amplitude.x = float.Parse(GUILayout.TextField(hayate.Amplitude.x.ToString(), GUILayout.MaxWidth(50)));
		GUILayout.Label("Frequency: ", GUILayout.MaxWidth(80));
		hayate.Frequency.x = float.Parse(GUILayout.TextField(hayate.Frequency.x.ToString(), GUILayout.MaxWidth(50)));
		GUILayout.Label("Offset: ", GUILayout.MaxWidth(80));
		hayate.Offset.x = float.Parse(GUILayout.TextField(hayate.Offset.x.ToString(), GUILayout.MaxWidth(50)));
		GUILayout.Label("OffsetSpeed: ", GUILayout.MaxWidth(80));
		hayate.OffsetSpeed.x = float.Parse(GUILayout.TextField(hayate.OffsetSpeed.x.ToString(), GUILayout.MaxWidth(50)));
		
		GUILayout.EndHorizontal();
		
		GUILayout.Space(5f);
		GUILayout.BeginHorizontal();
		
			GUILayout.Label("Y-Axis: ", GUILayout.MaxWidth(80));
			
			if(GUILayout.Button ("None", GUILayout.MaxWidth(80)))
			{
				hayate.UseCalculationMethodY = Hayate.CalculationMethod.none;
			}
		
			if(GUILayout.Button ("Sine", GUILayout.MaxWidth(80)))
			{
				hayate.UseCalculationMethodY = Hayate.CalculationMethod.sine;
			}
		
			if(GUILayout.Button ("Cosine", GUILayout.MaxWidth(80)))
			{
				hayate.UseCalculationMethodY = Hayate.CalculationMethod.cosine;
			}
		
			if(GUILayout.Button ("Perlin", GUILayout.MaxWidth(80)))
			{
				hayate.UseCalculationMethodY = Hayate.CalculationMethod.perlin;
			}
		
			if(GUILayout.Button ("Texture", GUILayout.MaxWidth(80)))
			{
				hayate.UseCalculationMethodY = Hayate.CalculationMethod.precalculatedTexture;
			}
		
			if(GUILayout.Button ("Audio", GUILayout.MaxWidth(80)))
			{
				hayate.UseCalculationMethodY = Hayate.CalculationMethod.Audio;
			}
		
		GUILayout.Label("Amplitude: ", GUILayout.MaxWidth(80));
		hayate.Amplitude.y = float.Parse(GUILayout.TextField(hayate.Amplitude.y.ToString(), GUILayout.MaxWidth(50)));
		GUILayout.Label("Frequency: ", GUILayout.MaxWidth(80));
		hayate.Frequency.y = float.Parse(GUILayout.TextField(hayate.Frequency.y.ToString(), GUILayout.MaxWidth(50)));
		GUILayout.Label("Offset: ", GUILayout.MaxWidth(80));
		hayate.Offset.y = float.Parse(GUILayout.TextField(hayate.Offset.y.ToString(), GUILayout.MaxWidth(50)));
		GUILayout.Label("OffsetSpeed: ", GUILayout.MaxWidth(80));
		hayate.OffsetSpeed.y = float.Parse(GUILayout.TextField(hayate.OffsetSpeed.y.ToString(), GUILayout.MaxWidth(50)));
		
		GUILayout.EndHorizontal();
		GUILayout.Space(5f);
		GUILayout.BeginHorizontal();
		
			GUILayout.Label("Z-Axis: ", GUILayout.MaxWidth(80));
			
			if(GUILayout.Button ("None", GUILayout.MaxWidth(80)))
			{
				hayate.UseCalculationMethodZ = Hayate.CalculationMethod.none;
			}
		
			if(GUILayout.Button ("Sine", GUILayout.MaxWidth(80)))
			{
				hayate.UseCalculationMethodZ = Hayate.CalculationMethod.sine;
			}
		
			if(GUILayout.Button ("Cosine", GUILayout.MaxWidth(80)))
			{
				hayate.UseCalculationMethodZ = Hayate.CalculationMethod.cosine;
			}
		
			if(GUILayout.Button ("Perlin", GUILayout.MaxWidth(80)))
			{
				hayate.UseCalculationMethodZ = Hayate.CalculationMethod.perlin;
			}
		
			if(GUILayout.Button ("Texture", GUILayout.MaxWidth(80)))
			{
				hayate.UseCalculationMethodZ = Hayate.CalculationMethod.precalculatedTexture;
			}
		
			if(GUILayout.Button ("Audio", GUILayout.MaxWidth(80)))
			{
				hayate.UseCalculationMethodZ = Hayate.CalculationMethod.Audio;
			}
		
		GUILayout.Label("Amplitude: ", GUILayout.MaxWidth(80));
		hayate.Amplitude.z = float.Parse(GUILayout.TextField(hayate.Amplitude.z.ToString(), GUILayout.MaxWidth(50)));
		GUILayout.Label("Frequency: ", GUILayout.MaxWidth(80));
		hayate.Frequency.z = float.Parse(GUILayout.TextField(hayate.Frequency.z.ToString(), GUILayout.MaxWidth(50)));
		GUILayout.Label("Offset: ", GUILayout.MaxWidth(80));
		hayate.Offset.z = float.Parse(GUILayout.TextField(hayate.Offset.z.ToString(), GUILayout.MaxWidth(50)));
		GUILayout.Label("OffsetSpeed: ", GUILayout.MaxWidth(80));
		hayate.OffsetSpeed.z = float.Parse(GUILayout.TextField(hayate.OffsetSpeed.z.ToString(), GUILayout.MaxWidth(50)));
		
		GUILayout.EndHorizontal();
		
		
		//hayate.Amplitude.y = GUILayout.HorizontalSlider(hayate.Amplitude.y, -500f, 500f);
		//hayate.Amplitude.z = GUILayout.HorizontalSlider(hayate.Amplitude.z, -500f, 500f);
	}
	
}
