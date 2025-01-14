namespace Dynamic.Core.Authentication
{
    public class BannedPasswordList : List<string>
    {
        // Singleton implementation, just only load it once. 
        private BannedPasswordList() { }
        private static BannedPasswordList? _instance;

        public static BannedPasswordList Passwords
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BannedPasswordList();
                    _instance.AddRange(File.ReadAllLines("bannedpasswordlist.txt"));
                }
                return _instance;
            }
        }
    }
}