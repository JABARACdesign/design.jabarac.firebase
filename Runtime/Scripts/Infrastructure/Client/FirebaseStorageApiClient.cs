using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Firebase.Storage;
using JABARACdesign.Base.Application.Interface;
using JABARACdesign.Base.Domain.Definition;
using JABARACdesign.Base.Domain.Entity.Helper;
using JABARACdesign.Base.Infrastructure.API;
using JABARACdesign.Base.Infrastructure.Client;
using JABARACdesign.Firebase.Infrastructure.Network.Initializer;
using VContainer;

namespace JABARACdesign.Firebase.Infrastructure.Client
{
    /// <summary>
    /// FirebaseStorageのクライアントクラス。
    /// </summary>
    public class FirebaseStorageApiClient : ICloudStorageClient
    {
        private readonly ICloudStorageInitializer _initializer;
        private readonly IPathProvider _pathProvider;
        
        private FirebaseStorage Storage => _initializer.Storage;
        
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="pathProvider">ローカルのパスプロパイダ</param>
        /// <param name="initializer">Firebaseのイニシャライザ</param>
        [Inject]
        public FirebaseStorageApiClient(
            ICloudStorageInitializer initializer,
            IPathProvider pathProvider)
        {
            _initializer = initializer;
            _pathProvider = pathProvider;
        }
        
        /// <summary>
        /// ファイルをアップロードする。
        /// </summary>
        /// <typeparam name="TEnum">拡張子タイプの列挙型</typeparam>
        /// <param name="identifier">識別子</param>
        /// <param name="cancellationToken">キャンセルトークン</param>
        /// <returns>APIレスポンス</returns>
        public async UniTask<IAPIResponse> UploadFileAsync<TEnum>(
            TEnum identifier,
            CancellationToken cancellationToken = default)
            where TEnum  : struct, Enum
        {
            try
            {
                var path = _pathProvider.GetPath(identifier: identifier);
                var localPath = _pathProvider.GetLocalPath(identifier: identifier);
                var storageReference = GetFileReference(path);
                await storageReference.PutFileAsync(
                    filePath: localPath, cancelToken: 
                    cancellationToken);
                return new APIResponse(
                    status: APIDefinition.Code.Success);
            }
            catch (Exception ex)
            {
                var errorMessage = $"Cloud Storageへのファイルアップロードに失敗しました: {ex.Message}";
                LogHelper.Error(message: errorMessage);
                return new APIResponse(status: APIDefinition.Code.Error, errorMessage: errorMessage);
            }
        }
        
        /// <summary>
        /// ファイルをダウンロードする。
        /// </summary>
        /// <param name="identifier">識別子</param>
        /// <param name="cancellationToken">キャンセルトークン</param>
        /// <returns>APIレスポンス(ローカルのパス)</returns>
        public async UniTask<IAPIResponse<string>> DownloadFileAsync<TEnum>(
            TEnum identifier,
            CancellationToken cancellationToken = default)
        where TEnum : struct, Enum
        {
            try
            {
                var path = _pathProvider.GetPath(identifier: identifier);
                var localPath = _pathProvider.GetLocalPath(identifier: identifier);
                var storageReference = GetFileReference(path);
                await storageReference.GetFileAsync(
                    destinationFilePath: localPath, 
                    cancelToken: cancellationToken);
                
                return new APIResponse<string>(status: APIDefinition.Code.Success, data: path);
            }
            catch (Exception ex)
            {
                var errorMessage = $"Cloud Storageからのファイルダウンロードに失敗しました: {ex.Message}";
                LogHelper.Error(message: errorMessage);
                return new APIResponse<string>(status: APIDefinition.Code.Error, data: "", errorMessage: errorMessage);
            }
        }
        
        /// <summary>
        /// ファイルの存在チェックを行う。
        /// </summary>
        /// <param name="identifier">識別子</param>
        /// <returns>APIレスポンス(ファイルの有無)</returns>
        public async UniTask<IAPIResponse<bool>> FileExistsAsync<TEnum>(TEnum identifier)
        where TEnum : struct, Enum
        {
            try
            {
                var path = _pathProvider.GetPath(identifier: identifier);
                var storageReference = GetFileReference(path);
                var result = await storageReference.GetMetadataAsync();
                return string.IsNullOrEmpty(value: result.Path)
                    ? new APIResponse<bool>(status: APIDefinition.Code.Error, data: false)
                    : new APIResponse<bool>(status: APIDefinition.Code.Success, data: true);
            }
            catch (Exception)
            {
                return new APIResponse<bool>(status: APIDefinition.Code.Error, data: false);
            }
        }
        
        /// <summary>
        /// ファイルのリファレンスを取得する。
        /// </summary>
        /// <param name="path">ファイルのパス</param>
        /// <returns>ファイルのリファレンス</returns>
        private StorageReference GetFileReference(string path)
        {
            // Firebase Storageは相対パスを期待するため、先頭の/を削除
            var relativePath = path.StartsWith("/") ? path.Substring(1) : path;
            var storageReference = Storage.GetReference(location: relativePath);
            
            return storageReference;
        }
    }
}