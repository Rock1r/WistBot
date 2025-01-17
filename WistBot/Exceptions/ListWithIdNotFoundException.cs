namespace WistBot.Exceptions
{
    public class ListWithIdNotFoundException : Exception
    {
        public ListWithIdNotFoundException(Guid id) : base($"Lists with name {id} not found.")
        {
        }
    }
}
