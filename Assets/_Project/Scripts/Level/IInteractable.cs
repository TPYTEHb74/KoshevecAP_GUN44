namespace Game.Level
{
    public interface IInteractable
    {
        string InteractionPrompt { get; }
        bool CanInteract { get; }
        void Interact(UnityEngine.GameObject interactor);
    }
}
