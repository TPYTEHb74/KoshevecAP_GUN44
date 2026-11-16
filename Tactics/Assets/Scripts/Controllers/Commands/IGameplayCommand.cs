public interface IGameplayCommand
{
    void Interact(Cell cell);
    void Cancel();
    bool Confirm();
    void ResetCommand();
}
