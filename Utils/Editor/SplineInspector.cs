using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Framework
{
    [CustomEditor(typeof(Spline))]
    public class SplineInspector : Editor
    {
        public static readonly float PropertyFieldHeight = 18;

        private Spline spline;

        private Transform handleTransform;
        private Quaternion handleRotation;

        private List<Vector3> transformedPoints = new List<Vector3>();

        private ReorderableList list;

        #region OnInspector

        private void OnEnable()
        {
            list = new ReorderableList(serializedObject,
                serializedObject.FindProperty("Points"),
                true, true, true, true);
            list.drawElementCallback += PointInspectorDraw;
            list.elementHeightCallback += PointInspectorHeight;
        }

        private float PointInspectorHeight(int index)
        {
            float height = PropertyFieldHeight;

            if ((spline.AdditionalTransform & Spline.SplineTransform.Rotation) != 0)
            {
                height += PropertyFieldHeight;
            }

            if ((spline.AdditionalTransform & Spline.SplineTransform.Scale) != 0)
            {
                height += PropertyFieldHeight;
            }

            return height;
        }

        private Vector3? PointPropertyField(ref Rect rect, Vector3 value, string label = "")
        {
            EditorGUI.LabelField(rect, label);
            rect.x += rect.width * 0.25f;

            Vector3 result;
            EditorGUI.BeginChangeCheck();
            {
                result = EditorGUI.Vector3Field(rect, String.Empty, value);
            }

            rect.y += PropertyFieldHeight;
            rect.x -= rect.width * 0.25f;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spline, "Transform Point");
                EditorUtility.SetDirty(spline);
                return result;
            }

            return null;
        }

        private void PointInspectorDraw(Rect rect, int index, bool active, bool focused)
        {
            rect.width *= 0.75f;
            rect.position = new Vector2(rect.position.x, rect.position.y + 2);
            {
                var newPos = PointPropertyField(ref rect, spline.Points[index].position, "Position");
                if (newPos.HasValue)
                {
                    spline.Points[index].position = newPos.Value;
                }
            }

            if ((spline.AdditionalTransform & Spline.SplineTransform.Rotation) != 0)
            {
                var newRot = PointPropertyField(ref rect, spline.Points[index].rotation.eulerAngles, "Rotation");
                if (newRot.HasValue)
                {
                    spline.Points[index].rotation = Quaternion.Euler(newRot.Value);
                }
            }

            if ((spline.AdditionalTransform & Spline.SplineTransform.Scale) != 0)
            {
                var newScl = PointPropertyField(ref rect, spline.Points[index].scale, "Scale");
                if (newScl.HasValue)
                {
                    spline.Points[index].scale = newScl.Value;
                }
            }
        }

        public override void OnInspectorGUI()
        {
            spline = target as Spline;

            DrawDefaultInspector();

            list.DoLayoutList();
        }

        #endregion

        #region OnScene

        private void ProvideList()
        {
            if (transformedPoints == null)
                transformedPoints = new List<Vector3>();
            else
                transformedPoints.Clear();
        }

        private void OnSceneGUI()
        {
            ProvideList();

            spline = target as Spline;

            handleTransform = spline.transform;
            handleRotation = Tools.pivotRotation == PivotRotation.Local ?
                handleTransform.rotation : Quaternion.identity;

            for (int i = 0; i < spline.Points.Length; i++)
            {
                transformedPoints.Add(ShowPoint(i));
            }

            Handles.color = Color.white;

            if (transformedPoints.Count < 2)
                return;

            for (int i = 0; i < transformedPoints.Count - 1; i++)
            {
                Handles.DrawLine(transformedPoints[i], transformedPoints[i + 1]);
            }

            if (spline.Loop)
                Handles.DrawLine(transformedPoints[transformedPoints.Count - 1], transformedPoints[0]);
        }

        private Vector3 ShowPoint(int index)
        {
            Vector3 point = handleTransform.TransformPoint(spline.Points[index].position);
            float size = HandleUtility.GetHandleSize(point);

            EditorGUI.BeginChangeCheck();
            {
                point = Handles.DoPositionHandle(point, handleRotation);
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spline, "Move Point");
                EditorUtility.SetDirty(spline);
                spline.Points[index].position = handleTransform.InverseTransformPoint(point);
            }

            return point;
        }

        #endregion

        #region Unused

        //private Vector3 HandleTransformTool(int index)
        //{
        //    Vector3    pos = handleTransform.TransformPoint(spline.Points[index].position);
        //    Quaternion rot = spline.Points[index].rotation;
        //    Vector3    scl = spline.Points[index].scale;

        //    EditorGUI.BeginChangeCheck();
        //    {
        //        switch (Tools.current)
        //        {
        //            case Tool.Move:
        //                pos = Handles.DoPositionHandle(pos, handleRotation);
        //                break;

        //            case Tool.Rotate:
        //                rot = Handles.DoRotationHandle(rot, pos);
        //                break;

        //            case Tool.Scale:
        //                scl = Handles.DoScaleHandle(scl, pos, rot, 1);
        //                break;
        //        }
        //    }
        //    if (EditorGUI.EndChangeCheck())
        //    {
        //        Undo.RecordObject(spline, "Transform Point");
        //        EditorUtility.SetDirty(spline);
        //        switch (Tools.current)
        //        {
        //            case Tool.Move:
        //                spline.Points[index].position = handleTransform.InverseTransformPoint(pos);
        //                break;

        //            case Tool.Rotate:
        //                spline.Points[index].rotation = rot;
        //                break;

        //            case Tool.Scale:
        //                spline.Points[index].scale = scl;
        //                break;
        //        }
        //    }

        //    return pos;
        //}

        //private void HandleMove()
        //{

        //}

        //private void HandleRotate()
        //{

        //}

        //private void HandleScale()
        //{

        //}

        #endregion
    }
}
