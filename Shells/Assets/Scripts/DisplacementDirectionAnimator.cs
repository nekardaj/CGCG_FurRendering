using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplacementDirectionAnimator : DisplacementDirectionProvider
{
    // Simple class that returns points in a circle
    [SerializeField] float CycleDuration = 5.0f;
    [Tooltip("If true object will move around in a circle as well")]
    [SerializeField] bool moveObject = false;
    [SerializeField] float radius = 5.0f;
    private Vector3 origin;
    float time = 0.0f;

    private void Start()
    {
        origin = transform.position;
    }
    public override Vector3 GetDisplacementDirection()
    {
        float x = Mathf.Cos(time / CycleDuration * 2 * Mathf.PI);
        float z = Mathf.Sin(time / CycleDuration * 2 * Mathf.PI);
        return new Vector3(x, 0, z);
    }
    void Update()
    {
        time += Time.deltaTime;
        if (moveObject)
        {
            // because velocity is perpendiculat to the displacement direction, displacement direction is the direction of the velocity
            // this means position phase needs to be 90 degrees ahead of the velocity phase
            float x = Mathf.Cos(time / CycleDuration * 2 * Mathf.PI + Mathf.PI / 2);
            float z = Mathf.Sin(time / CycleDuration * 2 * Mathf.PI + Mathf.PI / 2);
            transform.position = origin + new Vector3(x, 0, z) * radius;
        }
    }

}
