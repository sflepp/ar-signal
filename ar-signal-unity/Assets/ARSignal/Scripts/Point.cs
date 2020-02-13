using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Point {
	public Vector3 position;
	public float size;
	public float color;
	public float amount = 1;

	public Point (Vector3 position, float size, float color){
		this.position = position;
		this.size = size;
		this.color = color;
	}

	public override int GetHashCode()
	{
		int hash = 13;
		hash = (hash * 7) + position.GetHashCode();
		return hash;
	}
}
