using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

namespace Dennis.UI
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class UIMaterialModifier : UIBehaviour, IMaterialModifier
    {
        [Header("Shader Parameters")]
        [SerializeField]
        private List<ShaderParameter> _parameters = new List<ShaderParameter>();

        [NonSerialized]
        private Graphic _graphic;
        public Graphic Graphic => _graphic ? _graphic : _graphic = GetComponent<Graphic>();

        private static Dictionary<Material, Material> _materialCache = new Dictionary<Material, Material>();

        protected override void OnEnable()
        {
            base.OnEnable();
            if (Graphic == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[CustomUI] Missing Graphic component on {gameObject.name}");
#endif
                return;
            }
            Graphic.SetMaterialDirty();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (_materialCache != null)
            {
                foreach (var material in _materialCache.Values)
                {
                    if (material != null)
                    {
                        DestroyImmediate(material);
                    }
                }
                _materialCache.Clear();
                _materialCache = null;
            }

            if (Graphic != null) _graphic.SetMaterialDirty();
        }


#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (!IsActive() || Graphic == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[CustomUI] Invalid state: IsActive={IsActive()} Graphic={Graphic}");
#endif
                return;
            }
            Graphic.SetMaterialDirty();
        }
#endif

        public Material GetModifiedMaterial(Material baseMaterial)
        {
            if (!IsActive() || Graphic == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[CustomUI] GetModifiedMaterial called but Graphic is null on {gameObject.name}");
#endif
                return baseMaterial;
            }

            if (baseMaterial == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[CustomUI] Base material is null on {gameObject.name}");
#endif
                return null;
            }

            // Do not share materials;
            // Each Graphic should have its own independent instance.
            Material modifiedMaterial = new Material(baseMaterial)
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            modifiedMaterial.CopyPropertiesFromMaterial(baseMaterial);

            foreach (var param in _parameters)
            {
                if (string.IsNullOrEmpty(param.PropertyName)) continue;

                if (!baseMaterial.HasProperty(param.PropertyName)) continue;

                switch (param.ParameterType)
                {
                    case ShaderParameterType.Float:
                        modifiedMaterial.SetFloat(param.PropertyName, param.FloatValue);
                        break;
                    case ShaderParameterType.Int:
                        modifiedMaterial.SetInt(param.PropertyName, param.IntValue);
                        break;
                    case ShaderParameterType.Color:
                        modifiedMaterial.SetColor(param.PropertyName, param.ColorValue);
                        break;
                    case ShaderParameterType.Vector4:
                        modifiedMaterial.SetVector(param.PropertyName, param.VectorValue);
                        break;
                }
            }

            return modifiedMaterial;
        }

        public void SetFloat(string propertyName, float value)
        {
            SetProperty(propertyName, value);
        }

        public void SetInt(string propertyName, int value)
        {
            SetProperty(propertyName, value);
        }

        public void SetColor(string propertyName, Color color)
        {
            SetProperty(propertyName, color);
        }

        public void SetVector4(string propertyName, Vector4 vector)
        {
            SetProperty(propertyName, vector);
        }

        public float GetFloat(string propertyName)
        {
            return GetProperty<float>(propertyName, 0f);
        }

        public int GetInt(string propertyName)
        {
            return GetProperty<int>(propertyName, 0);
        }

        public Color GetColor(string propertyName)
        {
            return GetProperty<Color>(propertyName, Color.white);
        }

        public Vector4 GetVector4(string propertyName)
        {
            return GetProperty<Vector4>(propertyName, Vector4.zero);
        }

        private void SetProperty<T>(string propertyName, T value)
        {
            if (Graphic == null || Graphic.material == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[UIMaterialModifier] No valid material found on {gameObject.name}");
#endif
                return;
            }

            Material mat = Graphic.material;

            if (!mat.HasProperty(propertyName))
            {
#if UNITY_EDITOR
                Debug.LogError($"[UIMaterialModifier] Shader does not contain property '{propertyName}' on {gameObject.name}");
#endif
                return;
            }

            switch (value)
            {
                case float floatValue:
                    mat.SetFloat(propertyName, floatValue);
                    break;
                case int intValue:
                    mat.SetInt(propertyName, intValue);
                    break;
                case Color colorValue:
                    mat.SetColor(propertyName, colorValue);
                    break;
                case Vector4 vectorValue:
                    mat.SetVector(propertyName, vectorValue);
                    break;
            }

            Graphic.SetMaterialDirty();
        }

        private T GetProperty<T>(string propertyName, T defaultValue)
        {
            if (Graphic == null || Graphic.material == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[UIMaterialModifier] No valid material found on {gameObject.name}");
#endif
                return defaultValue;
            }

            Material mat = Graphic.material;

            if (!mat.HasProperty(propertyName))
            {
#if UNITY_EDITOR
                Debug.LogError($"[UIMaterialModifier] Shader does not contain property '{propertyName}' on {gameObject.name}");
#endif
                return defaultValue;
            }

            return propertyName switch
            {
                _ when typeof(T) == typeof(float) => (T)(object)mat.GetFloat(propertyName),
                _ when typeof(T) == typeof(int) => (T)(object)mat.GetInt(propertyName),
                _ when typeof(T) == typeof(Color) => (T)(object)mat.GetColor(propertyName),
                _ when typeof(T) == typeof(Vector4) => (T)(object)mat.GetVector(propertyName),
                _ => defaultValue
            };
        }
    }
}
