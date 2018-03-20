using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMotor : MonoBehaviour {

	public WheelJoint2D[] MotorWheels;
	public float Speed = 5;

	private void Start() {
		foreach(var wheel in MotorWheels) {
			wheel.useMotor = true;
			wheel.motor = new JointMotor2D() {
				motorSpeed = -Speed,
				maxMotorTorque = wheel.motor.maxMotorTorque
			};
		}
		var rb = GetComponent<Rigidbody2D>();
		rb.centerOfMass += Vector2.down * 0.1f + Vector2.right * 0.1f;
	}
}
