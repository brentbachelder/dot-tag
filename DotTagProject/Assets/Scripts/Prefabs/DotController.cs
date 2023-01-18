using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotController : MonoBehaviour
{
	[SerializeField] SpriteRenderer TheDot;
	[SerializeField] GameObject Life1;
	[SerializeField] GameObject Life2;
	[SerializeField] GameObject Life3;

	[Space]

	// Sent from Intialize Settings in GameController
	[HideInInspector] public bool isMyDot = false;
	[HideInInspector] public Color color;
	[HideInInspector] public string ID;
	public float moveSpeed;
	public float rotationSpeed;
	[HideInInspector] public int GameType;

	public int isIt;
	public int lives;

	public Vector2 GoalPosition;
	Vector2 PreviousPosition;
	Vector2 Direction;
	[HideInInspector] public bool GetDirection = false;
	[HideInInspector] public bool GetInitialDirection = false;

	
	public int previousIt = 0;
	int previousLives = 0;

	void Start()
	{
		TheDot.color = color;
	}

	void Update()
	{
		if(isIt == 1) TheDot.color = Color.red;
		else TheDot.color = color;
		/*if(isIt != previousIt)
		{
			Debug.Log("Hit previous it");
			previousIt = isIt;
			if(isIt == 1) ImIt();
			else ImNotIt();
		}
		if(lives != previousLives && GameType == 0)
		{
			previousLives = lives;
			AdjustLives();
		}*/
	}

	public void ImIt(bool noAnimation = false)
	{
		if(lives == 0) Debug.Log("You are dead");
		else
		{
			// Need to create animation or something when getting it
			TheDot.color = Color.red;
		}
	}

	void ImNotIt()
	{
		TheDot.color = color;
	}

	void AdjustLives()
	{
		Life1.SetActive(false);
		Life2.SetActive(false);
		Life3.SetActive(false);

		if(lives == 2) Life3.SetActive(true);
		if(lives == 1)
		{
			Life1.SetActive(true);
			Life2.SetActive(true);
		}
	}

	void OnCollisionEnter2D(Collision2D collision)
	{
		if(collision.transform.tag == "Dot")
		{
			Physics2D.IgnoreCollision(collision.gameObject.GetComponent<CircleCollider2D>(), GetComponent<CircleCollider2D>());
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		//if(collision.transform.tag == "Dot" && isMyDot) Debug.Log("Ran into dot " + collision.gameObject.GetComponent<DotController>().ID);
	}

	void FixedUpdate()
	{
		Vector2 CurrentPosition = GetComponent<Rigidbody2D>().position;
		if(GetInitialDirection)
		{
			Direction = (new Vector2(0, 0) - CurrentPosition).normalized;
			Quaternion rotation = Quaternion.LookRotation(Vector3.forward, Direction);
			transform.rotation = rotation;
		}
		else
		{
			if(GetDirection)
			{
				if(GoalPosition != CurrentPosition) Direction = (GoalPosition - CurrentPosition).normalized;
				GetDirection = false;
			}
			Quaternion rotation = Quaternion.LookRotation(Vector3.forward, Direction);
			//transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, rotationSpeed);
			transform.rotation = rotation;
		}

		if(Vector2.Distance(CurrentPosition, GoalPosition) < moveSpeed) GetComponent<Rigidbody2D>().MovePosition(GoalPosition);
		else GetComponent<Rigidbody2D>().MovePosition(CurrentPosition + Direction * moveSpeed);
	}
}
