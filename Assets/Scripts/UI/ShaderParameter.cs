using UnityEngine;
using System;

namespace Dennis.UI
{
    public enum ShaderParameterType
    {
        Float,
        Int,
        Color,
        Vector4
    }

    [Serializable]
    public class ShaderParameter
    {
        public string PropertyName;
        public ShaderParameterType ParameterType = ShaderParameterType.Float;

        public float FloatValue = 1.0f;
        public int IntValue = 1;
        public Color ColorValue = Color.white;
        public Vector4 VectorValue = Vector4.one;

        public Vector2 Range = new Vector2(0, 2);
    }
}