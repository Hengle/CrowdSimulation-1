﻿using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public struct SetDensityGridJob : IJobForEachWithEntity<Translation, Walker, CollisionParameters>
{
    [NativeDisableParallelForRestriction]
    public NativeArray<float> quadrantHashMap;

    public void Execute(Entity entity, int index, ref Translation translation, ref Walker walker, ref CollisionParameters collisionParameters)
    {
        float3 pos = translation.Value;
        var step = math.length(DensitySystem.Up);
        var max = collisionParameters.outerRadius * 2;

        for (float i = -max; i < max; i += step)
            for (float j = -max; j < max; j += step)
            {
                Add(pos + DensitySystem.Up * i + DensitySystem.Right * j, pos, max, walker.broId);
            }
    }

    private void Add(float3 position, float3 prev, float maxdistance, int gid)
    {
        var keyDistance = DensitySystem.IndexFromPosition(position, prev);
        if (keyDistance.key < 0)
        {
            return;
        }
        for (int group = 0; group < Map.MaxGroup; group++)
        {
            if (group != gid)
            {
                quadrantHashMap[Map.OneLayer * group + keyDistance.key] += math.max(0f, (maxdistance - keyDistance.distance) / maxdistance);
            }
        }
    }
}
