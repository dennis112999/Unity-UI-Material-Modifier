using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

namespace Dennis.UI
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class CustomUI : UIBehaviour, IMaterialModifier
    {
        [System.Serializable]
        public class ShaderParameter
        {
            public string propertyName;
            public float value;
        }

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
            if (Graphic != null)
            {
                Graphic.SetMaterialDirty();
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (!IsActive() || Graphic == null)
            {
                Debug.LogError($"[CustomUI] Invalid state: IsActive={IsActive()} Graphic={Graphic}");
                return;
            }
            Graphic.SetMaterialDirty();
        }
#endif

        protected override void OnDidApplyAnimationProperties()
        {
            base.OnDidApplyAnimationProperties();
            if (!IsActive() || Graphic == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[CustomUI] Animation properties applied but component is not active on {gameObject.name}");
#endif
                return;
            }
            Graphic.SetMaterialDirty();
        }

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

            if (!_materialCache.TryGetValue(baseMaterial, out var modifiedMaterial))
            {
                modifiedMaterial = new Material(baseMaterial)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                _materialCache[baseMaterial] = modifiedMaterial;
            }

            modifiedMaterial.CopyPropertiesFromMaterial(baseMaterial);

            foreach (var param in _parameters)
            {
                if (string.IsNullOrEmpty(param.propertyName))
                {
#if UNITY_EDITOR
                    Debug.LogError($"[CustomUI] Shader property name is empty in {gameObject.name}");
#endif
                    continue;
                }

                if (!baseMaterial.HasProperty(param.propertyName))
                {
#if UNITY_EDITOR
                    Debug.LogError($"[CustomUI] Shader does not have property '{param.propertyName}' on {gameObject.name}");
#endif
                    continue;
                }

                modifiedMaterial.SetFloat(param.propertyName, param.value);
            }

            return modifiedMaterial;
        }
    }
}
