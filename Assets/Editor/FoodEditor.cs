using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FoodManager))]
public class FoodEditor : Editor
{
  public override void OnInspectorGUI()
  {
    FoodManager food = (FoodManager)target;

    EditorGUI.BeginChangeCheck();
    base.OnInspectorGUI();

    if (EditorGUI.EndChangeCheck())
    {
      food.GenerateFood();
    }

    if (GUILayout.Button("Generate"))
    {
      food.GenerateFood();
    }

    if (GUILayout.Button("Clear"))
    {
      food.ClearFood();
    }
  }
}