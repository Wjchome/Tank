using System;
using System.Collections;
using System.Collections.Generic;
using FixMath.NET;
using Physics2D;
using UnityEngine;

public class qwer : MonoBehaviour
{
    public RigidBody2DComponent rigidBodyA;
    public RigidBody2DComponent rigidBodyB;


    public float a;

    public void Start()
    {
        PhysicsWorld2DComponent.Instance.AddRigidBody(rigidBodyA, (FixVector2)(Vector2)rigidBodyA.transform.position);
        PhysicsWorld2DComponent.Instance.AddRigidBody(rigidBodyB, (FixVector2)(Vector2)rigidBodyB.transform.position);
        
        
        rigidBodyA.Body.ApplyForce(FixVector2.Right*(Fix64)a);
    }

    public void Update()
    {
        
    }
}