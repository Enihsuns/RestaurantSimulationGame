using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Animator))]

public class PlayerMovement : MonoBehaviour {

    [Header("Parameters")]
    [Range(0, 2)] public int maxJump = 2;
    [Range(0f, 5f)] public float groundCheckDistance = 0.1f;
    [Range(0f, 0.2f)] public float runCycleLegOffset = 0.1f;
    [Range(1f, 10f)] public float animSpeedMultiplier = 1f;
    [Range(1f, 10f)] public float moveSpeedMultiplier = 1f;
    [Range(1f, 4f)] public float gravityMultiplier = 2f;
    [Range(5f, 20f)] public float jumpPower = 12f;

    private Rigidbody rigidbody;
    private Animator animator;
    private CapsuleCollider collider;
    private bool isGrounded;
    private bool isCrouching;
    private float origGroundCheckDistance;


	void Start ()
    {
        animator = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<CapsuleCollider>();

        isGrounded = true;
        isCrouching = false;
        origGroundCheckDistance = groundCheckDistance;
	}
	
	void Update ()
    {
		
	}

    /// <summary>
    /// Control the movement of player's transform position.
    /// </summary>
    /// <para>
    /// The transform rotation is not considered since it is the same as the rotation of camera's 
    /// transform.
    /// </para>
    /// <param name="h">Horizontal input.</param>
    /// <param name="v">Vertical input.</param>
    /// <param name="crouch">Crouch or not.</param>
    /// <param name="jump">Jump or not.</param>
    /// <param name="run">Run or not.</param>
    public void Move(float h, float v, bool crouch, bool jump, bool run)
    {
        Vector3 move = (h * transform.forward + v * transform.right) * (run? 2f: 1f);
        if (move.magnitude > 1f) move.Normalize();

        // Update isGrounded value
        CheckGroundStatus();

        // control and velocity handling is different when grounded and airborne:
        if (isGrounded)
        {
            HandleGroundedMovement(crouch, jump);
        }
        else
        {
            HandleAirborneMovement();
        }

        // Update Animator
        UpdateAnimator(move);
    }

    /// <summary>
    /// Handle airborne movement.
    /// </summary>
    /// <para>
    /// Using code in ThirdPersonCharacter.cs.
    /// </para>
    private void HandleAirborneMovement()
    {
        // apply extra gravity from multiplier:
        Vector3 extraGravityForce = (Physics.gravity * gravityMultiplier) - Physics.gravity;
        rigidbody.AddForce(extraGravityForce);

        groundCheckDistance = rigidbody.velocity.y < 0 ? origGroundCheckDistance : 0.01f;
    }

    /// <summary>
    /// Handle movement on ground.
    /// </summary>
    /// <para>
    /// Using code in ThirdPersonCharacter.cs.
    /// </para>
    /// <param name="crouch"></param>
    /// <param name="jump"></param>
    private void HandleGroundedMovement(bool crouch, bool jump)
    {
        // check whether conditions are right to allow a jump:
        if (jump && !crouch && animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded"))
        {
            // jump!
            rigidbody.velocity = new Vector3(rigidbody.velocity.x, jumpPower, rigidbody.velocity.z);
            isGrounded = false;
            animator.applyRootMotion = false;
            groundCheckDistance = 0.1f;
        }
    }

    /// <summary>
    /// Update animator.
    /// </summary>
    /// <para>
    /// Using the code in ThirdPersonCharacter.cs.
    /// </para>
    /// <param name="move">The move vector3.</param>
    void UpdateAnimator(Vector3 move)
    {
        // Calculate forwardAmount and turnAmount
        float forwardAmount = move.z;
        float turnAmount = Mathf.Atan2(move.x, move.z);

        // update the animator parameters
        animator.SetFloat("Forward", forwardAmount, 0.1f, Time.deltaTime);
        animator.SetFloat("Turn", turnAmount, 0.1f, Time.deltaTime);
        animator.SetBool("Crouch", isCrouching);
        animator.SetBool("OnGround", isGrounded);
        if (!isGrounded)
        {
            animator.SetFloat("Jump", rigidbody.velocity.y);
        }

        // calculate which leg is behind, so as to leave that leg trailing in the jump animation
        // (This code is reliant on the specific run cycle offset in our animations,
        // and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
        float runCycle =
            Mathf.Repeat(
                animator.GetCurrentAnimatorStateInfo(0).normalizedTime + runCycleLegOffset, 1);
        float jumpLeg = (runCycle < 0.5f ? 1 : -1) * forwardAmount;
        if (isGrounded)
        {
            animator.SetFloat("JumpLeg", jumpLeg);
        }

        // the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
        // which affects the movement speed because of the root motion.
        if (isGrounded && move.magnitude > 0)
        {
            animator.speed = animSpeedMultiplier;
        }
        else
        {
            // don't use that while airborne
            animator.speed = 1;
        }
    }

    /// <summary>
    /// Check whether the player is on ground.
    /// </summary>
    /// <para>
    /// Check the value of bool isGrounded.
    /// </para>
    private void CheckGroundStatus()
    {
        NavMeshHit hitInfo;
#if UNITY_EDITOR
        Debug.DrawLine(transform.position + (Vector3.up * 0.3f),
            transform.position + Vector3.down);
#endif
        // Using NavMesh Raycast
        if (NavMesh.Raycast(transform.position + (Vector3.up * 0.3f),
            transform.position + Vector3.down, out hitInfo, NavMesh.AllAreas))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }
}
