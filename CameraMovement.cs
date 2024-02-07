using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform playerTransform;
    public Rigidbody2D playerRigidbody;
    public float followRate = 1;
    public float yDampeningUp = 1;
    public float yDampeningDown = 0.5f;
    public float xDampening = 0.75f;
    public float xOffset = 3.5f;
    public float xDirectionChangePause = 1f;
    public float yDirectionChangePause = 1f;
    
    private float xVelocity = 0.0f;
    private float yVelocity = 0.0f;
    private float _offset;
    private float _yDamp;
    private float prevScale;
    private float prevVelocity;
    private Coroutine _xCoroutine;
    private Coroutine _yCoroutine;

    //smoothly follows player. Follows quicker while falling. Allows for a x offset to show more of the level horizontally while moving that direction. Timers to prevent the camera from snapping around while player is jumping or quickly flipping back and forth horizontally.
    void Update()
    {
        Vector3 currPosition = transform.position;
        if(playerTransform.localScale.x != prevScale)
        {
            _offset = 0; //center char while jumping back and forth
            if(_xCoroutine != null) StopCoroutine(_xCoroutine);
            _xCoroutine = StartCoroutine(xDirectionalPause());
            prevScale = playerTransform.localScale.x;
        }

        float newX = Mathf.SmoothDamp(currPosition.x, playerTransform.position.x + _offset, ref xVelocity, followRate * xDampening);

        if(playerRigidbody.velocity.y != prevVelocity)
        {
            prevVelocity = playerRigidbody.velocity.y;
            if(playerRigidbody.velocity.y <= 0.01f)
            {
                if(_yCoroutine != null) StopCoroutine(_yCoroutine);
                _yCoroutine = StartCoroutine(yDirectionalPause());
            }
            else _yDamp = yDampeningUp;
        }
        float newY = Mathf.SmoothDamp(currPosition.y, playerTransform.position.y, ref yVelocity, followRate * _yDamp);

        transform.position = new Vector3(newX, newY, -10);
    }

    private IEnumerator xDirectionalPause()
    {
        yield return new WaitForSecondsRealtime(xDirectionChangePause);
        _offset = playerTransform.localScale.x < 0 ? -xOffset : xOffset;
    }

    private IEnumerator yDirectionalPause()
    {
        yield return new WaitForSecondsRealtime(yDirectionChangePause);
        _yDamp = yDampeningDown;
    }
}
