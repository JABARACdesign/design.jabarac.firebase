using Firebase.Storage;
using JABARACdesign.Base.Domain.Entity.Helper;

namespace JABARACdesign.Firebase.Infrastructure.Network.Initializer
{
    /// <summary>
    /// CloudStorageの初期化クラス
    /// </summary>
    public class CloudStorageInitializer : ICloudStorageInitializer
    {
        private FirebaseStorage _storage;
        
        public FirebaseStorage Storage => _storage;
        
        /// <summary>
        /// CloudStorageの初期化
        /// </summary>
        public void Initialize()
        {
            _storage = FirebaseStorage.DefaultInstance;
            LogHelper.Debug(message: "CloudStorageを初期化しました。");
        }
    }
}