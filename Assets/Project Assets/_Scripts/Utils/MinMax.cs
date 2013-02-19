using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class MinMaxVector2
{
	public Vector2 min = Vector2.zero;
	public Vector2 max = Vector2.zero;
}

[Serializable]
public class MinMaxVector3
{
	public Vector3 min = Vector3.zero;
	public Vector3 max = Vector3.zero;
}

[Serializable]
public class MinMaxVector4
{
	public Vector4 min = Vector4.zero;
	public Vector4 max = Vector4.zero;
}

[Serializable]
public class MinMaxDouble
{
	public double min = 0;
	public double max = 0;
}

[Serializable]
public class MinMaxFloat
{
	public float min = 0f;
	public float max = 0f;
}

[Serializable]
public class MinMaxInt
{
	public int min = 0;
	public int max = 0;
}

[Serializable]
public class StartEnd
{
	public int start = 0;
	public int end = 1;
}