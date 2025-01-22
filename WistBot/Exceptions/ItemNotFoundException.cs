namespace WistBot.Exceptions
{
    public class ItemNotFoundException : Exception
    {
        public ItemNotFoundException(string name) : base($"Item with name {name} not found.")
        {
        }
    }
}
