﻿using Assets.CrowdSimulation.Scripts.Utilities;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class NavMeshObject : MonoBehaviour
{
    private NativeArray<float3> positions;
    private NativeArray<bool> graph;
    List<List<float3>> shapes;

    private Dijsktra dijsktra;

    public Transform A;
    public Transform B;

    private static int count;

    public static int Index(int from, int to)
    {
        return from * count + to;
    }

    private void Start()
    {
        dijsktra = new Dijsktra(positions, graph);
    }

    // Update is called once per frame
    void Update()
    {
        if (PhysicsShapeConverter.Changed)
        {
            PhysicsShapeConverter.Changed = false;
            //PhysicsShapeConverter.graph.Draw();
            ReCalculate();
            if (dijsktra != null) dijsktra.Dispose();
            dijsktra = new Dijsktra(positions, graph);
        }

        var pointA = A.position;
        var pointB = B.position;

        var list = CalculatePath(pointA, pointB);

        for (int i = 0; i < list.Count - 1; i++)
        {
            Debug.DrawLine(list[i], list[i + 1], Color.blue);
        }

    }

    List<float3> CalculatePath(float3 pointA, float3 pointB)
    {

        if (IsNotCrossing(pointA, pointB))
        {
            return new List<float3>()
            {
                pointA, pointB
            };
        }

        var nodeListA = new List<int>();
        var nodeListB = new List<int>();

        for (int i = 0; i < positions.Length; i++)
        {
            if (IsNotCrossing(pointA, positions[i]))
            {
                nodeListA.Add(i);
            }
            if (IsNotCrossing(pointB, positions[i]))
            {
                nodeListB.Add(i);
            }
        }
        dijsktra.CalculatePaths(pointB, nodeListB);
        return dijsktra.CalculatePath(pointA, nodeListA);
    }


    void ReCalculate()
    {
        count = 0;
        shapes = PhysicsShapeConverter.graph.GetShapes();
        foreach (var shape in shapes)
        {
            count += shape.Count;
        }
        if (graph.IsCreated) graph.Dispose();
        if (positions.IsCreated) positions.Dispose();
        graph = new NativeArray<bool>(count * count, Allocator.Persistent);
        positions = new NativeArray<float3>(count, Allocator.Persistent);
        Debug.Log(count + " : " + positions.Length);
        var meIndex = 0;
        for (int sI = 0; sI < shapes.Count; sI++)
        {
            var shape = shapes[sI];
            for (int i = 0; i < shape.Count; i++)
            {
                var left = i - 1;
                if (left < 0) left = shape.Count - 1;
                var lPoint = shape[left];
                var me = shape[i];

                positions[meIndex + i] = me;

                if (IsNotCrossing(me, lPoint))
                {
                    AddGraphEdge(meIndex + left, meIndex + i);
                }
                else
                {
                    Debug.DrawLine(me, lPoint, Color.black, 100f);
                }
            }
            meIndex += shape.Count;
        }
        for (int i = 0; i < count; i++)
        {
            for (int j = 0; j < count; j++)
            {
                if (!graph[Index(i, j)])
                {
                    if (IsNotCrossing(positions[i], positions[j]))
                    {
                        graph[Index(i, j)] = true;
                    }
                }
            }
        }


        //for (int i = 0; i < count; i++)
        //{
        //    for (int j = 0; j < count; j++)
        //    {
        //        if (i == j) continue;
        //        if (graph[Index(i, j)])
        //        {
        //            Debug.DrawLine(positions[i], positions[j] + new float3(0, 1, 0), Color.green, 100f);
        //        }
        //    }
        //}
    }

    public bool IsNotCrossing(float3 me, float3 point)
    {
        foreach (var shape in shapes)
        {
            for (int i = 0; i < shape.Count; i++)
            {
                var left = i - 1;
                if (left < 0) left = shape.Count - 1;
                var other1 = shape[left];
                var other2 = shape[i];
                if ((other1.Equals(point) && other2.Equals(me)) || (other1.Equals(me) && other2.Equals(point)))
                {
                    continue;
                }
                if (MyMath.AreInLine(me, point, other1, other2))
                {
                    if (MyMath.AreIntersect(me, point, other1, other2))
                    {
                        return false;
                    }
                    continue;
                }
                if (MyMath.DoIntersect(me, point, other1, other2))
                {
                    return false;
                }
            }

            if (MyMath.InnerPoint((point + me) / 2f, shape.ToArray()))
            {
                return false;
            }
        }
        return true;
    }

    private bool IsNotBetween(float3 left, float3 right, float3 other)
    {
        var angle = Vector3.SignedAngle(left, right, new float3(0, 1, 0));
        if (angle < 0) angle += 180;
        var angle2 = Vector3.SignedAngle(left, other, new float3(0, 1, 0));
        if (angle2 < 0) angle += 180;
        return angle > angle2;
    }

    private void AddGraphEdge(int from, int to)
    {
        graph[Index(from, to)] = true;
        graph[Index(to, from)] = true;
    }

    private void OnDestroy()
    {
        if (graph.IsCreated) graph.Dispose();
        if (positions.IsCreated) positions.Dispose();
    }
}
