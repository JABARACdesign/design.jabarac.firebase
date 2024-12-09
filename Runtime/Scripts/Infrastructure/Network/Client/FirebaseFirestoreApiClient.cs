using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Firebase.Firestore;
using JABARACdesign.Base.Application.Interface;
using JABARACdesign.Base.Domain.Entity.Helper;
using JABARACdesign.Base.Infrastructure.Dto;
using JABARACdesign.Base.Infrastructure.Network.API;
using JABARACdesign.Base.Infrastructure.Network.Client;
using JABARACdesign.Firebase.Infrastructure.Network.Initializer;
using JabaracDesign.Voick.Infrastructure.PathProvider;
using VContainer;
using StatusCode = JABARACdesign.Base.Domain.Entity.API.APIStatus.Code;

namespace JABARACdesign.Firebase.Infrastructure.Network.Client
{
    /// <summary>
    /// Firestoreのバッチ処理の操作インターフェース
    /// </summary>
    public interface IBatchOperation
    {
        void Apply(WriteBatch batch, IFirestorePathProvider pathProvider);
    }
    
    /// <summary>
    /// バッチ処理におけるデータの追加操作
    /// </summary>
    public class SetOperation<TData> : IBatchOperation
    {
        public TData Data { get; set; }
        
        public void Apply(WriteBatch batch, IFirestorePathProvider pathProvider)
        {
            var docRef = pathProvider.GetDocumentPath<TData>();
            batch.Set(documentReference: docRef, documentData: Data);
        }
    }
    
    /// <summary>
    /// バッチ処理におけるデータの更新操作
    /// </summary>
    public class UpdateOperation<TData> : IBatchOperation
    {
        public string FieldName { get; set; }
        public object FieldValue { get; set; }
        
        public void Apply(WriteBatch batch, IFirestorePathProvider pathProvider)
        {
            var docRef = pathProvider.GetDocumentPath<TData>();
            batch.Update(documentReference: docRef, field: FieldName, value: FieldValue);
        }
    }
    
    /// <summary>
    /// バッチ処理におけるデータの削除操作
    /// </summary>
    public class DeleteOperation<TData> : IBatchOperation
    {
        public void Apply(WriteBatch batch, IFirestorePathProvider pathProvider)
        {
            var docRef = pathProvider.GetDocumentPath<TData>();
            batch.Delete(documentReference: docRef);
        }
    }
    
    /// <summary>
    /// Firestoreのクライアントクラス
    /// </summary>
    public class FirebaseFirestoreApiClient : IUserDataApiClient
    {
        private readonly IFirestoreInitializer _initializer;
        
        private FirebaseFirestore Firestore => _initializer.Firestore;
        
        private readonly IFirestorePathProvider _pathProvider;
        
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="pathProvider">パスプロパイダ</param>
        /// <param name="initializer">Firebaseのイニシャライザ</param>
        [Inject]
        public FirebaseFirestoreApiClient(
            IFirestorePathProvider pathProvider,
            IFirestoreInitializer initializer)
        {
            _pathProvider = pathProvider;
            _initializer = initializer;
        }
        
        /// <summary>
        /// Firestoreからデータを取得する
        /// </summary>
        /// <typeparam name="TData">取得するデータの型</typeparam>
        /// <param name="identifier">ドキュメントの識別子(未指定の場合は、パスプロパイダからドキュメントを特定する)</param>
        /// <returns>APIレスポンス</returns>
        public async UniTask<IAPIResponse<TData>> GetAsync<TData>(string identifier = default)
        {
            var docRef = string.IsNullOrEmpty(value: identifier)
                ? _pathProvider.GetDocumentPath<TData>()
                : _pathProvider.GetCollectionPath<TData>().Document(path: identifier);
            
            try
            {
                var snapshot = await docRef.GetSnapshotAsync();
                
                if (!snapshot.Exists)
                {
                    var errorMessage = $"FireStoreで対象のドキュメントが見つかりませんでした";
                    LogHelper.Error(message: errorMessage);
                    return new APIResponse<TData>(status: StatusCode.Error, data: default, errorMessage: errorMessage);
                }
                
                var data = snapshot.ConvertTo<TData>();
                return new APIResponse<TData>(status: StatusCode.Success, data: data);
            }
            catch (Exception ex)
            {
                var errorMessage = $"FireStoreからデータの取得に失敗しました :{docRef}, {ex.Message}";
                LogHelper.Error(message: errorMessage);
                return new APIResponse<TData>(status: StatusCode.Error, data: default, errorMessage: errorMessage);
            }
        }
        
