﻿using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Systems
{
    [AlwaysSynchronizeSystem]
    [UpdateAfter(typeof(WalkingSystem))]
    class LoggingSystem : ComponentSystem
    {
        private int db = 0;
        private readonly float period = 30;

        private static readonly int maxResult = 12;

        NativeArray<float> Avarage;

        protected override void OnCreate()
        {
            base.OnCreate();
            Avarage = new NativeArray<float>(maxResult, Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            Avarage.Dispose();
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;

            NativeArray<float> result = new NativeArray<float>(maxResult, Allocator.TempJob);
            Clear(result);

            Entities.ForEach((ref Walker walker, ref PathFindingData data, ref Translation tr) =>
            {
                var length = math.length(data.decidedGoal - tr.Value);
                result[0] += 1f;

                if (data.pathFindingMethod == PathFindingMethod.DensityGrid)
                {
                    result[1] += math.length(walker.direction);
                    result[3] += math.max(0f, length - data.radius);
                }
                if (data.pathFindingMethod == PathFindingMethod.Forces)
                {
                    result[2] += math.length(walker.direction);
                    result[4] += math.max(0f, length - data.radius);
                }
            });
            if (result[0] > 0)
            {
                for (int i=1; i<maxResult; i++)
                {
                    Avarage[i] += (result[i] / result[0]) / period;
                }
                db++;
                if (db % period == 0)
                {
                    for (int i = 1; i < maxResult; i++)
                    {
                        Logger.Log(Avarage[i].ToString("N3"));
                    }
                    Clear(Avarage);
                }
            }
            result.Dispose();
        }

        void Clear(NativeArray<float> array)
        {
            for (int i = 0; i < maxResult; i++)
            {
                array[i] = 0f;
            }
        }
    }
}
