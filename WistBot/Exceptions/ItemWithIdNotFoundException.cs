namespace WistBot.Exceptions
{
    public class ItemWithIdNotFoundException : Exception
    {
        public ItemWithIdNotFoundException(Guid id) : base($"Item with id {id} not found.")
        {
        }
    }
}
