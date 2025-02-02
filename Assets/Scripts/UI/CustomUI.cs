using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

namespace Dennis.UI
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class CustomUI : UIBehaviour, IMaterialModifier
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

    }
}
