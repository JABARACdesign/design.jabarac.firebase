using Firebase.Functions;
using JABARACdesign.Base.Domain.Entity.Helper;

namespace JABARACdesign.Firebase.Infrastructure.Network.Initializer
{
    /// <summary>
    /// Firebase Functions初期化クラス。
    /// </summary>
    public class FirebaseFunctionsInitializer : IFirebaseFunctionsInitializer
    {
        private FirebaseFunctions _functions;
        
        public FirebaseFunctions Functions => _functions;
        
        /// <summary>
        /// Firebase Functionsを初期化する。
        /// </summary>
        public void Initialize()
        {
            _functions = FirebaseFunctions.DefaultInstance;
            LogHelper.Debug(message: "Firebase Functionsを初期化しました。");
        }
    }
}