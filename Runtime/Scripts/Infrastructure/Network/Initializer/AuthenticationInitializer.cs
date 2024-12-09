using Firebase.Auth;
using JABARACdesign.Base.Domain.Entity.Helper;

namespace JABARACdesign.Firebase.Infrastructure.Network.Initializer
{
    /// <summary>
    /// Firebaseの認証の初期化クラス
    /// </summary>
    public class AuthenticationInitializer : IAuthenticationInitializer
    {
        private FirebaseAuth _auth;
        
        public FirebaseAuth Auth => _auth;
        
        /// <summary>
        /// Firebaseの認証を初期化する
        /// </summary>
        public void Initialize()
        {
            _auth = FirebaseAuth.DefaultInstance;
            LogHelper.Debug(message: "Firebaseの認証を初期化しました。");
        }
    }
}