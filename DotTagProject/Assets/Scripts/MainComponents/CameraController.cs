using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    [HideInInspector] public float zoom;

	void Start()
	{
        GetComponent<Camera>().orthographicSize = zoom;
    }

	void FixedUpdate()
    {
        if(target)
        {
            transform.position = Vector2.Lerp(transform.position, target.position, 0.1f);
            if(transform.position.z != -10) transform.position = new Vector3(transform.position.x, transform.position.y, -10);
            transform.position = new Vector3(Mathf.Clamp(transform.position.x, -24f, 24f), Mathf.Clamp(transform.position.y, -21f, 21f), -10f);
        }

        if(GetComponent<Camera>().orthographicSize > zoom) GetComponent<Camera>().orthographicSize -= .1f;
        else GetComponent<Camera>().orthographicSize = zoom;
    }

    public IEnumerator SetZoom()
	{
        while(GetComponent<Camera>().orthographicSize != zoom)
		{
            float remaining = zoom - GetComponent<Camera>().orthographicSize;
            float inOut = Mathf.Sign(remaining);
            if(remaining <= .1f) GetComponent<Camera>().orthographicSize = zoom;
            else GetComponent<Camera>().orthographicSize += .1f * inOut;
            yield return 0;
        }
	}

    public void AutoZoom()
	{
        GetComponent<Camera>().orthographicSize = zoom;
    }
}
