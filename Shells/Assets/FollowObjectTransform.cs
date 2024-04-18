using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObjectTransform : MonoBehaviour
{
    [Tooltip("The object to copy transform (pos, rot, scale)")]
    public Transform Target;
    [HideInInspector]
    [Tooltip("The scale multiplier to apply to the target scale")]
    [SerializeField] private float _scaleMultiplier = 1f;
    [Tooltip("The distance to move the verticies.")]
    [SerializeField] private float _moveDist = 0.01f;
    [SerializeField] private bool _discard = true;

    // TODO: does not react to when Target id being (de)activated

    private void Start()
    {
        MeshRenderer mr = GetComponent<MeshRenderer>();
        mr.enabled = true;
        mr.material.SetFloat("_MoveDist", _moveDist);
        mr.material.SetInt("_Discard", _discard ? 1 : 0);
    }

    private void Update()
    {
        UpdateTransform();
    }

    private void LateUpdate()
    {
        UpdateTransform();
    }

    private void UpdateTransform()
    {
        if (Target != null)
        {
            transform.position = Target.position;
            transform.rotation = Target.rotation;
            transform.localScale = Target.localScale * _scaleMultiplier;
        }
    }
}
