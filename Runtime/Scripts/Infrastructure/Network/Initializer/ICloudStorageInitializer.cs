using Firebase.Storage;

namespace JABARACdesign.Firebase.Infrastructure.Network.Initializer
{
    /// <summary>
    /// CloudStorageの初期化インターフェース。
    /// </summary>
    public interface ICloudStorageInitializer
    {
        FirebaseStorage Storage { get; }
    }
}