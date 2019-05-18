using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(TransformSystemGroup))]
[UpdateAfter(typeof(EndFrameTRSToLocalToWorldSystem))]
public class ScaleSystem : JobComponentSystem
{
    EntityQuery entitiesScaleGroup;

    protected override void OnCreate()
    {
        entitiesScaleGroup = GetEntityQuery(new EntityQueryDesc
        {
            All = new[] {
                ComponentType.ReadOnly<Scale>(),
                ComponentType.ReadOnly<LocalToWorld>()
            },
        });
    }

    [BurstCompile]
    struct ScaleJob : IJobForEachWithEntity<Scale, LocalToWorld>
    {
        public void Execute(Entity entity, int index, [ReadOnly] ref Scale scale, ref LocalToWorld matrice)
        {
            float4x4 baseMat = matrice.Value;
            float4x4 scaleMat = float4x4.Scale(scale.Value);
            float4x4 newMat = math.mul(baseMat, scaleMat);

            matrice = new LocalToWorld
            {
                Value = newMat
            };
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new ScaleJob();

        JobHandle jobHandler = job.Schedule(entitiesScaleGroup, inputDeps);
        entitiesScaleGroup.AddDependency(inputDeps);

        return jobHandler;
    }
}