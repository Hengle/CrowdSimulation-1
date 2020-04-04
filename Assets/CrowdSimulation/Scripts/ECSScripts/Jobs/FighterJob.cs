﻿using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public struct FighterJob : IJobForEach<Fighter, DecidedForce, Translation, Walker>
{
    [NativeDisableParallelForRestriction]
    [ReadOnly]
    public NativeMultiHashMap<int, FightersHashMap.MyData> targetMap;

    public void Execute(ref Fighter fighter, ref DecidedForce decidedForce, [ReadOnly] ref Translation translation, ref Walker walker)
    {
        if (fighter.state == FightState.Rest)
        {
            Rest(fighter, translation, ref decidedForce);
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
                decidedForce.force = float3.zero;
                fighter.state = FightState.Fight;
                walker.direction = direction;
                return;
            }
        }
        decidedForce.force = fighter.targetGroupPos - translation.Value;
        fighter.state = FightState.GoToFight;
    }

    private void Rest(Fighter fighter, Translation translation, ref DecidedForce decidedForce)
    {
        var force = (fighter.restPos - translation.Value);
        if (math.length(force) > fighter.restRadius)
        {
            if (math.length(force) > 1f)
            {
                force = math.normalize(force);
            }
            decidedForce.force = force;
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
