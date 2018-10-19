using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (PlayerMovement))]

public class PlayerControl : MonoBehaviour {

    [Header("Components")]
    public Transform cameraTransform;

    [Header("Parameters")]
    [Range (70f, 200f)] public float mouseSensitivity = 120f;


    private PlayerMovement playerMovement;
    private bool[] playerInput;
    

    private enum input { w = 0, a, s, d, jump };
    private Vector3 camOffset;


	void Start ()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerInput = new bool[5];
        camOffset = new Vector3(
            cameraTransform.position.x - transform.position.x,
            cameraTransform.position.y - transform.position.y,
            cameraTransform.position.z - transform.position.z);
    }
	

	void Update ()
    {
        
	}

    private void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        bool crouch = Input.GetKey(KeyCode.C);
        bool jump = Input.GetKey(KeyCode.Space);
        bool run = Input.GetKey(KeyCode.LeftShift);

        playerMovement.Move(h, v, crouch, jump, run);
    }


    private void LateUpdate()
    {
        cameraTransform.position = transform.position + camOffset;

        float moveFactor = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        if(!Mathf.Approximately(moveFactor, 0f))
        {
            cameraTransform.RotateAround(transform.position, Vector3.up, moveFactor);
            transform.rotation = cameraTransform.rotation;
            camOffset = cameraTransform.position - transform.position;
        }
    }
}
