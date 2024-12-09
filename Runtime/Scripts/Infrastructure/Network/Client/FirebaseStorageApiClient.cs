using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Firebase.Storage;
using JABARACdesign.Base.Application.Interface;
using JABARACdesign.Base.Domain.Definition;
using JABARACdesign.Base.Domain.Entity.Helper;
using JABARACdesign.Base.Infrastructure.Network.API;
using JABARACdesign.Base.Infrastructure.Network.Client;
using JABARACdesign.Base.Infrastructure.PathProvider;
using JABARACdesign.Firebase.Infrastructure.Network.Initializer;
using VContainer;
using StatusCode = JABARACdesign.Base.Domain.Entity.API.APIStatus.Code;

namespace JABARACdesign.Firebase.Infrastructure.Network.Client
{
    /// <summary>
    /// FirebaseStorageのクライアントクラス。
    /// </summary>
    public class FirebaseStorageApiClient : ICloudStorageClient
    {
        private readonly ICloudStorageInitializer _initializer;
        private readonly ICloudStoragePathProvider _cloudStoragePathProvider;
        private readonly ILocalPathProvider _localPathProvider;
        
        private FirebaseStorage Storage => _initializer.Storage;
        
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="cloudStoragePathProvider">クラウドストレージのパスプロパイダ</param>
        /// <param name="localPathProvider">ローカルのパスプロパイダ</param>
        /// <param name="initializer">Firebaseのイニシャライザ</param>
        [Inject]
        public FirebaseStorageApiClient(
            ICloudStoragePathProvider cloudStoragePathProvider,
            ICloudStorageInitializer initializer,
            ILocalPathProvider localPathProvider)
        {
            _cloudStoragePathProvider = cloudStoragePathProvider;
            _initializer = initializer;
            _localPathProvider = localPathProvider;
        }
        
        /// <summary>
        /// ファイルをアップロードする。
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <param name="identifier">識別子</param>
        /// <param name="extensionType">拡張子タイプ</param>
        /// <param name="fileType">ファイルタイプ</param>
        /// <param name="cancellationToken">キャンセルトークン</param>
        /// <returns>APIレスポンス</returns>
        public async UniTask<IAPIResponse> UploadFileAsync(
            string userId,
            string identifier,
            StorageDefinition.ExtensionType extensionType,
            StorageDefinition.FileType fileType,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var storageReference =　GetFileReference(
                    identifier: identifier,
                    extensionType: extensionType,
                    fileType: fileType);
                var localFilePath = _localPathProvider.GetFilePath(
                    userId: userId,
                    identifier: identifier,
                    extensionType: extensionType,
                    fileType: fileType);
                await storageReference.PutFileAsync(filePath: localFilePath, cancelToken: cancellationToken);
                return new APIResponse(
                    status: StatusCode.Success);
            }
            catch (Exception ex)
            {
                var errorMessage = $"Cloud Storageへのファイルアップロードに失敗しました: {ex.Message}";
                LogHelper.Error(message: errorMessage);
                return new APIResponse(status: StatusCode.Error, errorMessage: errorMessage);
            }
        }
        
        /// <summary>
        /// ファイルをダウンロードする。
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <param name="identifier">識別子</param>
        /// <param name="extensionType">拡張子タイプ</param>
        /// <param name="fileType">ファイルタイプ</param>
        /// <param name="cancellationToken">キャンセルトークン</param>
        /// <returns>APIレスポンス(ローカルのパス)</returns>
        public async UniTask<IAPIResponse<string>> DownloadFileAsync(
            string userId,
            string identifier,
            StorageDefinition.ExtensionType extensionType,
            StorageDefinition.FileType fileType,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var storageReference =　GetFileReference(
                    identifier: identifier,
                    extensionType: extensionType,
                    fileType: fileType);
                var localFilePath = _localPathProvider.GetFilePath(
                    userId: userId,
                    identifier: identifier,
                    extensionType: extensionType,
                    fileType: fileType);
                await storageReference.GetFileAsync(destinationFilePath: localFilePath, cancelToken: cancellationToken);
                return new APIResponse<string>(status: StatusCode.Success, data: localFilePath);
            }
            catch (Exception ex)
            {
                var errorMessage = $"Cloud Storageからのファイルダウンロードに失敗しました: {ex.Message}";
                LogHelper.Error(message: errorMessage);
                return new APIResponse<string>(status: StatusCode.Error, data: "", errorMessage: errorMessage);
            }
        }
        
        /// <summary>
        /// ファイルの存在チェックを行う。
        /// </summary>
        /// <param name="identifier">識別子</param>
        /// <param name="extensionType">拡張子タイプ</param>
        /// <param name="fileType">ファイルタイプ</param>
        /// <returns>APIレスポンス(ファイルの有無)</returns>
        public async UniTask<IAPIResponse<bool>> FileExistsAsync(
            string identifier,
            StorageDefinition.ExtensionType extensionType,
            StorageDefinition.FileType fileType)
        {
            try
            {
                var storageReference =　GetFileReference(
                    identifier: identifier,
                    extensionType: extensionType,
                    fileType: fileType);
                var result = await storageReference.GetMetadataAsync();
                return string.IsNullOrEmpty(value: result.Path)
                    ? new APIResponse<bool>(status: StatusCode.Error, data: false)
                    : new APIResponse<bool>(status: StatusCode.Success, data: true);
            }
            catch (Exception)
            {
                return new APIResponse<bool>(status: StatusCode.Success, data: false);
            }
        }
        
        /// <summary>
        /// ファイルのリファレンスを取得する。
        /// </summary>
        /// <param name="identifier">識別子</param>
        /// <param name="extensionType">拡張子タイプ</param>
        /// <param name="fileType">ファイルタイプ</param>
        /// <returns>ファイルのリファレンス</returns>
        private StorageReference GetFileReference(
            string identifier,
            StorageDefinition.ExtensionType extensionType,
            StorageDefinition.FileType fileType)
        {
            var url = _cloudStoragePathProvider.GetFilePath(
                identifier: identifier,
                extensionType: extensionType,
                fileType: fileType);
            var storageReference =
                Storage.GetReference(location: url);
            
            return storageReference;
        }
    }
}