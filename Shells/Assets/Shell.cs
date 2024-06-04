using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : MonoBehaviour
{
    [SerializeField] private string _materialPath = "Materials/FurMaterial";

    [Tooltip("If set to false, the object will not move on user input.")]
    public bool AllowObjectMovement = true;

    [Header("Parameters")]

    [Range(1, 256)] public int shellCount = 16;

    [Range(0.0f, 4.0f)] public float shellLength = 0.15f;

    [Range(0.01f, 3.0f)] public float distanceAttenuation = 1.0f;

    [Range(1.0f, 5000.0f)] public float density = 100.0f;

    [Range(0.0f, 1.0f)] public float noiseMin = 0.0f;

    [Range(0.0f, 1.0f)] public float noiseMax = 1.0f;

    [Range(0.0f, 10.0f)] public float thickness = 1.0f;

    [Range(0.0f, 10.0f)] public float curvature = 1.0f;

    [Range(0.0f, 1.0f)] public float displacementStrength = 0.1f;

    public Color shellColor;

    [Range(0.0f, 5.0f)] public float occlusionAttenuation = 1.0f;

    [Range(0.0f, 1.0f)] public float occlusionBias = 0.0f;

    public Texture2D texture;

    [SerializeField] private DisplacementDirectionProvider displacementDirectionProvider;

    public SkinnedMeshRenderer skinnedMeshRenderer;

    private Material Material;
    private Mesh mesh { get; set; }
    private Material shellMaterial;
    private GameObject[] shells;
    private int shellsCapacity = 0;

    

    // TODO: We could be doing this via tesselation shaders somehow? Again a comparison in a report would be nice!
    void OnEnable()
    {
        SetMaterial();
        SetMesh();
        GenerateShells();
    }

    void Start()
    {
        if (displacementDirectionProvider == null)
        {
            if (GetComponentInParent<MovementController>() != null)
            {
                Debug.LogWarning("Found MovementController which is not bound. Did you forget to assign displacement provider?");
            }
            if (displacementDirectionProvider == null)
            {
                Debug.Log("No displacement direction provider found. Assuming movement is locked");
            }
        }
    }

    void OnDisable()
    {
        CleanShells();
        shells = null;
    }

    void Update()
    {
        
        
        if (shellsCapacity != shellCount)
        {
            CleanShells();
            GenerateShells();
        }

        SetMovement();

        // TODO: update only changed stuff - we could meassure performance without having this done and then with having it done and include it in the report!!
        for (var i = 0; i < shellCount; ++i)
        {
            UpdateUniforms(i);
        }
    }

    private void SetMaterial()
    {
        shellMaterial = Resources.Load<Material>(_materialPath);
        GetComponent<SkinnedMeshRenderer>().material = shellMaterial;
    }

    private void SetMesh()
    {
        mesh = GetComponent<SkinnedMeshRenderer>().sharedMesh;
    }

    private void GenerateShells()
    {
        // TODO: generated shells might have Cast Shadows to off for performance boost
        // ^^ discuss with Vlada when he is done with shader experiments

        shells = new GameObject[shellCount];
        shellsCapacity = shellCount;

        for (var i = 0; i < shellCount; ++i)
        {
            shells[i] = new GameObject("Shell " + i);
            var meshFilter = shells[i].AddComponent<MeshFilter>();
            var meshRenderer = shells[i].AddComponent<MeshRenderer>();

            meshFilter.mesh = mesh;
            meshRenderer.material = shellMaterial;
            shells[i].transform.SetParent(transform, false);
        }
    }

    private void SetMovement()
    {
        if (!AllowObjectMovement || displacementDirectionProvider == null) 
        {
            for (int i = 0; i < shells.Length; i++)
            {
                shells[i].GetComponent<MeshRenderer>().material.SetVector("_ShellDisplacementDir", Vector4.zero);
            }
            return;
        }
        for (int i = 0; i < shells.Length; i++)
        {
            shells[i].GetComponent<MeshRenderer>().material.SetVector("_ShellDisplacementDir", displacementDirectionProvider.GetDisplacementDirection());
        }
    }

    private void UpdateUniforms(int index)
    {
        shells[index].GetComponent<MeshRenderer>().material.SetInt("_ShellCount", shellCount);
        shells[index].GetComponent<MeshRenderer>().material.SetInt("_ShellIndex", index);
        shells[index].GetComponent<MeshRenderer>().material.SetFloat("_TotalShellLength", shellLength);
        shells[index].GetComponent<MeshRenderer>().material.SetFloat("_ShellDensity", density);
        shells[index].GetComponent<MeshRenderer>().material.SetFloat("_Thickness", thickness);
        shells[index].GetComponent<MeshRenderer>().material.SetFloat("_Attenuation", occlusionAttenuation);
        shells[index].GetComponent<MeshRenderer>().material.SetFloat("_ShellDistanceAttenuation", distanceAttenuation);
        shells[index].GetComponent<MeshRenderer>().material.SetFloat("_Curvature", curvature);
        shells[index].GetComponent<MeshRenderer>().material.SetFloat("_DisplacementStrength", displacementStrength);
        shells[index].GetComponent<MeshRenderer>().material.SetFloat("_OcclusionBias", occlusionBias);
        shells[index].GetComponent<MeshRenderer>().material.SetFloat("_MinNormalizedLength", noiseMin);
        shells[index].GetComponent<MeshRenderer>().material.SetFloat("_MinNormalizedLength", noiseMax);
        shells[index].GetComponent<MeshRenderer>().material.SetVector("_ShellColor", shellColor);
        shells[index].GetComponent<MeshRenderer>().material.SetTexture("_MainTex", texture);
    }

    private void CleanShells()
    {
        foreach (var shell in shells)
        {
            Destroy(shell);
        }
    }
}