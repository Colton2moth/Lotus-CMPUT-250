using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrokenPlatformVisualFragment : MonoBehaviour
{
    private Vector3 _restPosition;
    public Vector3 restPosition {get{return _restPosition;} set{_restPosition = value;}}
    private Rigidbody _rigidbody;
    public Rigidbody Rigidbody {get{return _rigidbody;}}
    Renderer _renderer;
    public Renderer Renderer {get{return _renderer;}}



    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _renderer = GetComponent<Renderer>();
    }

}
