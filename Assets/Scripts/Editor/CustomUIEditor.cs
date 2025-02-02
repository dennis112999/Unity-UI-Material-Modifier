using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Dennis.UI
{
    [CustomEditor(typeof(CustomUI))]
    public class CustomUIEditor : Editor
    {
        private SerializedProperty parametersProp;
        private CustomUI customUI;

        private void OnEnable()
        {
            parametersProp = serializedObject.FindProperty("_parameters");
            customUI = (CustomUI)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Shader Parameters", EditorStyles.boldLabel);

            if (GUILayout.Button("Add Shader Parameter"))
            {
                parametersProp.arraySize++;
            }

            DrawShaderParameters();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawShaderParameters()
        {
            for (int i = 0; i < parametersProp.arraySize; i++)
            {
                SerializedProperty param = parametersProp.GetArrayElementAtIndex(i);
                DrawShaderParameter(param, i);
            }
        }

        private void DrawShaderParameter(SerializedProperty param, int index)
        {
            EditorGUILayout.BeginVertical("box");

            DrawParameterHeader(index);

            SerializedProperty propertyName = param.FindPropertyRelative("PropertyName");
            SerializedProperty type = param.FindPropertyRelative("ParameterType");

            DrawPropertyNameAndType(propertyName, type);

            CheckShaderPropertyType(propertyName.stringValue, (ShaderParameterType)type.enumValueIndex);

            DrawParameterValueFields(param, (ShaderParameterType)type.enumValueIndex);

            EditorGUILayout.EndVertical();
        }

        private void DrawParameterHeader(int index)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Parameter {index + 1}", EditorStyles.boldLabel);
            if (GUILayout.Button("Remove", GUILayout.Width(70)))
            {
                parametersProp.DeleteArrayElementAtIndex(index);
                return;
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawPropertyNameAndType(SerializedProperty propertyName, SerializedProperty type)
        {
            propertyName.stringValue = EditorGUILayout.TextField("Property Name", propertyName.stringValue);
            type.enumValueIndex = (int)(ShaderParameterType)EditorGUILayout.EnumPopup("Type", (ShaderParameterType)type.enumValueIndex);
        }


        private void DrawParameterValueFields(SerializedProperty param, ShaderParameterType type)
        {
            SerializedProperty floatValue = param.FindPropertyRelative("FloatValue");
            SerializedProperty intValue = param.FindPropertyRelative("IntValue");
            SerializedProperty colorValue = param.FindPropertyRelative("ColorValue");
            SerializedProperty vectorValue = param.FindPropertyRelative("VectorValue");
            SerializedProperty range = param.FindPropertyRelative("Range");

            switch (type)
            {
                case ShaderParameterType.Float:
                    range.vector2Value = EditorGUILayout.Vector2Field("Range", range.vector2Value);
                    floatValue.floatValue = EditorGUILayout.Slider("Float Value", floatValue.floatValue, range.vector2Value.x, range.vector2Value.y);
                    break;

                case ShaderParameterType.Int:
                    intValue.intValue = EditorGUILayout.IntField("Int Value", intValue.intValue);
                    break;

                case ShaderParameterType.Color:
                    colorValue.colorValue = EditorGUILayout.ColorField("Color", colorValue.colorValue);
                    break;

                case ShaderParameterType.Vector4:
                    vectorValue.vector4Value = EditorGUILayout.Vector4Field("Vector4", vectorValue.vector4Value);
                    break;

                default:
                    EditorGUILayout.HelpBox($"Unhandled ShaderParameterType: {type}", MessageType.Warning);
                    break;
            }
        }


        private void CheckShaderPropertyType(string propertyName, ShaderParameterType selectedType)
        {
            if (string.IsNullOrEmpty(propertyName)) return;

            // Get the Material from the Graphic component
            Material baseMaterial = customUI.GetComponent<Graphic>()?.material;
            if (baseMaterial == null || !baseMaterial.HasProperty(propertyName))
            {
                EditorGUILayout.HelpBox($"Shader does not have property '{propertyName}'", MessageType.Warning);
                return;
            }

            // Get the Shader property type
            Shader shader = baseMaterial.shader;
            int propertyIndex = shader.FindPropertyIndex(propertyName);
            if (propertyIndex == -1) return;

            // Determine if the type matches
            ShaderUtil.ShaderPropertyType propertyType = ShaderUtil.GetPropertyType(shader, propertyIndex);
            string expectedType = propertyType == ShaderUtil.ShaderPropertyType.Range ? "Float" : propertyType.ToString();
            string selectedTypeStr = selectedType.ToString();

            bool typeMismatch = false;
            switch (selectedType)
            {
                case ShaderParameterType.Float:
                case ShaderParameterType.Int:
                    if (propertyType != ShaderUtil.ShaderPropertyType.Float && propertyType != ShaderUtil.ShaderPropertyType.Range)
                        typeMismatch = true;
                    break;

                case ShaderParameterType.Color:
                    if (propertyType != ShaderUtil.ShaderPropertyType.Color)
                        typeMismatch = true;
                    break;

                case ShaderParameterType.Vector4:
                    if (propertyType != ShaderUtil.ShaderPropertyType.Vector)
                        typeMismatch = true;
                    break;

                default:
                    typeMismatch = true;
                    break;
            }

            if (typeMismatch)
            {
                EditorGUILayout.HelpBox($"Shader property '{propertyName}' is expected to be {expectedType}, but you assigned {selectedTypeStr}.", MessageType.Warning);
            }
        }
    }
}
