namespace WistBot.Exceptions
{
    public class ListNotFoundException : Exception
    {
        public ListNotFoundException(string name) : base($"List with name {name} not found.")
        {
        }
    }
}
