using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PointCloud.LasFormat;

namespace PointCloudEditor.LasFormat
{
    [CustomEditor(typeof(LasLoadBehaviour))]
    public class LasLoadBehaviourEditor : Editor
    {
        private System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder(128);
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var obj = target as LasLoadBehaviour;
            if( obj == null)
            {
                return;
            }
            EditorGUILayout.LabelField("===========LasFileHeader============");
            var header = obj.HeaderInfo;
            stringBuilder.Append("Version:").Append(header.versionMajaor).
                Append(".").Append(header.versionMinor);
            EditorGUILayout.LabelField(stringBuilder.ToString());
            stringBuilder.Length = 0;

            stringBuilder.Append("Format:").Append(header.pointDatRecordFormat);
            EditorGUILayout.LabelField(stringBuilder.ToString());
            stringBuilder.Length = 0;

            stringBuilder.Append("Offset:").Append(header.xOffset).Append(",").
                Append(header.yOffset).Append(",").
                Append(header.zOffset);
            EditorGUILayout.LabelField(stringBuilder.ToString());
            stringBuilder.Length = 0;

            stringBuilder.Append("ScaleFactor:").Append(header.xScaleFactor).Append(",").
                Append(header.yScaleFactor).Append(",").
                Append(header.zScaleFactor);
            EditorGUILayout.LabelField(stringBuilder.ToString());
            stringBuilder.Length = 0;


            stringBuilder.Append("XRange:").Append(header.minX).Append("  ").
                Append(header.maxX);
            EditorGUILayout.LabelField(stringBuilder.ToString());
            stringBuilder.Length = 0;
            stringBuilder.Append("YRange:").Append(header.minY).Append("  ").
                Append(header.maxY);
            EditorGUILayout.LabelField(stringBuilder.ToString());
            stringBuilder.Length = 0;
            stringBuilder.Append("ZRange:").Append(header.minZ).Append("  ").
                Append(header.maxZ);
            EditorGUILayout.LabelField(stringBuilder.ToString());
            stringBuilder.Length = 0;
        }
    }
}