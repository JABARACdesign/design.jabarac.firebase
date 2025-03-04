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
        /// <summary>
        /// 初期化設定。
        /// </summary>
        public class InitializeSettings
        {
            public bool IsUseFirestore { get; set; }
            
            public bool IsUseRealtimeDatabase { get; set; }
            
            public bool IsUseCloudStorage { get; set; }
        }
        
        public FirebaseAuth Auth { get; private set; }
        
        public FirebaseFirestore Firestore { get; private set; }
        
        public FirebaseDatabase Database { get; private set; }
        
        public FirebaseStorage Storage { get; private set; }
        
        public bool IsInitialized { get; private set; }
        
        #region Private Field
        
        private InitializeSettings _settings;
        
        private IAuthenticationInitializer _authenticationInitializer;
        
        private IFirestoreInitializer _firestoreInitializer;
        
        private IRealtimeDatabaseInitializer _realtimeDatabaseInitializer;
        
        private ICloudStorageInitializer _cloudStorageInitializer;
        
        #endregion
        
        /// <summary>
        /// コンストラクタ(DI)
        /// </summary>
        /// <param name="settings">初期化設定</param>
        /// <param name="authenticationInitializer">Firebaseの認証の初期化クラス</param>
        /// <param name="firestoreInitializer">Firestoreの初期化クラス</param>
        /// <param name="realtimeDatabaseInitializer">RealtimeDatabaseの初期化クラス</param>
        /// <param name="cloudStorageInitializer">CloudStorageの初期化クラス</param>
        [Inject]
        public void Constructor(
            InitializeSettings settings,
            IAuthenticationInitializer authenticationInitializer,
            IFirestoreInitializer firestoreInitializer,
            IRealtimeDatabaseInitializer realtimeDatabaseInitializer,
            ICloudStorageInitializer cloudStorageInitializer)
        {
            _settings = settings;
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
            Auth = _authenticationInitializer.Auth;
            
            if (_settings.IsUseFirestore)
            {
                _firestoreInitializer.Initialize();
                Firestore = _firestoreInitializer.Firestore;
            }
            
            if (_settings.IsUseRealtimeDatabase)
            {
                _realtimeDatabaseInitializer.Initialize();
                Database = _realtimeDatabaseInitializer.Database;
            }
            
            if (_settings.IsUseCloudStorage)
            {
                _cloudStorageInitializer.Initialize();
                Storage = _cloudStorageInitializer.Storage;
            }
            
            IsInitialized = true;
        }
        
    }
}