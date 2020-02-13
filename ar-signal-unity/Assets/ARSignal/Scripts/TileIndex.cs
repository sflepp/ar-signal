using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileIndex {
	public int x;
	public int y;
	public int z;

	public TileIndex(Vector3 point) {
		x = (int) point.x; 
		y = (int) point.y; 
		z = (int) point.z;
	}

	public TileIndex(int x, int y, int z) {
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public override int GetHashCode()
	{
		int hash = 13;
		hash = (hash * 7) + x.GetHashCode();
		hash = (hash * 7) + y.GetHashCode();
		hash = (hash * 7) + z.GetHashCode();
		return hash;
	}

	public override bool Equals(object obj)
	{
		var item = obj as TileIndex;

		if (item == null)
		{
			return false;
		}

		return this.GetHashCode().Equals(item.GetHashCode());
	}
}
