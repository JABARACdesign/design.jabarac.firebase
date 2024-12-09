using Firebase.Auth;
using Firebase.Database;
using Firebase.Firestore;
using Firebase.Storage;
using VContainer;

namespace JABARACdesign.Firebase.Infrastructure.Network.Initializer
{
    /// <summary>
    /// Firebaseの初期化クラス。
    /// </summary>
    public class FirebaseInitializer : 
        IAuthenticationInitializer,
        IFirestoreInitializer,
        IRealtimeDatabaseInitializer,
        ICloudStorageInitializer
    {
        public FirebaseAuth Auth { get; private set; }
        
        public FirebaseFirestore Firestore { get; private set; }
        
        public FirebaseDatabase Database { get; private set; }
        
        public FirebaseStorage Storage { get; private set; }
        
        public bool IsInitialized { get; private set; }
        
        #region Private Field
        
        private AuthenticationInitializer _authenticationInitializer;
        
        private FirestoreInitializer _firestoreInitializer;
        
        private RealtimeDatabaseInitializer _realtimeDatabaseInitializer;
        
        private CloudStorageInitializer _cloudStorageInitializer;
        
        #endregion
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="authenticationInitializer">Firebaseの認証の初期化クラス</param>
        /// <param name="firestoreInitializer">Firestoreの初期化クラス</param>
        /// <param name="realtimeDatabaseInitializer">RealtimeDatabaseの初期化クラス</param>
        /// <param name="cloudStorageInitializer">CloudStorageの初期化クラス</param>
        [Inject]
        public void Constructor(
            AuthenticationInitializer authenticationInitializer,
            FirestoreInitializer firestoreInitializer,
            RealtimeDatabaseInitializer realtimeDatabaseInitializer,
            CloudStorageInitializer cloudStorageInitializer)
        {
            _authenticationInitializer = authenticationInitializer;
            _firestoreInitializer = firestoreInitializer;
            _realtimeDatabaseInitializer = realtimeDatabaseInitializer;
            _cloudStorageInitializer = cloudStorageInitializer;
        }
        
        /// <summary>
        /// Firebaseを初期化する。
        /// </summary>
        public void Initialize()
        {
            _authenticationInitializer.Initialize();
            _firestoreInitializer.Initialize();
            _realtimeDatabaseInitializer.Initialize();
            _cloudStorageInitializer.Initialize();
            
            Auth = _authenticationInitializer.Auth;
            Firestore = _firestoreInitializer.Firestore;
            Database = _realtimeDatabaseInitializer.Database;
            Storage = _cloudStorageInitializer.Storage;
            
            IsInitialized = true;
        }
        
    }
}