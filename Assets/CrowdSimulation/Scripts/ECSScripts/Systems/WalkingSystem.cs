﻿using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Rendering;
using Assets.CrowdSimulation.Scripts.ECSScripts.Jobs;
using Unity.Mathematics;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Systems
{
    [AlwaysSynchronizeSystem]
    [UpdateAfter(typeof(PathFindingSystem))]
    [UpdateAfter(typeof(CollisionSystem))]
    public class WalkingSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var deltaTime = math.min(Time.DeltaTime, 0.05f);

            var forceJob = new ForceJob() { deltaTime = deltaTime };
            var forceHandle = forceJob.Schedule(this);

            var walkerJob = new WalkerJob() { deltaTime = deltaTime, maxWidth = Map.MaxWidth, maxHeight = Map.MaxHeight };
            var walkerHandle = walkerJob.Schedule(this, forceHandle);
            walkerHandle.Complete();
        }
    }
}
