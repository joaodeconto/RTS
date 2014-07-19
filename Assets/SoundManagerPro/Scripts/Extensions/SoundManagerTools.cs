using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;

public static class SoundManagerTools {
	static readonly System.Random random = new System.Random();
	public static void Shuffle<T> ( ref List<T> theList )
	{
		int n = theList.Count;
		while (n > 1)
		{
			n--;
			int k = random.Next(n + 1);
			T val = theList[k];
			theList[k] = theList[n];
			theList[n] = val;
		}
	}
	
	public static void ShuffleTwo<T, K> ( ref List<T> theList, ref List<K> otherList)
	{
		int n = theList.Count;
		while (n > 1)
		{
			n--;
			int k = random.Next(n + 1);
			T val = theList[k];
			theList[k] = theList[n];
			theList[n] = val;
			
			if(otherList.Count != theList.Count)
			{
				Debug.LogError("Can't shuffle both lists because this " + typeof(T).ToString() + " list doesn't have the same amount of items.");
				continue;
			}
			K otherVal = otherList[k];
			otherList[k] = otherList[n];
			otherList[n] = otherVal;
		}
	}
	
	public static void make2D ( ref AudioSource theAudioSource )
	{
		theAudioSource.panLevel = 0f;
	}
	
	public static void make3D ( ref AudioSource theAudioSource )
	{
		theAudioSource.panLevel = 1f;
	}
	
	public static float VaryWithRestrictions ( this float theFloat, float variance, float minimum=0f, float maximum=1f)
	{
		float max = theFloat * (1f+variance);
		float min = theFloat * (1f-variance);
		
		if(max > maximum)
			max = maximum;
		if(min < minimum)
			min = minimum;
		
		return UnityEngine.Random.Range(min, max);
	}
	
	public static float Vary ( this float theFloat, float variance)
	{
		float max = theFloat * (1f+variance);
		float min = theFloat * (1f-variance);
		
		return UnityEngine.Random.Range(min, max);
	}
	
#if !(UNITY_WP8 || UNITY_METRO)
	/// <summary>
	/// Returns all instance fields on an object, including inherited fields
	/// http://stackoverflow.com/a/1155549/154165
	/// </summary>
	public static FieldInfo[] GetAllFieldInfos(this Type type)
	{
		if(type == null)
			return new FieldInfo[0];

		BindingFlags flags = 
			BindingFlags.Public | 
			BindingFlags.NonPublic | 
			BindingFlags.Instance | 
			BindingFlags.DeclaredOnly;

		return type.GetFields(flags)
			.Concat(GetAllFieldInfos(type.BaseType))
			.Where(f => !f.IsDefined(typeof(HideInInspector), true))
			.ToArray();
	}
#endif
}
