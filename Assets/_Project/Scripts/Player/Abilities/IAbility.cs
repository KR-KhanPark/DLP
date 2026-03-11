namespace DLP.Player.Abilities
{
    public interface IAbility
    {
        int Priority { get; }
        void Initialize(AbilityContext ctx);
        void OnUpdate();
        void OnFixedUpdate();
        void OnMoveInput(UnityEngine.Vector2 move);
        bool OnJumpPressed();
        void OnJumpHeld(bool held);
    }
}