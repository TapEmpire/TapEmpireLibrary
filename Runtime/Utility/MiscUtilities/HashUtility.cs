namespace TapEmpire.Utility
{
    public static class HashUtility
    {
        public static int GetStableHashCode(string value)
        {
            unchecked
            {
                int hash = 5381;
                foreach (var character in value)
                {
                    hash = ((hash << 5) + hash) + character;
                }
                return hash;
            }
        }
    }
}
