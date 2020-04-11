﻿using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public struct FighterJob : IJobForEach<Fighter, DecidedForce, Translation, Rotation>
{
    [NativeDisableParallelForRestriction]
    [ReadOnly]
    public NativeMultiHashMap<int, FightersHashMap.MyData> targetMap;

    public void Execute(ref Fighter fighter, ref DecidedForce decidedForce, [ReadOnly] ref Translation translation, ref Rotation walker)
    {
        if (fighter.state == FightState.Rest)
        {
            GetNear(fighter.restPos, fighter.restRadius, translation, ref decidedForce);
            return;
        }
        var selected = new FightersHashMap.MyData();
        var found = ForeachAround(translation.Value, ref selected, fighter.targerGroupId);

        fighter.targetId = -1;
        if (found)
        {
            var direction = selected.position - translation.Value;
            fighter.targetId = selected.data.Id;

            if (math.length(direction) < fighter.attackRadius)
            {
                decidedForce.force = direction * 0.1f;
                RotateForward(direction, ref walker);
                fighter.state = FightState.Fight;
                return;
            }
            decidedForce.force = direction * 0.5f;
        }
        GetNear(fighter.targetGroupPos, fighter.restRadius, translation, ref decidedForce);
        fighter.state = FightState.GoToFight;
    }

    private void RotateForward(float3 direction, ref Rotation rotation)
    {
        var speed = math.length(direction);

        if (speed > 0.1f)
        {
            var toward = quaternion.LookRotationSafe(direction, new float3(0, 1, 0));
            rotation.Value = toward;
        }
    }

    private void GetNear(float3 goal, float radius, Translation translation, ref DecidedForce decidedForce)
    {
        var force = (goal - translation.Value);
        if (math.length(force) > radius)
        {
            if (math.length(force) > 1f)
            {
                force = math.normalizesafe(force);
            }
            decidedForce.force = force * 0.5f;
        }
        else
        {
            decidedForce.force = float3.zero;
        }
    }

    private bool ForeachAround(float3 position, ref FightersHashMap.MyData output, int targetId)
    {
        var found = false;
        var key = QuadrantVariables.GetPositionHashMapKey(position);
        found = found || Foreach(key, position, ref output, found, targetId);
        key = QuadrantVariables.GetPositionHashMapKey(position, new float3(1, 0, 0));
        found = found || Foreach(key, position, ref output, found, targetId);
        key = QuadrantVariables.GetPositionHashMapKey(position, new float3(-1, 0, 0));
        found = found || Foreach(key, position, ref output, found, targetId);
        key = QuadrantVariables.GetPositionHashMapKey(position, new float3(0, 0, 1));
        found = found || Foreach(key, position, ref output, found, targetId);
        key = QuadrantVariables.GetPositionHashMapKey(position, new float3(0, 0, -1));
        found = found || Foreach(key, position, ref output, found, targetId);
        return found;
    }

    private bool Foreach(int key, float3 position, ref FightersHashMap.MyData output, bool found, int targetId)
    {
        if (targetMap.TryGetFirstValue(key, out FightersHashMap.MyData other, out NativeMultiHashMapIterator<int> iterator))
        {
            do
            {
                if (other.data2.broId != targetId) continue;
                if (!found)
                {
                    output = other;
                    found = true;
                }
                else
                {
                    var prevDist = math.length(output.position - position);
                    var nowDistance = math.length(other.position - position);
                    if (prevDist > nowDistance)
                    {
                        output = other;
                    }
                }

            } while (targetMap.TryGetNextValue(out other, ref iterator));
        }
        return found;
    }
}