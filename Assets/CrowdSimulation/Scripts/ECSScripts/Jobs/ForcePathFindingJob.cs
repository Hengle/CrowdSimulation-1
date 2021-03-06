﻿using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Assets.CrowdSimulation.Scripts.ECSScripts.Systems;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Jobs
{
    [BurstCompile]
    public struct ForcePathFindingJob : IJobForEach<PathFindingData, CollisionParameters, Walker, Translation>
    {
        [NativeDisableParallelForRestriction]
        [ReadOnly]
        public NativeMultiHashMap<int, EntitiesHashMap.MyData> targetMap;


        public void Execute([ReadOnly]ref PathFindingData data, 
            [ReadOnly]ref CollisionParameters collisionParameters, ref Walker walker, [ReadOnly]ref Translation translation)
        {
            if (!(data.avoidMethod == CollisionAvoidanceMethod.Forces))
            {
                return;
            }

            var avoidanceForce = float3.zero;
            var convinientForce = float3.zero;
            var bros = 0;
            ForeachAround(new QuadrantData() { direction = walker.direction, position = translation.Value, broId = walker.broId },
                ref avoidanceForce, ref convinientForce, ref bros, collisionParameters.outerRadius);

            var distance = translation.Value - data.decidedGoal;
            if (math.length(distance) < data.radius)
            {
                walker.force = data.decidedForce + avoidanceForce * 0.2f;
                return;
            }

            walker.force = data.decidedForce + avoidanceForce;

            if (bros > 0)
            {
                walker.force += convinientForce *= 1f / bros;
            }
        }

        private void ForeachAround(QuadrantData me, ref float3 avoidanceForce, ref float3 convinientForce, ref int bros, float radius)
        {
            var position = me.position;
            var key = QuadrantVariables.GetPositionHashMapKey(position);
            Foreach(key, me, ref avoidanceForce, ref convinientForce, ref bros, radius);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(1, 0, 0));
            Foreach(key, me, ref avoidanceForce, ref convinientForce, ref bros, radius);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(-1, 0, 0));
            Foreach(key, me, ref avoidanceForce, ref convinientForce, ref bros, radius);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(0, 0, 1));
            Foreach(key, me, ref avoidanceForce, ref convinientForce, ref bros, radius);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(0, 0, -1));
            Foreach(key, me, ref avoidanceForce, ref convinientForce, ref bros, radius);
        }

        private void Foreach(int key, QuadrantData me, ref float3 avoidanceForce, ref float3 convinientForce, ref int bros, float radius)
        {
            if (targetMap.TryGetFirstValue(key, out EntitiesHashMap.MyData other, out NativeMultiHashMapIterator<int> iterator))
            {
                do
                {

                    var direction = me.position - other.position;
                    var distance = math.length(direction);

                    if (me.broId == other.data2.broId && distance < 2 * radius)
                    {
                        convinientForce += other.data2.direction;
                        bros++;
                        distance *= 2f;
                    }

                    var distanceNormalized = (radius - distance) / (radius);

                    if (distanceNormalized > 0f && distanceNormalized < 1f)
                    {
                        var dot = (math.dot(math.normalizesafe(-direction), math.normalizesafe(me.direction)) + 1f) * 0.5f;

                        var forceMultiplyer = math.length(other.data2.direction) + 0.7f;

                        var multiplyer = distanceNormalized * dot * forceMultiplyer;

                        var multiplyerSin = math.sin(multiplyer * math.PI / 2f);

                        avoidanceForce += math.normalizesafe(other.data2.direction) * multiplyerSin;

                        avoidanceForce += direction / radius;
                    }

                } while (targetMap.TryGetNextValue(out other, ref iterator));
            }
        }
    }
}
