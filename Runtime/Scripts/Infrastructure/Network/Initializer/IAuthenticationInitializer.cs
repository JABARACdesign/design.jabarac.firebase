using Firebase.Auth;

namespace JABARACdesign.Firebase.Infrastructure.Network.Initializer
{
    /// <summary>
    /// Firebase Authenticationの初期化インターフェース。
    /// </summary>
    public interface IAuthenticationInitializer
    {
        public FirebaseAuth Auth { get; }
    }
}
