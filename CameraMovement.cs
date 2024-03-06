using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform playerTransform;
    public Rigidbody2D playerRigidbody;
    public float followRate = 1;
    public float zoomRate = 1;
    public float floatZoomOut = 7;
    public float yDampeningUp = 1;
    public float yDampeningDown = 0.5f;
    public float xDampening = 0.75f;
    public float xOffset = 3.5f;
    public float yOffset = 1f;
    public float xDirectionChangePause = 1f;
    public float yDirectionChangePause = 1f;
    public Vector2 horizontalBounds;
    public Vector2 verticalBounds;

    private PlayerMovement _movement;
    private float fVelocity = 0.0f;
    private float xVelocity = 0.0f;
    private float yVelocity = 0.0f;
    private float _offset;
    private float _yDamp;
    private float prevScale;
    private float prevVelocity;
    private Coroutine _xCoroutine;
    private Coroutine _yCoroutine;

    //smoothly follows player. Follows quicker while falling. Allows for a x offset to show more of the level horizontally while moving that direction. Timers to prevent the camera from snapping around while player is jumping or quickly flipping back and forth horizontally.

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    
    void Start()
    {
        _movement = playerRigidbody.GetComponent<PlayerMovement>();
    }

    void Update()
    {
        //zoom out camera a bit while player is floating
        float newOrthoSize = 5;
        if(_movement.isFloating) newOrthoSize = floatZoomOut;

        Camera.main.orthographicSize = Mathf.SmoothDamp(Camera.main.orthographicSize, newOrthoSize, ref fVelocity, zoomRate);
        //===============================================


        Vector3 currPosition = transform.position;
        if(playerTransform.localScale.x != prevScale) // if player flips around, center character for a bit before applying the horizontal offset. Keeps camera from sliding back and forth while character is flipping rapidly (like during a jump puzzle or a more verticle section)
        {
            _offset = 0; //center char while jumping back and forth
            if(_xCoroutine != null) StopCoroutine(_xCoroutine);
            _xCoroutine = StartCoroutine(xDirectionalPause());
            prevScale = playerTransform.localScale.x;
        }

        float newX = Mathf.SmoothDamp(currPosition.x, playerTransform.position.x + _offset, ref xVelocity, followRate * xDampening);

        float _yOffset = yOffset;
        if(playerRigidbody.velocity.y != prevVelocity)
        {
            prevVelocity = playerRigidbody.velocity.y;
            if(playerRigidbody.velocity.y <= 0.01f) //follow much faster while character is falling
            {
                _yOffset *= -1;
                if(_yCoroutine != null) StopCoroutine(_yCoroutine);
                _yCoroutine = StartCoroutine(yDirectionalPause());
            }
            else _yDamp = yDampeningUp;
        }
        float newY = Mathf.SmoothDamp(currPosition.y, playerTransform.position.y + _yOffset, ref yVelocity, followRate * _yDamp);
        
        //keep camera view within the bounding box
        float vertExtent = Camera.main.orthographicSize;	
    	float horzExtent = vertExtent * Screen.width / Screen.height;

        float minX = horizontalBounds.x + horzExtent;
        float maxX = horizontalBounds.y - horzExtent;
        float minY = verticalBounds.y + vertExtent;
        float maxY = verticalBounds.x - vertExtent;

        transform.position = new Vector3(Mathf.Clamp(newX, minX, maxX), Mathf.Clamp(newY, minY, maxY), -10);
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

    private void OnDrawGizmos()
    {
        Vector3 topLeft = new Vector3(horizontalBounds.x, verticalBounds.x, 0);
        Vector3 topRight = new Vector3(horizontalBounds.y, verticalBounds.x, 0);
        Vector3 bottomLeft = new Vector3(horizontalBounds.x, verticalBounds.y, 0);
        Vector3 bottomRight = new Vector3(horizontalBounds.y, verticalBounds.y, 0);
        Debug.DrawLine(topLeft, topRight, Color.red);
        Debug.DrawLine(topRight, bottomRight, Color.red);
        Debug.DrawLine(bottomLeft, bottomRight, Color.red);
        Debug.DrawLine(topLeft, bottomLeft, Color.red);
    }
}
