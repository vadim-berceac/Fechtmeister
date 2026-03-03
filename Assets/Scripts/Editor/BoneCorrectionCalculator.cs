#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class BoneCorrectionCalculator : EditorWindow
{
    private Vector3 _actualPosition;
    private Vector3 _nominalPosition;
    private Vector3 _resultPosition;

    private Vector3 _actualEuler;
    private Vector3 _nominalEuler;
    private Vector3 _resultEuler;

    [MenuItem("Tools/Bone Correction Calculator")]
    public static void ShowWindow() => GetWindow<BoneCorrectionCalculator>("Bone Correction");

    private void OnGUI()
    {
        // ---- Position ----
        GUILayout.Label("Position", EditorStyles.boldLabel);
        _actualPosition  = EditorGUILayout.Vector3Field("Actual",  _actualPosition);
        _nominalPosition = EditorGUILayout.Vector3Field("Nominal", _nominalPosition);

        GUILayout.Space(4);

        // ---- Rotation ----
        GUILayout.Label("Rotation", EditorStyles.boldLabel);
        _actualEuler  = EditorGUILayout.Vector3Field("Actual",  _actualEuler);
        _nominalEuler = EditorGUILayout.Vector3Field("Nominal", _nominalEuler);

        GUILayout.Space(8);

        if (GUILayout.Button("Calculate"))
        {
            _resultPosition = _nominalPosition - _actualPosition;

            var actual     = Quaternion.Euler(_actualEuler);
            var nominal    = Quaternion.Euler(_nominalEuler);
            var correction = nominal * Quaternion.Inverse(actual);
            _resultEuler   = correction.eulerAngles;
        }

        GUILayout.Space(10);
        GUILayout.Label("Result", EditorStyles.boldLabel);

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.Vector3Field("Position Correction", _resultPosition);
        EditorGUILayout.Vector3Field("Rotation Correction", _resultEuler);
        EditorGUI.EndDisabledGroup();

        GUILayout.Space(4);

        if (GUILayout.Button("Copy Position to Clipboard"))
        {
            EditorGUIUtility.systemCopyBuffer =
                $"{_resultPosition.x}, {_resultPosition.y}, {_resultPosition.z}";
        }

        if (GUILayout.Button("Copy Rotation to Clipboard"))
        {
            EditorGUIUtility.systemCopyBuffer =
                $"{_resultEuler.x}, {_resultEuler.y}, {_resultEuler.z}";
        }
    }
}
#endif