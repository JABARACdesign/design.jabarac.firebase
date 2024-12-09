using Firebase.Database;
using JABARACdesign.Base.Domain.Entity.Helper;

namespace JABARACdesign.Firebase.Infrastructure.Network.Initializer
{
    /// <summary>
    /// RealtimeDatabaseの初期化クラス
    /// </summary>
    public class RealtimeDatabaseInitializer
    {
        private FirebaseDatabase _database;
        
        public FirebaseDatabase Database => _database;
        
        /// <summary>
        /// RealtimeDatabaseの初期化
        /// </summary>
        public void Initialize()
        {
            _database = FirebaseDatabase.DefaultInstance;
            LogHelper.Debug(message: "RealtimeDatabaseを初期化しました。");
        }
    }
}