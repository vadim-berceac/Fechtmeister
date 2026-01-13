public interface INavMeshState
{
    void Enter(ref NavMeshStateData data, NavMeshCharacterInput input);
    void Update(ref NavMeshStateData data, NavMeshCharacterInput input);
    void Exit(ref NavMeshStateData data, NavMeshCharacterInput input);
}