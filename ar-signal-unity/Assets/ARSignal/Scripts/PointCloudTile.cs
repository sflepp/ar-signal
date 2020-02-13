using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCloudTile : MonoBehaviour {
	private const string POINT_ACCURACY = "0.0";
	private const int POINT_AMOUNT_DRAW_LIMIT = 50;
	public TileIndex tileIndex;

	public Dictionary<string, Point> queuedPoints = new Dictionary<string,Point>();
	public Dictionary<string, Point> points = new Dictionary<string,Point>();


	// Update is called once per frame
	void Update () {
		DrawPoints ();		
	}


	public void QueuePoint(Point point) {
		string key = PointToKey (point);

		if (queuedPoints.ContainsKey (key)) {
			Point existingPoint = queuedPoints [key];
			existingPoint.amount++;
			existingPoint.color = point.color;

			if (!points.ContainsKey (key)) {
				existingPoint.position.x += (point.position.x - existingPoint.position.x) / (float)existingPoint.amount;
				existingPoint.position.y += (point.position.y - existingPoint.position.y) / (float)existingPoint.amount;
				existingPoint.position.z += (point.position.z - existingPoint.position.z) / (float)existingPoint.amount;
			}

		} else {
			queuedPoints.Add (key, point);
		}
	}

	private string PointToKey(Point point){
		return point.position.x.ToString(POINT_ACCURACY) + "," + point.position.y.ToString(POINT_ACCURACY) + "," + point.position.z.ToString(POINT_ACCURACY);
	}

	public void DrawPoints(){
		foreach(KeyValuePair<string, Point> item in queuedPoints)
		{
			if (item.Value.amount > POINT_AMOUNT_DRAW_LIMIT) {
				AddCube (item.Value);

				if (points.ContainsKey (item.Key)) {
					points [item.Key].amount += item.Value.amount;
					points [item.Key].color = item.Value.color;
				} else {
					points.Add (item.Key, item.Value);
				}
			} 
		}

		foreach (KeyValuePair<string, Point> item in points) {
			queuedPoints.Remove (item.Key);
		}
	}



	public void AddCube (Point point) {
		Mesh mesh = GetComponent<MeshFilter> ().mesh;

		float zero = (-point.size / 2.0f);
		float one = point.size / 2.0f;


		Vector3[] vertices = {
			new Vector3(zero, one, zero),
			new Vector3(zero, zero, zero),
			new Vector3(one, one, zero),
			new Vector3(one, zero, zero),

			new Vector3(zero, zero, one),
			new Vector3(one, zero, one),
			new Vector3(zero, one, one),
			new Vector3(one, one, one),

			new Vector3(zero, one, zero),
			new Vector3(one, one, zero),

			new Vector3(zero, one, zero),
			new Vector3(zero, one, one),

			new Vector3(one, one, zero),
			new Vector3(one, one, one)
		};

		for (int i = 0; i < vertices.Length; i++) {
			vertices [i].x += point.position.x;
			vertices [i].y += point.position.y;
			vertices [i].z += point.position.z;
		}

		int[] triangles = {
			0, 2, 1, // front
			1, 2, 3,
			4, 5, 6, // back
			5, 7, 6,
			6, 7, 8, //top
			7, 9 ,8, 
			1, 3, 4, //bottom
			3, 5, 4,
			1, 11,10,// left
			1, 4, 11,
			3, 12, 5,//right
			5, 12, 13
		};

		for (int i = 0; i < triangles.Length; i++) {
			triangles [i] += mesh.vertices.Length;
		}

		Vector2 uv = new Vector2 (point.color * 0.33f, 0);
		Vector2[] uvs = new Vector2[vertices.Length];
		for (int i = 0; i < uvs.Length; i++) {
			uvs [i] = uv;
		}
			
		Vector3[] newVertices = Concat(mesh.vertices, vertices);
		Vector2[] newUvs = Concat(mesh.uv, uvs);
		mesh.vertices = newVertices;
		mesh.uv = newUvs;
		mesh.triangles = Concat(mesh.triangles, triangles);



		mesh.RecalculateNormals ();
	}

	Vector3 [] Concat(Vector3[] a, Vector3[] b) {
		Vector3[] concatted = new Vector3[a.Length + b.Length];
		a.CopyTo (concatted, 0);
		b.CopyTo (concatted, a.Length);
		return concatted;
	}

	Vector2 [] Concat(Vector2[] a, Vector2[] b) {
		Vector2[] concatted = new Vector2[a.Length + b.Length];
		a.CopyTo (concatted, 0);
		b.CopyTo (concatted, a.Length);
		return concatted;
	}

	int [] Concat(int[] a, int[] b) {
		int[] concatted = new int[a.Length + b.Length];
		a.CopyTo (concatted, 0);
		b.CopyTo (concatted, a.Length);
		return concatted;
	}
}
