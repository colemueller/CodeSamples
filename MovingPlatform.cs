using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    private Transform _platform;
    private Vector3[] _path;
    private int _currentPoint = 0;
    private bool _move = false;
    private bool _lock = false;


    public enum Mode {Always_Move, Start_On_Contact, Only_Move_On_Contact}
    
    //TODO: Add timer to pause at points. Choosable between just end points or all points
    public float moveSpeed = 1f;
    [Tooltip("Should the platform start going back once it hits the end of the path or stop and wait until reactivation?")]
    public bool loop = false;
    [Tooltip("The platform will stop once it's at the end of it's path and become deactivated. Will operate as a static platform.")]
    public bool oneWay = false;
    public Mode _mode;
    
    void Awake()
    {
        LineRenderer lr = transform.GetChild(0).GetComponent<LineRenderer>();
        _path = new Vector3[lr.positionCount];
        for(int i = 0; i < _path.Length; i++)
        {
            _path[i] = transform.TransformPoint(lr.GetPosition(i));
        }
        _platform = transform.GetChild(1);

        _platform.position = _path[0];
        if(_mode == Mode.Always_Move) _move = true;
    }

    void FixedUpdate()
    {
        if(!_lock && _move) Move();
    }

    void Move()
    {
        int nextPoint;
        if(_currentPoint != _path.Length - 1) nextPoint = _currentPoint + 1;
        else nextPoint = _currentPoint - 1;

        float step =  moveSpeed * Time.deltaTime;
        _platform.position = Vector3.MoveTowards(_platform.position, _path[nextPoint], step);

        if(Mathf.Approximately(Vector3.Distance(_platform.position, _path[nextPoint]), 0)) //switch target position to next point in path
        {
            if(nextPoint == _path.Length - 1 || nextPoint == 0)
            {
                if(_mode != Mode.Always_Move && !loop)
                {
                    _move = false;
                }
                if(nextPoint == 0 && _mode == Mode.Start_On_Contact)
                {
                    _move = false;
                }
                if(oneWay)
                {
                    _lock = true;
                }
            }
            _currentPoint = nextPoint;
        }
    }

    public void CustomCollisionEnter2D(Collision2D col)
    {
        if(col.gameObject.CompareTag("Player"))
        {
            col.transform.SetParent(_platform);
            if(_mode == Mode.Start_On_Contact || _mode == Mode.Only_Move_On_Contact) _move = true;
        }
    }

    public void CustomCollisionExit2D(Collision2D col)
    {
        if(col.gameObject.CompareTag("Player"))
        {
            col.transform.SetParent(null);
            if(_mode == Mode.Only_Move_On_Contact) _move = false;
        }
    }
}
