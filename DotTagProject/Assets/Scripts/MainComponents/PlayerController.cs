using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] GameController controller;
    [SerializeField] Joystick joystick;
    [HideInInspector] public GameObject myDot;
    
    [Space]

    Vector2 movement;
    Vector2 direction;
    public Vector2 myPosition;

	void FixedUpdate()
	{
        if(myDot != null)
        {
            if(!controller.gameOver && controller.gameStarted)
            {
                movement.x = Input.GetAxisRaw("Horizontal");
                movement.y = Input.GetAxisRaw("Vertical");
                if(movement == Vector2.zero)
                {
                    movement.x = joystick.Horizontal;
                    movement.y = joystick.Vertical;
                }
            }
            direction = new Vector2(movement.x, movement.y).normalized;
            RaycastHit2D hit = Physics2D.Raycast(myPosition, direction, myDot.GetComponent<DotController>().moveSpeed);
            if(hit.collider == null || hit.collider.name != "Background")
            {
                if(movement != Vector2.zero) myPosition = myPosition + direction * myDot.GetComponent<DotController>().moveSpeed;
            }
        }
    }
}
