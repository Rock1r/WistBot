namespace WistBot.Exceptions
{
    public class ListNotFoundException : Exception
    {
        public ListNotFoundException(string name) : base($"Lists with name {name} not found.")
        {
        }
    }
}
