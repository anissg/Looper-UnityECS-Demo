using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public struct CustomMeshInstanceRenderer : ISharedComponentData
{
    public Mesh mesh;
    public Material material;
    public int subMesh;

    public ShadowCastingMode castShadows;
    public bool receiveShadows;
}

public class CustomMeshInstanceRendererComponent : SharedComponentDataWrapper<CustomMeshInstanceRenderer> { }
