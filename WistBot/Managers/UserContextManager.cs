namespace WistBot.Managers
{
    public class UserContextManager
    {
        private static readonly Dictionary<long, UserContext> _userContext = new();

        public static void SetContext(long userId, UserContext userContext)
        {
            _userContext[userId] = userContext;
        }

        public static UserContext GetContext(long userId)
        {
            return _userContext.TryGetValue(userId, out var context) ? context : throw new Exception("UserContext not found");
        }

        public static void ClearContext(long userId)
        {
            _userContext.Remove(userId);
        }
    }
}
