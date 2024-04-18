using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObjectTransform : MonoBehaviour
{
    [Tooltip("The object to copy transform (pos, rot, scale)")]
    public Transform Target;
    [Tooltip("The scale multiplier to apply to the target scale")]
    [SerializeField] private float _scaleMultiplier = 1f;

    // TODO: does not react to when Target id being (de)activated

    private void Start()
    {
        GetComponent<MeshRenderer>().enabled = true;
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
