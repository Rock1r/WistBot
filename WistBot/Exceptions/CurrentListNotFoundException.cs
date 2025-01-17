namespace WistBot.Exceptions
{
    public class CurrentListNotFoundException : Exception
    {
        public CurrentListNotFoundException(Guid id) : base($"Lists with id {id} not found in current lists.")
        {
        }
    }
}
