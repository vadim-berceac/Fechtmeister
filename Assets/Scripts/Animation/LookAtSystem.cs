using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class LookAtSystem
{
    private LookAtBoneConfig[] boneConfigs;
    private Animator animator;
    
    private AnimationScriptPlayable lookAtPlayable;
    private NativeArray<BoneLookAtData> bonesData;
    private bool isInitialized;
    
    public bool IsInitialized => isInitialized;
    
    public LookAtSystem(LookAtBoneConfig[] configs, Animator animator)
    {
        this.boneConfigs = configs;
        this.animator = animator;
    }
    
    /// <summary>
    /// Инициализация и интеграция в граф
    /// </summary>
    public AnimationScriptPlayable Initialize(PlayableGraph graph, Playable sourcePlayable)
    {
        if (boneConfigs == null || boneConfigs.Length == 0)
        {
            Debug.LogWarning("LookAt: No bones configured!");
            return default;
        }
        
        // Создаем массив данных
        bonesData = new NativeArray<BoneLookAtData>(boneConfigs.Length, Allocator.Persistent);
        
        // Инициализация костей
        for (int i = 0; i < boneConfigs.Length; i++)
        {
            var config = boneConfigs[i];
            
            config.bone = animator.GetBoneTransform(config.humanBone);
            
            if (config.bone == null)
            {
                Debug.LogWarning($"LookAt: Bone {config.humanBone} not found!");
                continue;
            }
            
            config.targetPosArray = new NativeArray<Vector3>(1, Allocator.Persistent);
            if (config.target != null)
            {
                config.targetPosArray[0] = config.target.position;
            }
            
            bonesData[i] = new BoneLookAtData
            {
                boneHandle = animator.BindStreamTransform(config.bone),
                targetPosition = config.targetPosArray,
                weight = config.weight,
                aimAxis = config.aimAxis,
                upAxis = config.upAxis,
                maxAngle = config.maxAngle,
                minVerticalAngle = config.useAdvancedConstraints ? config.minVerticalAngle : -180f,
                maxVerticalAngle = config.useAdvancedConstraints ? config.maxVerticalAngle : 180f,
                minHorizontalAngle = config.useAdvancedConstraints ? config.minHorizontalAngle : -180f,
                maxHorizontalAngle = config.useAdvancedConstraints ? config.maxHorizontalAngle : 180f
            };
        }
        
        // Создаем Job
        var lookAtJob = new MultiLookAtJob
        {
            bones = bonesData
        };
        
        // Создаем Playable
        lookAtPlayable = AnimationScriptPlayable.Create(graph, lookAtJob, 1);
        
        // Подключаем источник
        graph.Connect(sourcePlayable, 0, lookAtPlayable, 0);
        lookAtPlayable.SetInputWeight(0, 1f);
        
        isInitialized = true;
        
        return lookAtPlayable;
    }
    
    /// <summary>
    /// Обновление данных (вызывать каждый кадр)
    /// </summary>
    public void Update()
    {
        if (!isInitialized || !lookAtPlayable.IsValid()) return;
        
        // Обновляем данные костей
        for (int i = 0; i < boneConfigs.Length; i++)
        {
            var config = boneConfigs[i];
            
            if (config.bone == null) continue;
            
            var data = bonesData[i];
            
            // Обновляем позицию цели
            if (config.target != null && config.targetPosArray.IsCreated)
            {
                config.targetPosArray[0] = config.target.position;
            }
            
            // Обновляем параметры
            data.weight = config.weight;
            data.maxAngle = config.maxAngle;
            
            if (config.useAdvancedConstraints)
            {
                data.minVerticalAngle = config.minVerticalAngle;
                data.maxVerticalAngle = config.maxVerticalAngle;
                data.minHorizontalAngle = config.minHorizontalAngle;
                data.maxHorizontalAngle = config.maxHorizontalAngle;
            }
            else
            {
                data.minVerticalAngle = -180f;
                data.maxVerticalAngle = 180f;
                data.minHorizontalAngle = -180f;
                data.maxHorizontalAngle = 180f;
            }
            
            bonesData[i] = data;
        }
        
        // Применяем к Job
        var job = lookAtPlayable.GetJobData<MultiLookAtJob>();
        job.bones = bonesData;
        lookAtPlayable.SetJobData(job);
    }
    
    /// <summary>
    /// Очистка ресурсов
    /// </summary>
    public void Dispose()
    {
        if (boneConfigs != null)
        {
            for (int i = 0; i < boneConfigs.Length; i++)
            {
                if (boneConfigs[i].targetPosArray.IsCreated)
                {
                    boneConfigs[i].targetPosArray.Dispose();
                }
            }
        }
        
        if (bonesData.IsCreated)
        {
            bonesData.Dispose();
        }
        
        isInitialized = false;
    }
    
    // ==================== PUBLIC API ====================
    
    /// <summary>
    /// Установить вес для кости
    /// </summary>
    public void SetBoneWeight(HumanBodyBones humanBone, float weight)
    {
        if (!isInitialized || boneConfigs == null) return;
        
        int index = GetBoneIndex(humanBone);
        if (index >= 0)
        {
            boneConfigs[index].weight = Mathf.Clamp01(weight);
        }
    }
    
    /// <summary>
    /// Получить текущий вес кости
    /// </summary>
    public float GetBoneWeight(HumanBodyBones humanBone)
    {
        if (!isInitialized || boneConfigs == null) return 0f;
        
        int index = GetBoneIndex(humanBone);
        if (index >= 0)
        {
            return boneConfigs[index].weight;
        }
        return 0f;
    }
    
    private int GetBoneIndex(HumanBodyBones humanBone)
    {
        for (int i = 0; i < boneConfigs.Length; i++)
        {
            if (boneConfigs[i].humanBone == humanBone)
            {
                return i;
            }
        }
        return -1;
    }
}