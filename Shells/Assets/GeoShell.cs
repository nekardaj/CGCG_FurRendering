using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeoShell : MonoBehaviour
{
    [SerializeField] private string _materialPath = "Materials/FurMaterial";

    [Tooltip("If set to false, the object will not move on user input.")]
    public bool AllowObjectMovement = true;

    [Header("Parameters")]

    [Range(1, 256)] public int shellCount = 16;

    [Range(0.0f, 1.0f)] public float shellLength = 0.15f;

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

    [SerializeField] private DisplacementDirectionProvider displacementDirectionProvider;

    private Material Material;
    private Mesh mesh { get; set; }
    private Material shellMaterial;


    private MeshRenderer meshRenderer;

    

    // TODO: We could be doing this via tesselation shaders somehow? Again a comparison in a report would be nice!
    void OnEnable()
    {
        SetMaterial();
        SetMesh();
    }

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
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

    void Update()
    {
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
        //Debug.Log("Loaded material: " + shellMaterial.name + " from path: " + _materialPath);
        meshRenderer.material = shellMaterial;
    }

    private void SetMesh()
    {
        mesh = GetComponent<MeshFilter>().mesh;
    }

    private void SetMovement()
    {
        if (!AllowObjectMovement || displacementDirectionProvider == null) 
        {
            meshRenderer.material.SetVector("_ShellDisplacementDir", Vector4.zero);
            return;
        }
        meshRenderer.material.SetVector("_ShellDisplacementDir", displacementDirectionProvider.GetDisplacementDirection());
    }

    private void UpdateUniforms(int index)
    {
        meshRenderer.material.SetInt("_ShellCount", shellCount);
        meshRenderer.material.SetFloat("_TotalShellLength", shellLength);
        meshRenderer.material.SetFloat("_ShellDensity", density);
        meshRenderer.material.SetFloat("_Thickness", thickness);
        meshRenderer.material.SetFloat("_Attenuation", occlusionAttenuation);
        meshRenderer.material.SetFloat("_ShellDistanceAttenuation", distanceAttenuation);
        meshRenderer.material.SetFloat("_Curvature", curvature);
        meshRenderer.material.SetFloat("_DisplacementStrength", displacementStrength);
        meshRenderer.material.SetFloat("_OcclusionBias", occlusionBias);
        meshRenderer.material.SetFloat("_MinNormalizedLength", noiseMin);
        meshRenderer.material.SetFloat("_MinNormalizedLength", noiseMax);
        meshRenderer.material.SetVector("_ShellColor", shellColor);
    }
}