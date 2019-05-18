using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct Scale : IComponentData
{
    public float3 Value;
}