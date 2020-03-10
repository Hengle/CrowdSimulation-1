﻿using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;

[AlwaysSynchronizeSystem]
[UpdateAfter(typeof(DensitySystem))]
[UpdateAfter(typeof(GoalSystem))]
[UpdateAfter(typeof(QuadrantSystem))]
public class PathFindingSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var pathFindingJob = new PathFindingJob() {
            targetMap = QuadrantSystem.quadrantHashMap,
        };
        var denistyAvoidanceJob = new DensityAvoidanceJob()
        {
            densityMap = DensitySystem.densityMatrix,
        };
        var handle = denistyAvoidanceJob.Schedule(this, inputDeps);

        return handle;
    }
}
