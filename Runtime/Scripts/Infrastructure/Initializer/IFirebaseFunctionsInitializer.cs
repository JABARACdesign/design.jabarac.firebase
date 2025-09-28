using Firebase.Functions;

namespace JABARACdesign.Firebase.Infrastructure.Network.Initializer
{
    /// <summary>
    /// Firebase Functions初期化インターフェース。
    /// </summary>
    public interface IFirebaseFunctionsInitializer
    {
        FirebaseFunctions Functions { get; }
        
        void Initialize();
    }
}