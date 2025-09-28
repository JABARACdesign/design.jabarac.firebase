using Firebase.Firestore;

namespace JABARACdesign.Firebase.Infrastructure.PathProvider
{
    /// <summary>
    /// Firestoreのパスを提供するインターフェース
    /// </summary>
    public interface IFirestorePathProvider
    {
        /// <summary>
        /// 指定した型に対応するパスを取得する。
        /// </summary>
        /// <typeparam name="T">対象の型</typeparam>
        /// <returns>パス</returns>
        /// <exception cref="NotImplementedException">未実装のエラー</exception>
        public string GetPath<T>();
        
        /// <summary>
        /// 指定した型に対応するドキュメント(レコード)のパスを取得する。
        /// </summary>
        /// <typeparam name="TDto">対象の型</typeparam>
        /// <returns>DocumentReference</returns>
        /// <exception cref="ArgumentException">型が未サポートのエラー</exception>
        public DocumentReference GetDocumentPath<TDto>();
        
        /// <summary>
        /// 指定した型に対応するコレクション(テーブル)のパスを取得する。
        /// </summary>
        /// <typeparam name="TDto">対象の型</typeparam>
        /// <returns>CollectionReference</returns>
        /// <exception cref="ArgumentException">型が未サポートのエラー</exception>
        public CollectionReference GetCollectionPath<TDto>();
    }
}