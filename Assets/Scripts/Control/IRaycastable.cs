namespace RPG.Control
{
    public interface IRaycastable
    {
        CursorType GetCursorType();
        bool HandleRaycast(PlayerControl callingController);
    }
}
