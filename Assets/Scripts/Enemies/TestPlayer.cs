using UnityEngine;

public class TestPlayer : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    private void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            _rigidbody.AddForce(Vector3.forward * 10f);
        }
        if (Input.GetKey(KeyCode.S))
        {
            _rigidbody.AddForce(Vector3.back * 10f);
        }
        if (Input.GetKey(KeyCode.A))
        {
            _rigidbody.AddForce(Vector3.left * 10f);
        }
        if (Input.GetKey(KeyCode.D))
        {
            _rigidbody.AddForce(Vector3.right * 10f);
        }

        if (_rigidbody.linearVelocity.magnitude > 5f)
        {
            _rigidbody.linearVelocity = _rigidbody.linearVelocity.normalized * 5f;
        }
    }
}
