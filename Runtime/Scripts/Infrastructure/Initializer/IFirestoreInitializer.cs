using Firebase.Firestore;

namespace JABARACdesign.Firebase.Infrastructure.Network.Initializer
{
    /// <summary>
    /// Firestoreの初期化インターフェース。
    /// </summary>
    public interface IFirestoreInitializer
    {
        FirebaseFirestore Firestore { get; }
        
        void Initialize();
    }
}