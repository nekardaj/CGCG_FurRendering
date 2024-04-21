using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplacementDirectionAnimator : DisplacementDirectionProvider
{
    // Simple class that returns points in a circle
    [SerializeField] float CycleDuration = 5.0f;
    float time = 0.0f;

    public override Vector3 GetDisplacementDirection()
    {
        float x = Mathf.Cos(time / CycleDuration * 2 * Mathf.PI);
        float z = Mathf.Sin(time / CycleDuration * 2 * Mathf.PI);
        return new Vector3(x, 0, z);
    }
    void Update()
    {
        time += Time.deltaTime;
    }

}
