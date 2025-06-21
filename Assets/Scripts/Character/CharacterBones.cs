using System.Runtime.CompilerServices;

public struct CharacterBones
{
    public enum Type
    {
        Hips,
        Head,
        Neck,
        Spine2,
        Spine1,
        Spine0,
        LeftUpperArm,
        RightUpperArm,
        LeftUpperLeg,
        RightUpperLeg,
        LeftLowerArm,
        RightLowerArm,
        LeftLowerLeg,
        RightLowerLeg,
        LeftFoot,
        RightFoot,
        LeftHand,
        RightHand,
    }
    
    public static readonly string[] BoneNames = new string[]
    {
        "Hips",        
        "Head",       
        "Neck",        
        "Spine2",      
        "Spine1",      
        "Spine",       
        "LeftArm",     
        "RightArm",   
        "LeftUpLeg",  
        "RightUpLeg",  
        "LeftForeArm", 
        "RightForeArm",
        "LeftLeg",    
        "RightLeg",    
        "LeftFoot",    
        "RightFoot",   
        "LeftHand",    
        "RightHand"    
    };
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetBoneName(Type boneType)
    {
        return BoneNames[(int)boneType];
    }
}
