//.Net 8.0
//proof of concept for upcoming API restructure
//corresponds to class diagram "API restructure - first draft.pdf"

internal class Program
{
    static void Main(string[] args)
    {
        var UA = API.CreateInstance(new UserAccessToken());
        var AA = API.CreateInstance(new AppAccessToken());

        AA.Ads.TestGeneralFunc();

        UA.Ads.TestGeneralFunc();
        UA.Ads.TestUserAccessFunc();
    }

    public abstract class API
    {
        private Ads? _Ads = null;
        public Ads Ads { get { if (_Ads == null) _Ads = new Ads(this); return _Ads; } }

        public static UserAccessAPI CreateInstance(UserAccessToken Token)
        {
            return new UserAccessAPI(Token);
        }

        public static AppAccessAPI CreateInstance(AppAccessToken Token)
        {
            return new AppAccessAPI(Token);
        }
    }
    public class UserAccessAPI : API
    {
        public UserAccessAPI(UserAccessToken Token) { }

        private Ads_UserAccess? _Ads = null;
        public new Ads_UserAccess Ads //hide base member
        {
            get
            {
                if(_Ads == null) _Ads= new Ads_UserAccess(this);
                return _Ads;
            }
        }
    }
    public class AppAccessAPI : API
    {
        public AppAccessAPI(AppAccessToken Token) { }
    }

    public class Ads
    {
        protected API Parent { get; }
        internal Ads(API Parent)
        {
            this.Parent = Parent;
        } 
        public void TestGeneralFunc() { }
    }
    public class Ads_UserAccess : Ads
    {
        internal Ads_UserAccess(UserAccessAPI Parent) : base(Parent) { }
        public void TestUserAccessFunc() { }
    }

    #region New Token Stuff
    public class Token { }
    public class UserAccessToken : Token { }
    public class AppAccessToken : Token { }
    #endregion
}