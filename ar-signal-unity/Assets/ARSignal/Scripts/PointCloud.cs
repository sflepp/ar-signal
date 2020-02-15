using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
// using UnityEngine.XR.iOS;
using UnityEngine.Networking;
using UnityEngine.XR.ARFoundation;




[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class PointCloud : NetworkBehaviour
{
    public float cubeSize = 0.05f;

    public ARPointCloudManager pointCloudManager;
    
    public readonly SyncPointCloud PointCloudData = new SyncPointCloud();
    public HashSet<ulong> drawnPoints = new HashSet<ulong>();
    
    private float connectionQuality = 0f;

    Dictionary<TileIndex, PointCloudTile> tiles;
    PointCloudMesh pointCloudMesh;

    private bool measurementActive = false;

    void OnConnectionQualityChange(string value)
    {
        Debug.Log("Connection Quality: " + value);
        connectionQuality = float.Parse(value);
    }

    void Start()
    {
        tiles = new Dictionary<TileIndex, PointCloudTile>();
        pointCloudMesh = AddPointCloudMesh();
        pointCloudManager.pointCloudsChanged += ARFrameUpdated;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        // Called when new points are available on server
        PointCloudData.Callback += PointCloudDataChangedOnServer;
    }

    void PointCloudDataChangedOnServer(SyncPointCloud.Operation op, ulong key, Vector3 item)
    {
        Debug.Log(op + " - " + key);
        DrawPoints();
    }

    private void DrawPoints()
    {
        foreach (var point in PointCloudData)
        {
            //if (!drawnPoints.Contains(point.Key))
            //{
                AddPoint(point.Value);
                drawnPoints.Add(point.Key);
            //}
        }
    }

    private void UpdatePointCloudMesh()
    {
        int cubeCount = 0;
        pointCloudMesh.points.Clear();

        foreach (KeyValuePair<TileIndex, PointCloudTile> tile in tiles)
        {
            foreach (KeyValuePair<string, Point> point in tile.Value.points)
            {
                cubeCount++;
                pointCloudMesh.points.Add(point.Value);
            }
        }

        pointCloudMesh.UpdatePoints(pointCloudMesh.points);

        Debug.Log("CubeCount: " + cubeCount);
    }

    public void ARFrameUpdated(ARPointCloudChangedEventArgs e)
    {
        foreach (var arPointCloud in e.added)
        {
            if (arPointCloud.identifiers.HasValue && arPointCloud.positions.HasValue)
            {
                for (int i = 0; i < arPointCloud.identifiers.Value.Length; i++)
                {
                    PointCloudData.Add(arPointCloud.identifiers.Value[i], arPointCloud.positions.Value[i]);
                }
            }
        }
        
        foreach (var arPointCloud in e.updated)
        {
            if (arPointCloud.identifiers.HasValue && arPointCloud.positions.HasValue)
            {
                for (int i = 0; i < arPointCloud.identifiers.Value.Length; i++)
                {
                    PointCloudData[arPointCloud.identifiers.Value[i]] = arPointCloud.positions.Value[i];
                }
            }
        }
        
        foreach (var arPointCloud in e.removed)
        {
            if (arPointCloud.identifiers.HasValue)
            {
                for (int i = 0; i < arPointCloud.identifiers.Value.Length; i++)
                {
                    PointCloudData.Remove(arPointCloud.identifiers.Value[i]);
                }
            }
        }

        DrawPoints();
    }
    
    private void AddPoint(Vector3 position)
    {
        TileIndex tileIndex = new TileIndex(position);
        PointCloudTile pointCloudTile;
        if (!tiles.TryGetValue(tileIndex, out pointCloudTile))
        {
            pointCloudTile = AddTile(tileIndex);
        }

        Point point = new Point(position, cubeSize, connectionQuality);

        pointCloudTile.QueuePoint(point);
    }

    public PointCloudTile AddTile(TileIndex tileIndex)
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

        PointCloudTile pointCloudTile = go.AddComponent<PointCloudTile>();
        pointCloudTile.tileIndex = tileIndex;
        tiles.Add(tileIndex, pointCloudTile);

        return pointCloudTile;
    }

    public PointCloudMesh AddPointCloudMesh()
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        GameObject.DestroyImmediate(go.GetComponent<Collider>());
        go.GetComponent<MeshFilter>().mesh = new Mesh();
        go.GetComponent<Renderer>().sharedMaterial = GetComponent<Renderer>().material;
        go.name = "point-cloud-mesh";
        go.transform.parent = transform;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        PointCloudMesh pointCloudMesh = go.AddComponent<PointCloudMesh>();
        return pointCloudMesh;
    }
}