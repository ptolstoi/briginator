using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField]
    private float scrollSpeed = 5;
    [SerializeField]
    private float scrollBorder = 0.05f;
    [SerializeField]
    private bool useBorderScroll = false;


    LevelManager levelManager;
    private Vector3 movePositionStart;
    private new Camera camera;

    void Start()
    {
        levelManager = GetComponentInParent<LevelManager>();
        camera = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (levelManager.level == null)
        {
            return;
        }

        var moveVector = Vector3.zero;
        var newPosition = transform.position;

        if (Input.GetMouseButton(2))
        {
            var mousePos = Input.mousePosition;
            mousePos.z = -transform.position.z;
            var worldPos = camera.ScreenToWorldPoint(mousePos);

            if (Input.GetMouseButtonDown(2))
            {
                movePositionStart = worldPos;
            }

            var diff = -(worldPos - movePositionStart);
            newPosition += diff;
            movePositionStart = worldPos + diff;
        }
        else
        {
            if (useBorderScroll)
            {
                if (Input.mousePosition.x < Screen.width * scrollBorder)
                {
                    moveVector.x = -1;
                }
                else if (Input.mousePosition.x > Screen.width * (1 - scrollBorder))
                {
                    moveVector.x = 1;
                }

                if (Input.mousePosition.y < Screen.height * scrollBorder)
                {
                    moveVector.y = -1;
                }
                else if (Input.mousePosition.y > Screen.height * (1 - scrollBorder))
                {
                    moveVector.y = 1;
                }

                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    moveVector *= 2;
                }
            }

            moveVector.z = Input.mouseScrollDelta.y * scrollSpeed;

            newPosition += moveVector * Time.deltaTime * scrollSpeed;
        }

        newPosition.x = Mathf.Max(levelManager.level.StartPoint.x, newPosition.x);
        newPosition.x = Mathf.Min(levelManager.level.EndPoint.x, newPosition.x);

        newPosition.y = Mathf.Max(levelManager.level.Rect.yMin, Mathf.Min(newPosition.y, levelManager.level.Rect.yMax));

        newPosition.z = Mathf.Max(-25, Mathf.Min(newPosition.z, -10));

        transform.position = newPosition;
    }
}