        /// <summary>
        /// Firestoreにデータを作成する。
        /// </summary>
        /// <typeparam name="TData">作成するデータの型。IDto を実装している必要がある</typeparam>
        /// <param name="data">Firestoreに作成するデータ</param>
        /// <param name="isSpecificId">作成するドキュメントが特定IDをもつか</param>
        /// <returns>
        /// 操作の結果を示すIAPIResponse。
        /// 成功した場合はStatusCode.Success、失敗した場合は StatusCode.Error とエラーメッセージを含む。
        /// </returns>
        public async UniTask<IAPIResponse> CreateAsync<TData>(TData data, bool isSpecificId)
        {
            try
            {
                if (isSpecificId)
                {
                    var docRef = _pathProvider.GetDocumentPath<TData>();
                    await docRef.SetAsync(documentData: data);
                }
                else
                {
                    var collectionRef = _pathProvider.GetCollectionPath<TData>();
                    await collectionRef.AddAsync(documentData: data);
                }
                
                return new APIResponse(status: StatusCode.Success);
            }
            catch (Exception ex)
            {
                var errorMessage = $"FireStoreへのデータの作成に失敗しました:{nameof(TData)}, {ex.Message}";
                LogHelper.Error(message: errorMessage);
                return new APIResponse(status: StatusCode.Error, errorMessage: errorMessage);
            }
        }
        
        /// <summary>
        /// Firestoreのデータを更新する
        /// </summary>
        /// <param name="data">更新するデータ</param>
        /// <typeparam name="TData">更新するデータの型</typeparam>
        /// <returns>操作の結果を示すIAPIResponse</returns>
        public async UniTask<IAPIResponse> UpdateAsync<TData>(TData data)
        {
            var docRef = _pathProvider.GetDocumentPath<TData>();
            try
            {
                await docRef.SetAsync(documentData: data);
                
                return new APIResponse(status: StatusCode.Success);
            }
            catch (Exception ex)
            {
                LogHelper.Error(message: $"FireStoreのデータの更新に失敗しました : {docRef}, {ex.Message}");
                
                return new APIResponse(status: StatusCode.Error);
            }
        }
        
        /// <summary>
        /// Firestoreからデータを削除する
        /// </summary>
        /// <param name="identifier">ドキュメントの識別子</param>
        /// <returns>操作の結果を示すIAPIResponse</returns>
        public async UniTask<IAPIResponse> DeleteAsync<TData>(string identifier)
        {
            var docRef = _pathProvider.GetCollectionPath<TData>().Document(path: identifier);
            try
            {
                await docRef.DeleteAsync();
                return new APIResponse(status: StatusCode.Success);
            }
            catch (Exception ex)
            {
                LogHelper.Error(message: $"FireStoreからデータの削除に失敗しました : {docRef}, {ex.Message}");
                return new APIResponse(status: StatusCode.Error);
            }
        }
        
        /// <summary>
        /// Firestoreのドキュメントが存在するかチェックする
        /// </summary>
        /// <param name="identifier">ドキュメントの識別子(未指定の場合は、パスプロパイダからドキュメントを特定する)</param>
        /// <returns>操作の結果を示すAPIResponse</returns>
        public async UniTask<IAPIResponse<DocumentExistsDto>> ExistsAsync<TData>(string identifier = default)
        {
            var docRef = string.IsNullOrEmpty(value: identifier)
                ? _pathProvider.GetDocumentPath<TData>()
                : _pathProvider.GetCollectionPath<TData>().Document(path: identifier);
            
            try
            {
                var snapshot = await docRef.GetSnapshotAsync();
                return new APIResponse<DocumentExistsDto>(
                    status: StatusCode.Success,
                    data: new DocumentExistsDto(isExists: snapshot.Exists));
            }
            catch (Exception ex)
            {
                LogHelper.Error(message: $"FireStoreのドキュメントの存在チェックに失敗しました : {identifier}, {ex.Message}");
                return new APIResponse<DocumentExistsDto>(
                    status: StatusCode.Error,
                    data: new DocumentExistsDto(isExists: false));
            }
        }
        
        /// <summary>
        /// Firestoreのバッチ書き込みを実行する
        /// </summary>
        /// <param name="batchOperations">バッチ処理の操作</param>
        /// <returns>操作の結果を示すAPIResponse</returns>
        public async UniTask<IAPIResponse> ExecuteBatchAsync(IEnumerable<IBatchOperation> batchOperations)
        {
            try
            {
                var batch = Firestore.StartBatch();
                foreach (var operation in batchOperations)
                {
                    operation.Apply(batch: batch, pathProvider: _pathProvider);
                }
                
                await batch.CommitAsync();
                return new APIResponse(status: StatusCode.Success);
            }
            catch (Exception ex)
            {
                LogHelper.Error(message: $"Firestoreのバッチ処理に失敗しました: {ex.Message}");
                return new APIResponse(status: StatusCode.Error);
            }
        }
    }
}