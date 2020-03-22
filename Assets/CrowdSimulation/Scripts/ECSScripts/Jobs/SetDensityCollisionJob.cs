﻿using Unity.Entities;
using Unity.Transforms;
using Unity.Physics;
using Unity.Collections;
using Unity.Mathematics;

public struct SetDensityCollisionJob : IJobForEach<PhysicsCollider, LocalToWorld>
{
    private static readonly float distance = 3f;
    [NativeDisableParallelForRestriction]
    public NativeArray<float> densityMatrix;

    public void Execute(ref PhysicsCollider collider, ref LocalToWorld localToWorld)
    {
        var aabb = collider.Value.Value.CalculateAabb();
        for (int j = 0; j < Map.heightPoints - 1; j++)
            for (int i = 0; i < Map.widthPoints - 1; i++)
            {
                var point = DensitySystem.ConvertToWorld(new float3(i, 0, j));
                var localPos = point - localToWorld.Position;
                localPos = math.mul(math.inverse(localToWorld.Rotation), localPos);
                if (aabb.Min.x - distance > localPos.x) continue;
                if (aabb.Min.z - distance > localPos.z) continue;
                if (aabb.Max.x + distance < localPos.x) continue;
                if (aabb.Max.z + distance < localPos.z) continue;

                if (collider.Value.Value.CalculateDistance(new PointDistanceInput()
                {
                    Position = localPos,
                    MaxDistance = float.MaxValue,
                    Filter = CollisionFilter.Default
                }, out DistanceHit hit))
                {
                    if (distance - hit.Distance > 0f)
                        densityMatrix[DensitySystem.Index(i, j)] += math.max(0f, distance - hit.Distance);
                }
            }
    }
}