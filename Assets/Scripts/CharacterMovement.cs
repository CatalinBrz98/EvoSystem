using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    [SerializeField]
    private float speed = 6f, gravity = 19.62f;
    private CharacterController controller;
    private Vector3 velocity, targetPosition, initialPosition;
    private bool isGrounded;
    private string action = "Staying";
    private GameObject targetObject;

    void Awake()
    {
        initialPosition = transform.position;
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        switch (action)
        {
            case "GoToObject":
                GoToObject();
                break;
            case "GoToPosition":
                GoToPosition();
                break;
            case "TeleportToPosition":
                TeleportToPosition();
                break;
            default:
                break;
        }
    }

    public void SetAction(string newAction = "Staying", GameObject newObject = null, Vector3 newPosition = default(Vector3))
    {
        action = newAction;
        targetObject = newObject;
        targetPosition = newPosition;
    }

    public string GetAction()
    {
        return action;
    }

    public Vector3 GetInitialPosition()
    {
        return initialPosition;
    }

    void GoToObject()
    {
        Vector3 targetObjectPostition = new Vector3(targetObject.transform.position.x, this.transform.position.y, targetObject.transform.position.z);
        transform.LookAt(targetObjectPostition);

        isGrounded = (controller.collisionFlags & CollisionFlags.Below) != 0;
        if (isGrounded && velocity.y < 0)
            velocity.y = 0f;
        Vector3 movement = transform.forward;
        controller.Move(movement * speed * Time.fixedDeltaTime);
        velocity.y -= gravity * Time.deltaTime * Time.fixedDeltaTime;
        controller.Move(velocity);

        if (Vector3.Distance(transform.position, targetObject.transform.position) < 1)
            SetAction();
    }

    void GoToPosition()
    {
        transform.LookAt(targetPosition);

        isGrounded = (controller.collisionFlags & CollisionFlags.Below) != 0;
        if (isGrounded && velocity.y < 0)
            velocity.y = 0f;
        Vector3 movement = transform.forward;
        controller.Move(movement * speed * Time.fixedDeltaTime);
        velocity.y -= gravity * Time.deltaTime * Time.fixedDeltaTime;
        controller.Move(velocity);

        if (Vector3.Distance(transform.position, targetPosition) < 0.2)
        {
            transform.position = targetPosition;
            if (targetObject)
            {
                Vector3 targetObjectPosition = new Vector3(targetObject.transform.position.x, this.transform.position.y, targetObject.transform.position.z);
                transform.LookAt(targetObjectPosition);
            }
            SetAction();
        }
    }

    void TeleportToPosition()
    {
        transform.LookAt(targetPosition);
        transform.position = targetPosition;
        if (targetObject)
        {
            Vector3 targetObjectPosition = new Vector3(targetObject.transform.position.x, this.transform.position.y, targetObject.transform.position.z);
            transform.LookAt(targetObjectPosition);
        }
        SetAction();
    }
}
