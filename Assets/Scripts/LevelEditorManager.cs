using UnityEngine;
using System;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class LevelEditorManager : MonoBehaviour
{
    enum EditorState
    {
        NothingSelected,
        AnchorSelected

    }


    [Header("Wiring")]
    public LevelManager levelManager;
    public Transform cursorTransform;
    public Transform selectedAnchorTransform;
    public NewConnectionManager newConnectionManager;
    private EditorMeshGenerator editorMeshGenerator;

    [Header("Misc")]
    public float cursorDepth = 10;
    public float gridDepth = 6;
    public float newConnectionDepth = 7;

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private Mesh mesh;
    private List<Vector3> vertices;
    private List<int> indices;

    private Anchor selectedAnchor;
    private ConnectionType currentConnectionType;

    private EditorState state;

    private void Start()
    {
        editorMeshGenerator = gameObject.EnsureComponent<EditorMeshGenerator>();
        editorMeshGenerator.levelEditorManager = this;

        cursorTransform.gameObject.SetActive(false);
        selectedAnchorTransform.gameObject.SetActive(false);
    }

    public void ModeChanged(GameState? prev, GameState mode, GameState? next)
    {
        editorMeshGenerator.enabled = mode == GameState.Edit;

        cursorTransform.gameObject.SetActive(false);
        selectedAnchorTransform.gameObject.SetActive(false);
        selectedAnchor = null;
        newConnectionManager.active = false;

        state = EditorState.NothingSelected;

        if (mode == GameState.Edit)
        {
            editorMeshGenerator.RegenerateMesh();
        }
    }

    private void Update()
    {
        if (levelManager.Mode != GameState.Edit)
        {
            return;
        }

        var gridPos = GetMouseWorldPosition();

        var bounds = levelManager.level.Rect;
        bounds.min -= Vector2.one * levelManager.gridSize / 3;
        bounds.max += Vector2.one * levelManager.gridSize / 3;

        var isInGrid = bounds.Contains(gridPos);

        var selectButtonClicked = Input.GetMouseButtonDown(0);
        var deselectButtonClicked = Input.GetMouseButtonDown(1);
        var deleteButtonClicked = Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.D);

        if (Input.GetKeyUp(KeyCode.Alpha1)) currentConnectionType = ConnectionType.Road;
        if (Input.GetKeyUp(KeyCode.Alpha2)) currentConnectionType = ConnectionType.Wood;
        if (Input.GetKeyUp(KeyCode.Alpha3)) currentConnectionType = ConnectionType.Steel;

        var nearestAnchor = isInGrid ? levelManager.GetNearestAnchor(gridPos) : null;

        if (nearestAnchor != null)
        {
            gridPos = nearestAnchor.Position;
        }


        cursorTransform.gameObject.SetActive(isInGrid);
        if (isInGrid)
        {
            cursorTransform.position = gridPos.WithZ(-cursorDepth);
        }

        if (state == EditorState.NothingSelected)
        {
            selectedAnchorTransform.gameObject.SetActive(false);
            newConnectionManager.active = false;

            if (isInGrid)
            {
                if (selectButtonClicked && nearestAnchor != null)
                {
                    selectedAnchor = nearestAnchor;
                    state = EditorState.AnchorSelected;
                }
            }
        }
        else if (state == EditorState.AnchorSelected)
        {
            selectedAnchorTransform.gameObject.SetActive(true);
            selectedAnchorTransform.position = selectedAnchor.Position.WithZ(-newConnectionDepth);

            newConnectionManager.active = isInGrid;
            newConnectionManager.anchorA = selectedAnchor.Position.WithZ(-newConnectionDepth);
            newConnectionManager.anchorB = gridPos.WithZ(-newConnectionDepth);

            if (selectButtonClicked && (nearestAnchor == selectedAnchor || !isInGrid) || deselectButtonClicked)
            {
                selectedAnchor = null;
                state = EditorState.NothingSelected;
            }
            else if (selectButtonClicked && isInGrid)
            {
                Anchor otherAnchor = null;
                if (nearestAnchor == null)
                {
                    var newAnchor = new Anchor(gridPos);
                    levelManager.solution.Add(newAnchor);
                    otherAnchor = newAnchor;
                }
                else
                {
                    otherAnchor = nearestAnchor;
                }
                var newConnection = new Connection(selectedAnchor.Id, otherAnchor.Id, currentConnectionType);
                levelManager.solution.Add(newConnection);
                selectedAnchor = otherAnchor;
            }
            else if (deleteButtonClicked)
            {
                levelManager.solution.Remove(selectedAnchor);
                selectedAnchor = null;
                state = EditorState.NothingSelected;
            }
        }

        // if (isInGrid)
        // {
        //     var nearestAnchor = levelManager.GetNearestAnchor(gridPos);

        //     cursorTransform.position =
        //         nearestAnchor != null ?
        //         (Vector3)nearestAnchor.Position + Vector3.back * 5
        //         : gridPos + Vector3.back * (depth);

        //     if (nearestAnchor != null)
        //     {
        //         gridPos = nearestAnchor;
        //     }

        //     if (selectedAnchor == null && nearestAnchor != null && leftClick)
        //     {
        //         selectedAnchor = nearestAnchor;
        //     }
        //     else if (leftClick && selectedAnchor != null)
        //     {
        //         Anchor otherAnchor = null;
        //         if (nearestAnchor == null)
        //         {
        //             otherAnchor = new Anchor(gridPos);
        //             levelManager.solution.Add(otherAnchor);
        //         }
        //         else
        //         {
        //             otherAnchor = nearestAnchor;
        //         }
        //         var newConnection = new Connection(otherAnchor.Id, selectedAnchor.Id, currentConnectionType);
        //         levelManager.solution.Add(newConnection);
        //         selectedAnchor = otherAnchor;
        //     }
        // }

        // selectedAnchorTransform.gameObject.SetActive(selectedAnchor != null);
        // newConnectionManager.active = selectedAnchor != null;
        // if (selectedAnchor != null)
        // {
        //     selectedAnchorTransform.position = (Vector3)selectedAnchor.Position + Vector3.back * 5;

        //     newConnectionManager.anchorA = (Vector2)selectedAnchor;
        //     if (!isInGrid)
        //     {
        //         newConnectionManager.active = false;
        //     }
        //     else
        //     {
        //         newConnectionManager.anchorB = (Vector2)gridPos;
        //     }


        //     if (rightClick)
        //     {
        //         selectedAnchor = null;
        //     }

        // }
    }

    Vector3 GetMouseWorldPosition()
    {
        var gridSize = levelManager.gridSize;
        var hexHeight = levelManager.hexHeight;
        var bounds = levelManager.level.Rect;
        var mousePosition = Input.mousePosition;
        mousePosition.z = -Camera.main.transform.position.z - cursorDepth;

        var mouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePosition) - (Vector3)bounds.min;

        mouseWorldPosition.y = Mathf.Round(mouseWorldPosition.y / hexHeight) * hexHeight;

        var posY = Mathf.RoundToInt(mouseWorldPosition.y / hexHeight);
        if (posY % 2 != 0)
        {
            mouseWorldPosition.x = Mathf.Round(mouseWorldPosition.x / gridSize - 0.5f) * gridSize + gridSize / 2;
        }
        else
        {
            mouseWorldPosition.x = Mathf.Round(mouseWorldPosition.x / gridSize) * gridSize;
        }

        mouseWorldPosition = mouseWorldPosition + (Vector3)bounds.min;

        return mouseWorldPosition;
    }
}