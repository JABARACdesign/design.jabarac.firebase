using Firebase.Firestore;
using JABARACdesign.Base.Domain.Entity.Helper;

namespace JABARACdesign.Firebase.Infrastructure.Network.Initializer
{
    /// <summary>
    /// Firestoreの初期化クラス
    /// </summary>
    public class FirestoreInitializer
    {
        private FirebaseFirestore _firestore;
        
        public FirebaseFirestore Firestore => _firestore;
        
        /// <summary>
        /// Firestoreを初期化する
        /// </summary>
        public void Initialize()
        {
            _firestore = FirebaseFirestore.DefaultInstance;
            LogHelper.Debug(message: "Firestoreを初期化しました。");
        }
    }
}