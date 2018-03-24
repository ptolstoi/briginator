using UnityEngine;
using UnityEditor;

public class LayerAttribute : PropertyAttribute
{

}

[CustomPropertyDrawer(typeof(LayerAttribute))]
public class LayerAttributeEditor : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        property.intValue = EditorGUI.LayerField(position, label, property.intValue);
    }
}