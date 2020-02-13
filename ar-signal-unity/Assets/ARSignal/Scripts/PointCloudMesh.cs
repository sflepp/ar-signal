using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCloudMesh : MonoBehaviour {
	public List<Point> points = new List<Point>();
	private List<Point> pointsSorted = new List<Point> ();
	private bool pointsUpdated = false;
	
	// Update is called once per frame
	void Update () {
		if (pointsUpdated) {
			GenerateMesh ();
			pointsUpdated = false;
		}
	}

	private void GenerateMesh() {
		
		Vector3[] vertices = new Vector3[points.Count * 6];
		Vector2[] uv = new Vector2[points.Count * 6];
		int[] triangles = new int[points.Count * 6];

		for (int i = 0; i < points.Count; i++) {
			Point[] nearestPoints = FindNNearestPoints (3, points [i]);

			for (int j = 0; j < 3; j++) {
				Point point = nearestPoints [j];
				vertices [i * 3 + j] = new Vector3(point.position.x, point.position.y, point.position.z);
				triangles [i * 3 + j] = i * 3 + j;
				uv [i * 3 + j] = new Vector2 (nearestPoints [j].color, 0);
			}
		}

		Mesh mesh = GetComponent<MeshFilter> ().mesh;
		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.triangles = triangles;
		mesh.RecalculateNormals ();

	}
		
	public void UpdatePoints(List<Point> points) {
		this.points = points;
		this.pointsSorted = new List<Point> (points);
		if (points.Count > 3) {
			this.pointsUpdated = true;
		}

	}


	private Point[] FindNNearestPoints(int n, Point origin) {
		pointsSorted.Sort((x, y) => Vector3.Distance(origin.position, x.position) < Vector3.Distance(origin.position, y.position) ? -1 : 1);
		Point[] nearest = new Point[n];
		for (int i = 0; i < n; i++) {
			nearest [i] = pointsSorted [i];
		}
		return nearest;
	}
}
