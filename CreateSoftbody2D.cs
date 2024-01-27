using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.U2D.Animation;
using System;
using Unity.VisualScripting;
using UnityEditor.Rendering;

public class CreateSoftbody2D : MonoBehaviour
{
    [MenuItem("Custom Tools/Create Softbody 2D")]
    static void MakeSoftbody()
    {
        Transform _selection = Selection.activeTransform;

        //check if current selection has "Sprite Skin" script and bones as childern
        SpriteSkin _spriteSkinScr = _selection.GetComponent<SpriteSkin>();
        if(_spriteSkinScr == null)
        {
            Debug.LogError("Create Softbody 2D :: Current selection (" + _selection.name + ") does not have a \"Sprite Skin\" component.");
            return;
        }
        if(_spriteSkinScr.rootBone == null || _spriteSkinScr.boneTransforms.Length == 0)
        {
            Debug.LogError("Create Softbody 2D :: Current selection (" + _selection.name + ") does not have any bone objects.");
            return;
        }
        
        //warn for uneven # of bones
        if(_spriteSkinScr.boneTransforms.Length % 2 != 0) Debug.LogWarning("Create Softbody 2D :: Selected object (" + _selection.name + ") has an uneven number of bones. Unexpected physics behaviors may occur.");
        
        //add rigidbodies and colliders
        Transform[] _bones = _spriteSkinScr.boneTransforms;
        foreach(Transform child in _bones)
        {
            if(!child.GetComponent<Rigidbody2D>())
            {
                Rigidbody2D _rb = child.AddComponent<Rigidbody2D>();
                _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
                _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }
            if(!child.GetComponent<CircleCollider2D>())
            {
                CircleCollider2D _col = child.AddComponent<CircleCollider2D>();

                _col.radius = 0.1f;
            }
        }

        /*This block adds spring joint to each bone and assigns appropriate rigidbodies + values to get the default behavior I want. I will manually tweak frequency and dampening ratio to get the stiffness I want later. */
        int _oppositeOffset = _bones.Length / 2;
        for(int j = 0; j < _bones.Length; j++)
        {
            SpringJoint2D _leftSpring = _bones[j].AddComponent<SpringJoint2D>();
            SpringJoint2D _rightSpring = _bones[j].AddComponent<SpringJoint2D>();
            SpringJoint2D _oppositeSpring = _bones[j].AddComponent<SpringJoint2D>();

            _leftSpring.autoConfigureConnectedAnchor = _rightSpring.autoConfigureConnectedAnchor = _oppositeSpring.autoConfigureConnectedAnchor = true;
            _leftSpring.autoConfigureDistance = _rightSpring.autoConfigureDistance = _oppositeSpring.autoConfigureDistance = false;

            if(j == 0)
            {
                _leftSpring.connectedBody = _bones[^1].GetComponent<Rigidbody2D>();
                _rightSpring.connectedBody = _bones[1].GetComponent<Rigidbody2D>();
            }
            else if(j == _bones.Length - 1)
            {
                _leftSpring.connectedBody = _bones[j-1].GetComponent<Rigidbody2D>();
                _rightSpring.connectedBody = _bones[0].GetComponent<Rigidbody2D>();
            }
            else
            {
                _leftSpring.connectedBody = _bones[j-1].GetComponent<Rigidbody2D>();
                _rightSpring.connectedBody = _bones[j+1].GetComponent<Rigidbody2D>();
            }

            if(j < _oppositeOffset)
            {
                _oppositeSpring.connectedBody = _bones[j + _oppositeOffset].GetComponent<Rigidbody2D>();
            }
            else
            {
                _oppositeSpring.connectedBody = _bones[j - _oppositeOffset].GetComponent<Rigidbody2D>();
            }

            _leftSpring.distance = _rightSpring.distance = _oppositeSpring.distance = 0.05f;

            _leftSpring.dampingRatio = _rightSpring.dampingRatio = _oppositeSpring.dampingRatio = 0.3f;
            _leftSpring.frequency = _rightSpring.frequency = _oppositeSpring.frequency = 5f;
        }

        Debug.Log("Create Softbody 2D :: Selected object (" + _selection.name + ") skinned successfully!");
    }
}
