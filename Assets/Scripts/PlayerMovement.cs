using UnityEngine;
using UnityEngine.Windows;
using static UnityEngine.InputSystem.InputAction;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private float _groundDrag = 5f;
    [SerializeField] private float _airDrag = 2f;
    [SerializeField] private float _groundDist = 0.4f;

    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private Transform _groundCheck;

    private Rigidbody _rb;
    private bool _isGrounded;
    private Vector3 _moveDirection;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        _isGrounded = Physics.CheckSphere(_groundCheck.position, _groundDist, _groundLayer);
        Vector3 targetHorizontalVelocity = new Vector3(_moveDirection.x * _moveSpeed, 0f, _moveDirection.z * _moveSpeed);
        _rb.linearVelocity = new Vector3(targetHorizontalVelocity.x, _rb.linearVelocity.y, targetHorizontalVelocity.z);
        _rb.linearDamping = _isGrounded ? _groundDrag : _airDrag;
    }

    public void OnMove(CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        _moveDirection = new Vector3(input.x, 0f, input.y);
    }

    public void Jump(CallbackContext context)
    {
        if(context.performed && _isGrounded)
        {
            _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
            _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        }
        
    }
}
