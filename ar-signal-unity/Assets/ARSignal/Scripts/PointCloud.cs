using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using UnityEngine.XR.iOS;
using UnityEngine.Networking;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof (MeshFilter))]
[RequireComponent(typeof (MeshRenderer))]
public class PointCloud : MonoBehaviour {
	private const float POOR_QUALITY_TIME_LIMIT = 2f;
	private const int MAX_VERTEX_COUNT = 65000;
	private const int CUBE_VERTEX_COUNT = 14;
	public float cubeSize = 0.05f;

	public ARPointCloudManager pointCloudManager;


	int frameCount = 0;

	private Vector3[] m_PointCloudData;
	private Vector3 cameraPosition = new Vector3(0,0,0);
	private bool frameUpdated = false;
	private float connectionQuality = 0f;

	Dictionary<TileIndex, PointCloudTile> tiles;
	PointCloudMesh pointCloudMesh;

	private bool measurementActive = false;

	void OnConnectionQualityChange(string value){
		Debug.Log ("Connection Quality: " + value);
		connectionQuality = float.Parse (value);
	}

	void Start () {
		tiles = new Dictionary<TileIndex, PointCloudTile> ();
		pointCloudMesh = AddPointCloudMesh ();
		pointCloudManager.pointCloudsChanged += ARFrameUpdated;

		Debug.Log("Start");
		//UnityARSessionNativeInterface.ARFrameUpdatedEvent += ARFrameUpdated;
	}

	void Update() {
		frameCount++;
		if (frameUpdated) {
			for (int i = 0; i < m_PointCloudData.Length; i++) {
				if (Vector3.Distance (m_PointCloudData [i], cameraPosition) < 10) {
					AddPoint (m_PointCloudData[i]);
				}
			}
		}
	}

	private void UpdatePointCloudMesh() {
		int cubeCount = 0;
		pointCloudMesh.points.Clear ();

		foreach(KeyValuePair<TileIndex, PointCloudTile> tile in tiles) {
			foreach (KeyValuePair<string, Point> point in tile.Value.points) {
				cubeCount++;
				pointCloudMesh.points.Add (point.Value);
			}
		}

		pointCloudMesh.UpdatePoints (pointCloudMesh.points);

		Debug.Log ("CubeCount: " + cubeCount);
	}


	public void ARFrameUpdated(ARPointCloudChangedEventArgs e)
    {
		Debug.Log("ARFrameUpdated");
		foreach (ARPointCloud pointCloud in pointCloudManager.trackables)
		{
			// Do something with the ARPlane

			if(pointCloud.positions.HasValue)
            {
				m_PointCloudData = pointCloud.positions.Value.ToArray();
				frameUpdated = true;
			}
			
		}
	}

	/*public void ARFrameUpdated(UnityARCamera camera)
	{
		if (camera.pointCloudData.Length > 0) {
			m_PointCloudData = camera.pointCloudData;
			frameUpdated = true;
		}
	} */

	private void AddPoint(Vector3 position) {
		TileIndex tileIndex = new TileIndex (position);
		PointCloudTile pointCloudTile;
		if (!tiles.TryGetValue (tileIndex, out pointCloudTile)) {
			pointCloudTile = AddTile (tileIndex);
		}

		Point point = new Point(position, cubeSize, connectionQuality);

		pointCloudTile.QueuePoint(point);
	}

	public PointCloudTile AddTile (TileIndex tileIndex)
	{
		GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
		GameObject.DestroyImmediate(go.GetComponent<Collider>());
		go.GetComponent<MeshFilter>().mesh = new Mesh();
		go.GetComponent<Renderer>().sharedMaterial = GetComponent<Renderer>().material;
		go.name = "point-cloud-tile-(" + tileIndex.x + ", " + tileIndex.y + ", " + tileIndex.z + ")";
		go.transform.parent = transform;
		go.transform.localPosition = Vector3.zero;
		go.transform.localRotation = Quaternion.identity;
		go.transform.localScale = Vector3.one;

		PointCloudTile pointCloudTile = go.AddComponent<PointCloudTile> ();
		pointCloudTile.tileIndex = tileIndex;
		tiles.Add (tileIndex, pointCloudTile);

		return pointCloudTile;
	}

	public PointCloudMesh AddPointCloudMesh() {
		GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
		GameObject.DestroyImmediate(go.GetComponent<Collider>());
		go.GetComponent<MeshFilter>().mesh = new Mesh();
		go.GetComponent<Renderer>().sharedMaterial = GetComponent<Renderer>().material;
		go.name = "point-cloud-mesh";
		go.transform.parent = transform;
		go.transform.localPosition = Vector3.zero;
		go.transform.localRotation = Quaternion.identity;
		go.transform.localScale = Vector3.one;

		PointCloudMesh pointCloudMesh = go.AddComponent<PointCloudMesh> ();
		return pointCloudMesh;
	}
}
