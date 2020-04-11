﻿using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[UpdateAfter(typeof(FightersHashMap))]
class FighterSystem : ComponentSystem
{
    public static NativeMultiHashMap<int, Fighter> hashMap;
    protected override void OnCreate()
    {
        hashMap = new NativeMultiHashMap<int, Fighter>(0, Allocator.Persistent);
        base.OnCreate();
    }

    protected override void OnDestroy()
    {
        hashMap.Dispose();
        base.OnDestroy();
    }

    [BurstCompile]
    public struct SetHashMapJob : IJobForEachWithEntity<Fighter>
    {
        public NativeMultiHashMap<int, Fighter>.ParallelWriter hashMap;

        public void Execute(Entity entity, int index, ref Fighter fighter)
        {
            int key = entity.Index;
            hashMap.Add(key, fighter);
        }
    }

    protected override void OnUpdate()
    {

        var job = new FighterJob()
        {
            targetMap = FightersHashMap.quadrantHashMap
        };
        var handle = job.Schedule(this);
        handle.Complete();
        var hJob = new HurtingJob()
        {
            targetMap = FightersHashMap.quadrantHashMap,
            deltaTime = Time.DeltaTime,
        };
        var hHandle = hJob.Schedule(this);
        hHandle.Complete();

        EntityQuery entityQuery = GetEntityQuery(typeof(Fighter));
        hashMap.Clear();
        if (entityQuery.CalculateEntityCount() > hashMap.Capacity)
        {
            hashMap.Capacity = entityQuery.CalculateEntityCount();
        }

        var hashJob = new SetHashMapJob()
        {
            hashMap = hashMap.AsParallelWriter(),
        };

        var hashHandle = JobForEachExtensions.Schedule(hashJob, entityQuery);
        hashHandle.Complete();

    }
}
