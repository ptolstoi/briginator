using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnCube : MonoBehaviour
{
	public GameObject CarPrefab;


    void Update()
    {
        var mouseLeft = Input.GetMouseButtonUp(0);
        var mouseRight = Input.GetMouseButtonUp(1);

        if (mouseLeft || mouseRight)
        {
            Transform trans;
            if (!Input.GetKey(KeyCode.LeftShift))
            {
                trans = SpawnACube(mouseLeft, mouseRight);
            } else {
				trans = SpawnACar(mouseLeft, mouseRight);
			}

            var mousePos = Input.mousePosition;
            mousePos.z = -Camera.main.transform.position.z;
            var pos = Camera.main.ScreenToWorldPoint(mousePos);
            trans.position = pos;
        }
    }

    private Transform SpawnACar(bool mouseLeft, bool mouseRight)
    {
        var go = Instantiate(CarPrefab);
		return go.transform;
    }

    Transform SpawnACube(bool mouseLeft, bool mouseRight)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Cube";

        GameObject.DestroyImmediate(go.GetComponent<Collider>());
        go.AddComponent<BoxCollider2D>();
        go.AddComponent<Rigidbody2D>();
        go.transform.localScale *= 0.5f;

        if (mouseRight)
        {
            go.GetComponent<Rigidbody2D>().mass = 10;
			go.GetComponent<MeshRenderer>().material.color = Color.grey;
        }

        return go.transform;
    }
}
