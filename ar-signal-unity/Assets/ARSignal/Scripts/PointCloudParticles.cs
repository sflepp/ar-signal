﻿using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
// using UnityEngine.XR.iOS;
using UnityEngine.Networking;
using UnityEngine.XR.ARFoundation;


[System.Serializable]
public class SyncPointCloud : SyncDictionary<ulong, Vector3>
{
}

[RequireComponent(typeof(ARPointCloudManager))]
public class PointCloudParticles : NetworkBehaviour
{
    ParticleSystem.Particle[] particles = new ParticleSystem.Particle[10000];
    public ParticleSystem _particleSystem;
    private bool _pointsUpdated;

    private ARPointCloudManager pointCloudManager;

    public readonly SyncPointCloud PointCloudData = new SyncPointCloud();

    private float connectionQuality = 0f;

    private bool measurementActive = false;

    void Start()
    {
        pointCloudManager = GetComponent<ARPointCloudManager>();
        pointCloudManager.pointCloudsChanged += ARFrameUpdated;

        for (int i = 0; i < particles.Length; i++)
        {
            particles[i] = new ParticleSystem.Particle();
        }
    }

    private void Update()
    {
        if (_pointsUpdated)
        {
            _particleSystem.SetParticles(particles, particles.Length);
            _pointsUpdated = false;
        }
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
        try
        {
            Debug.Log("DrawPoints");
            int i = 0;
            foreach (var point in PointCloudData)
            {
                particles[i].position = point.Value;
                particles[i].startColor = new Color32(255, 0, 0, 255);
                particles[i].startSize = 0.025f;
                i++;
            }
            
            Debug.Log("Amount of points: " + i);

            _pointsUpdated = true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public void ARFrameUpdated(ARPointCloudChangedEventArgs e)
    {
        foreach (var arPointCloud in e.added)
        {
            if (arPointCloud.identifiers.HasValue && arPointCloud.positions.HasValue &&
                arPointCloud.confidenceValues.HasValue)
            {
                for (int i = 0; i < arPointCloud.identifiers.Value.Length; i++)
                {
                    if (arPointCloud.confidenceValues.Value[i] > 0.75)
                    {
                        PointCloudData.Add(arPointCloud.identifiers.Value[i], arPointCloud.positions.Value[i]);
                    }
                }
            }
        }

        foreach (var arPointCloud in e.updated)
        {
            if (arPointCloud.identifiers.HasValue && arPointCloud.positions.HasValue)
            {
                for (int i = 0; i < arPointCloud.identifiers.Value.Length; i++)
                {
                    if (arPointCloud.confidenceValues.Value[i] > 0.75)
                    {
                        PointCloudData[arPointCloud.identifiers.Value[i]] = arPointCloud.positions.Value[i];
                    }
                }
            }
        } 

        foreach (var arPointCloud in e.removed)
        {
            if (arPointCloud.identifiers.HasValue)
            {
                for (int i = 0; i < arPointCloud.identifiers.Value.Length; i++)
                {
                    if (PointCloudData.ContainsKey(arPointCloud.identifiers.Value[i]))
                    {
                        PointCloudData.Remove(arPointCloud.identifiers.Value[i]);
                    }
                }
            }
        }

        DrawPoints();
    }
}