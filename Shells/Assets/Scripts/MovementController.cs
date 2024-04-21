using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DisplacementDirectionProvider : MonoBehaviour
{
    /// <summary>
    /// Returns the displacement direction of the object normalized to 0-1
    /// For the provider that is connected to velocity, this should be normalized by maximal velocity
    /// The fur shader can then set its own displacement strength while keeping the same scale
    /// </summary>
    /// <returns></returns>
    public abstract Vector3 GetDisplacementDirection();
}

public class MovementController : DisplacementDirectionProvider
{
    private Rigidbody rb;
    [SerializeField, Range(0.1f, 50f)] private float Acceleration = 5f;
    [SerializeField, Range(0.1f, 100f)] private float MaxSpeed = 6f;
    Vector3 direction;
    private static readonly int ShellDisplacementDir = Shader.PropertyToID("_ShellDisplacementDir");

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody not found");
        }
    }

    Vector3 GetInputTranslationDirection()
    {
        Vector3 direction = new Vector3();
        if (Input.GetKey(KeyCode.W))
        {
            direction += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            direction += Vector3.back;
        }
        if (Input.GetKey(KeyCode.A))
        {
            direction += Vector3.left;
        }
        if (Input.GetKey(KeyCode.D))
        {
            direction += Vector3.right;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            direction += Vector3.down;
        }
        if (Input.GetKey(KeyCode.E))
        {
            direction += Vector3.up;
        }
        return direction.normalized;
    }

    // Update is called once per frame
    void Update()
    {
        direction = GetInputTranslationDirection();
        rb.AddForce(direction * Acceleration,ForceMode.Acceleration);
        // cap the speed
        if (rb.velocity.magnitude > MaxSpeed)
        {
            rb.velocity = rb.velocity.normalized * MaxSpeed;
        }
    }

    public override Vector3 GetDisplacementDirection()
    {
        return (-transform.InverseTransformDirection(rb.velocity)) / MaxSpeed;
    }
}
