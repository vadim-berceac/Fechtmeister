using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MicahW.PointGrass {
    
    [ExecuteAlways]
    public class PointGrassWind : MonoBehaviour {
        [Tooltip("The scale of the sampled noise")] 
        public float windScale = 1f;
        
        [Tooltip("The range of the sampled noise")] 
        public Vector2 noiseRange = new Vector2(0f, 1f);
        
        [Space()]
        [Tooltip("The direction the wind will push the grass")] 
        public Vector3 windDirection = Vector3.forward;
        
        [Tooltip("The distance the sampled noise moves each second")] 
        public Vector3 windScroll = Vector3.one;
        
        private Vector3 currentNoisePosition;
        private Vector4 cachedVecA;
        private Vector4 cachedVecB;
        private float cachedValA;
       
        private bool needsUpdate = true;
        
        private Vector3 lastWindDirection;
        private Vector3 lastWindScroll;
        private Vector2 lastNoiseRange;
        private float lastWindScale;

        private static int ID_vecA = -1;
        private static int ID_vecB = -1;
        private static int ID_valA = -1;

#if UNITY_EDITOR
        private double previousEditorTime = 0f;
        private const float EDITOR_UPDATE_INTERVAL = 0.016f; 
        private float editorUpdateAccumulator = 0f;
#endif

        private void Awake() {
            InitializeShaderIDs();
        }

#if UNITY_EDITOR
        private void OnValidate() {
            InitializeShaderIDs();
            needsUpdate = true;
        }
#endif

        private void OnEnable() {
            InitializeShaderIDs();
           
            currentNoisePosition = Vector3.zero;
            needsUpdate = true;
        
            CacheCurrentValues();
            UpdateShaderProperties();

#if UNITY_EDITOR
            if (!Application.isPlaying) {
                previousEditorTime = EditorApplication.timeSinceStartup;
                editorUpdateAccumulator = 0f;
                EditorApplication.update += EditorUpdate;
            }
#endif
        }

#if UNITY_EDITOR
        private void OnDisable() {
            if (!Application.isPlaying) {
                EditorApplication.update -= EditorUpdate;
            }
        }

        private void EditorUpdate() {
            float timeStep = (float)(EditorApplication.timeSinceStartup - previousEditorTime);
            previousEditorTime = EditorApplication.timeSinceStartup;
            
            editorUpdateAccumulator += timeStep;
            
            if (editorUpdateAccumulator >= EDITOR_UPDATE_INTERVAL) {
                UpdateWindPosition(editorUpdateAccumulator);
                editorUpdateAccumulator = 0f;
            }
        }
#endif

        private void LateUpdate() {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            UpdateWindPosition(Time.deltaTime);
        }

        private static void InitializeShaderIDs() {
            if (ID_vecA != -1) return;
            
            ID_vecA = Shader.PropertyToID("_PG_VectorA");
            ID_vecB = Shader.PropertyToID("_PG_VectorB");
            ID_valA = Shader.PropertyToID("_PG_ValueA");
        }

       
        private void UpdateWindPosition(float deltaTime) {
            currentNoisePosition.x += windScroll.x * deltaTime;
            currentNoisePosition.y += windScroll.y * deltaTime;
            currentNoisePosition.z += windScroll.z * deltaTime;
            
            CheckForChanges();
            
            if (needsUpdate) {
                UpdateShaderProperties();
                needsUpdate = false;
            }
        }

        private void CheckForChanges() {
            if (needsUpdate) return;

            const float epsilon = 0.0001f;
            
            if (Vector3.SqrMagnitude(windDirection - lastWindDirection) > epsilon ||
                Vector3.SqrMagnitude(windScroll - lastWindScroll) > epsilon ||
                Mathf.Abs(noiseRange.x - lastNoiseRange.x) > epsilon ||
                Mathf.Abs(noiseRange.y - lastNoiseRange.y) > epsilon ||
                Mathf.Abs(windScale - lastWindScale) > epsilon) {
                
                needsUpdate = true;
            }
        }
        
        private void CacheCurrentValues() {
            lastWindDirection = windDirection;
            lastWindScroll = windScroll;
            lastNoiseRange = noiseRange;
            lastWindScale = windScale;
        }
        
        private void UpdateShaderProperties() {
            cachedVecA.x = windDirection.x;
            cachedVecA.y = windDirection.y;
            cachedVecA.z = windDirection.z;
            cachedVecA.w = noiseRange.x;
            
            cachedVecB.x = currentNoisePosition.x;
            cachedVecB.y = currentNoisePosition.y;
            cachedVecB.z = currentNoisePosition.z;
            cachedVecB.w = noiseRange.y;
            
            cachedValA = windScale;
            
            Shader.SetGlobalVector(ID_vecA, cachedVecA);
            Shader.SetGlobalVector(ID_vecB, cachedVecB);
            Shader.SetGlobalFloat(ID_valA, cachedValA);
            
            CacheCurrentValues();
        }
        
        public void ForceRefresh() {
            needsUpdate = true;
            UpdateShaderProperties();
        }

        public void SetWindDirection(Vector3 direction) {
            windDirection = direction;
            needsUpdate = true;
        }
        
        public void SetWindScroll(Vector3 scroll) {
            windScroll = scroll;
            needsUpdate = true;
        }

        
        public void ResetNoisePosition() {
            currentNoisePosition = Vector3.zero;
            needsUpdate = true;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.cyan;
            Vector3 start = transform.position;
            Vector3 end = start + windDirection.normalized * 5f;
            Gizmos.DrawLine(start, end);
            Gizmos.DrawSphere(end, 0.2f);
            
            // Визуализация скорости прокрутки
            Gizmos.color = Color.yellow;
            Vector3 scrollEnd = start + windScroll.normalized * 3f;
            Gizmos.DrawLine(start, scrollEnd);
        }
#endif
    }
}