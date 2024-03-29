﻿using System;
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
    private Vector3 targetPosition;

    void Start()
    {
        levelManager = GetComponentInParent<LevelManager>();
        camera = GetComponent<Camera>();

        targetPosition = transform.position;
        camera.orthographicSize = Mathf.Abs(transform.position.z) * Mathf.Tan(camera.fieldOfView / 2 * Mathf.Deg2Rad);
    }

    void LateUpdate()
    {
        if (levelManager.level == null || levelManager.Mode == GameState.Transition)
        {
            return;
        }

        var newPosition = targetPosition;

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
            var moveVector = Vector3.zero;
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

            var zoomDelta = Input.mouseScrollDelta.y * scrollSpeed;

            newPosition += (moveVector + Vector3.forward * zoomDelta) * Time.deltaTime * scrollSpeed;
        }

        camera.orthographicSize = Mathf.Abs(targetPosition.z) * Mathf.Tan(45f / 2 * Mathf.Deg2Rad);

        newPosition.x = Mathf.Max(levelManager.level.StartPoint.x, newPosition.x);
        newPosition.x = Mathf.Min(levelManager.level.EndPoint.x, newPosition.x);

        newPosition.y = Mathf.Max(levelManager.level.Rect.yMin, Mathf.Min(newPosition.y, levelManager.level.Rect.yMax));
        newPosition.z = Mathf.Max(-25, Mathf.Min(newPosition.z, -10));

        var targetZ = 0f;

        if (levelManager.Mode == GameState.Edit)
        {
            targetZ = -900f;
            targetPosition = newPosition;
        }
        else
        {
            targetZ = newPosition.z;

            targetPosition = newPosition;
        }

        camera.fieldOfView = Mathf.Atan(camera.orthographicSize / Mathf.Abs(targetZ)) * 2 * Mathf.Rad2Deg;

        transform.position = new Vector3(
            newPosition.x,
            newPosition.y,
            targetZ
        );
    }

    public void ModeChanged(GameState? prev, GameState mode, GameState? next)
    {
        if (mode != GameState.Transition)
        {
            camera.orthographic = mode == GameState.Edit;
            return;
        }

        if (next == GameState.Edit || next == GameState.Play)
        {
            camera.orthographic = false;
            StartCoroutine(AnimateBlend(next.Value));
        }
    }

    Func<float, float> cubicEasingIn = x => x * x * x;
    Func<float, float> cubicEasingOut = x => 1f + (x -= 1f) * x * x;

    IEnumerator AnimateBlend(GameState mode)
    {
        Func<float, float> myEasing =
            mode == GameState.Edit
                ? cubicEasingIn
                : cubicEasingOut;

        while (levelManager.TransitionProgress < 1)
        {
            var blendCameraMode = myEasing(levelManager.TransitionProgress);

            var position = transform.position;

            var targetZ = mode == GameState.Edit ? -900f : targetPosition.z;

            targetZ = Mathf.Lerp(position.z, targetZ, blendCameraMode);

            camera.fieldOfView = Mathf.Atan(camera.orthographicSize / Mathf.Abs(targetZ)) * 2 * Mathf.Rad2Deg;

            transform.position = new Vector3(
                position.x,
                position.y,
                targetZ
            );

            yield return null;
        }
    }

}
