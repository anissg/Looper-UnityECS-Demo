using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

[UpdateAfter(typeof(PreLateUpdate.ParticleSystemBeginUpdateAll))]
[UpdateAfter(typeof(MeshCullingBarrier))]
[ExecuteInEditMode]
public class CustomMeshInstanceRendererSystem : ComponentSystem
{
    Matrix4x4[] m_MatricesArray = new Matrix4x4[1023];
    float[] m_HueArray = new float[1023];
    List<CustomMeshInstanceRenderer> m_CacheduniqueRendererTypes = new List<CustomMeshInstanceRenderer>(10);
    ComponentGroup m_InstanceRendererGroup;
    
    public unsafe static void CopyMatrices(ComponentDataArray<TransformMatrix> transforms, int beginIndex, int length, Matrix4x4[] outMatrices)
    {
        fixed (Matrix4x4* matricesPtr = outMatrices)
        {
            var matricesSlice = NativeSliceUnsafeUtility.ConvertExistingDataToNativeSlice<TransformMatrix>(matricesPtr, sizeof(Matrix4x4), length);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeSliceUnsafeUtility.SetAtomicSafetyHandle(ref matricesSlice, AtomicSafetyHandle.GetTempUnsafePtrSliceHandle());
#endif
            transforms.CopyTo(matricesSlice, beginIndex);
        }
    }

    public unsafe static void CopyArray(ComponentDataArray<Tint> tints, int beginIndex, int length, float[] outParams)
    {
        fixed (float* matricesPtr = outParams)
        {
            var paramSlice = NativeSliceUnsafeUtility.ConvertExistingDataToNativeSlice<Tint>(matricesPtr, sizeof(float), length);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeSliceUnsafeUtility.SetAtomicSafetyHandle(ref paramSlice, AtomicSafetyHandle.GetTempUnsafePtrSliceHandle());
#endif
            tints.CopyTo(paramSlice, beginIndex);
        }
    }

    protected override void OnCreateManager(int capacity)
    {
        m_InstanceRendererGroup = GetComponentGroup(
            typeof(CustomMeshInstanceRenderer), 
            typeof(TransformMatrix), 
            typeof(Tint),
            ComponentType.Subtractive<MeshCulledComponent>(), 
            ComponentType.Subtractive<MeshLODInactive>());
    }

    protected override void OnUpdate()
    {
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();

        EntityManager.GetAllUniqueSharedComponentDatas(m_CacheduniqueRendererTypes);
        var forEachFilter = m_InstanceRendererGroup.CreateForEachFilter(m_CacheduniqueRendererTypes);

        for (int i = 0; i != m_CacheduniqueRendererTypes.Count; i++)
        {
            var renderer = m_CacheduniqueRendererTypes[i];
            var transforms = m_InstanceRendererGroup.GetComponentDataArray<TransformMatrix>(forEachFilter, i);
            var renderparam = m_InstanceRendererGroup.GetComponentDataArray<Tint>(forEachFilter, i);

            int beginIndex = 0;
            while (beginIndex < transforms.Length)
            {
                int length = math.min(m_MatricesArray.Length, transforms.Length - beginIndex);
                CopyMatrices(transforms, beginIndex, length, m_MatricesArray);
                CopyArray(renderparam, beginIndex, length, m_HueArray);
                mpb.SetFloatArray("_Hue", m_HueArray);
                Graphics.DrawMeshInstanced(renderer.mesh, renderer.subMesh, renderer.material, m_MatricesArray, length, mpb, renderer.castShadows, renderer.receiveShadows);

                beginIndex += length;
            }
        }

        m_CacheduniqueRendererTypes.Clear();
        forEachFilter.Dispose();
    }
}