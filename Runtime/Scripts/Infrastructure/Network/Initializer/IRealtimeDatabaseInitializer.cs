using Firebase.Database;

namespace JABARACdesign.Firebase.Infrastructure.Network.Initializer
{
    /// <summary>
    /// RealtimeDatabaseの初期化インターフェース。
    /// </summary>
    public interface IRealtimeDatabaseInitializer
    {
        FirebaseDatabase Database { get; }
    }
}