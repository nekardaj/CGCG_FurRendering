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

    /// In order: WSAD, up and down (Default will respect previous implementation)
    /// </summary>
    [SerializeField] private KeyCode[] m_MovementKeys = new KeyCode[] { KeyCode.W, KeyCode.S, KeyCode.A, KeyCode.D, KeyCode.E, KeyCode.Q };

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
        if (Input.GetKey(m_MovementKeys[0]))
        {
            direction += Vector3.forward;
        }
        if (Input.GetKey(m_MovementKeys[1]))
        {
            direction += Vector3.back;
        }
        if (Input.GetKey(m_MovementKeys[2]))
        {
            direction += Vector3.left;
        }
        if (Input.GetKey(m_MovementKeys[3]))
        {
            direction += Vector3.right;
        }
        if (Input.GetKey(m_MovementKeys[4]))
        {
            direction += Vector3.down;
        }
        if (Input.GetKey(m_MovementKeys[5]))
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
