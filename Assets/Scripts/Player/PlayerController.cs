using System.Collections;
using System.Collections.Generic;
// using System.Numerics;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private int speed;

    private PlayerControls playerControls;
    private Rigidbody rb;
    private Vector3 movement;

    private void Awake() {
        playerControls = new PlayerControls();
    }

    private void OnEnable() {
        playerControls.Enable();
    }

    private void Start() {
        rb = gameObject.GetComponent<Rigidbody>();
    }
    // Update is called once per frame
    void Update()
    {
        float x = playerControls.Player.Move.ReadValue<Vector2>().x;
        float z = playerControls.Player.Move.ReadValue<Vector2>().y;
        Debug.Log(x + ", " + z);

        movement = new Vector3(x,0,z).normalized;
    }
    
    private void FixedUpdate() {
        rb.MovePosition(transform.position + movement * speed * Time.fixedDeltaTime);
    }
}
