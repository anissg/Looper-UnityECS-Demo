using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(TransformSystem))]
public class ScaleSystem : JobComponentSystem
{
    struct ScaleGroup
    {
        [ReadOnly] public ComponentDataArray<Scale> Scales;
        public ComponentDataArray<TransformMatrix> Matrices;
        public readonly int Length;
    }

    [Inject] private ScaleGroup entities;

    [BurstCompile]
    struct ScaleJob : IJobParallelFor
    {
        [ReadOnly] public ComponentDataArray<Scale> Scales;
        public ComponentDataArray<TransformMatrix> Matrices;

        public void Execute(int index)
        {
            float4x4 baseMat = Matrices[index].Value;
            float4x4 scaleMat = float4x4.scale(Scales[index].Value);
            float4x4 newMat = math.mul(baseMat, scaleMat);

            Matrices[index] = new TransformMatrix { Value = newMat };
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new ScaleJob()
        {
            Scales = entities.Scales,
            Matrices = entities.Matrices
        };
        return job.Schedule(entities.Length, 64, inputDeps);
    }
}