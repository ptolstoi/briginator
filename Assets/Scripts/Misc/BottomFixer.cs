using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottomFixer : MonoBehaviour
{

    // Use this for initialization
    void Update()
    {
        var scale = transform.localScale;
        var position = transform.position;

        scale.y = Mathf.Max(0, position.y - -4);

        transform.localScale = scale;
    }
}
