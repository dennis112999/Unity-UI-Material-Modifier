using UnityEngine;
using UnityEngine.UI;

namespace Dennis.UI
{

#if UNITY_EDITOR

    using UnityEditor;

    [CustomEditor(typeof(UIMaterialModifier))]
    public class UIMaterialModifierEditor : Editor
    {
        private SerializedProperty parametersProp;
        private UIMaterialModifier _UIMaterialModifier;

        private void OnEnable()
        {
            parametersProp = serializedObject.FindProperty("_parameters");
            _UIMaterialModifier = (UIMaterialModifier)target;
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Label("Click here to visit wiki!", EditorStyles.linkLabel);
            Rect rect = GUILayoutUtility.GetLastRect();
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
            Event currentEvent = Event.current;
            if (currentEvent.type == EventType.MouseDown && rect.Contains(currentEvent.mousePosition))
            {
                Help.BrowseURL("https://github.com/dennis112999/Unity-UI-Material-Modifier/wiki");
            }

            serializedObject.Update();

            EditorGUILayout.LabelField("Shader Parameters", EditorStyles.boldLabel);

            if (GUILayout.Button("Add Shader Parameter"))
            {
                serializedObject.Update();
                parametersProp.arraySize++;
                serializedObject.ApplyModifiedProperties();
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

            // Draw parameter header (includes Remove button)
            if (DrawParameterHeader(index))
            {
                return; // Exit if the parameter was removed
            }

            // Ensure parameter still exists before accessing it
            if (index >= parametersProp.arraySize) return;

            SerializedProperty propertyName = param.FindPropertyRelative("PropertyName");
            SerializedProperty type = param.FindPropertyRelative("ParameterType");

            DrawPropertyNameAndType(propertyName, type);

            // Only proceed if the shader property is valid
            if (CheckShaderPropertyType(propertyName.stringValue, (ShaderParameterType)type.enumValueIndex))
            {
                DrawParameterValueFields(param, (ShaderParameterType)type.enumValueIndex);
            }

            EditorGUILayout.EndVertical();
        }


        private bool DrawParameterHeader(int index)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Parameter {index + 1}", EditorStyles.boldLabel);

            if (GUILayout.Button("Remove", GUILayout.Width(70)))
            {
                serializedObject.Update();
                parametersProp.DeleteArrayElementAtIndex(index);
                serializedObject.ApplyModifiedProperties();

                return true;
            }

            EditorGUILayout.EndHorizontal();
            return false;
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


        /// <summary>
        /// Checks if the selected shader property type matches the actual shader property.
        /// Returns `false` if there's a mismatch or the property does not exist.
        /// </summary>
        private bool CheckShaderPropertyType(string propertyName, ShaderParameterType selectedType)
        {
            if (string.IsNullOrEmpty(propertyName)) return false;

            // Get the Material from the Graphic component
            Material baseMaterial = _UIMaterialModifier.GetComponent<Graphic>()?.material;
            if (baseMaterial == null || !baseMaterial.HasProperty(propertyName))
            {
                EditorGUILayout.HelpBox($"Shader does not have property '{propertyName}'", MessageType.Warning);
                return false;
            }

            // Get the Shader property type
            Shader shader = baseMaterial.shader;
            int propertyIndex = shader.FindPropertyIndex(propertyName);
            if (propertyIndex == -1) return false;

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
                return false;
            }

            return true;
        }

    }

#endif
}
