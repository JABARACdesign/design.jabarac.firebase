using System;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using JABARACdesign.Base.Application.Interface;
using JABARACdesign.Base.Domain.Definition;
using JABARACdesign.Base.Domain.Entity.API;
using JABARACdesign.Base.Domain.Entity.Helper;
using JABARACdesign.Base.Domain.Interface;
using JABARACdesign.Base.Infrastructure.Network.API;
using JABARACdesign.Base.Infrastructure.Network.Client;
using JABARACdesign.Firebase.Infrastructure.Network.Initializer;
using UnityEngine.Networking;
using Newtonsoft.Json;
using VContainer;

namespace JABARACdesign.Firebase.Infrastructure.Network.Client
{
    /// <summary>
    /// Firebase Functions APIクライアントクラス。
    /// UnityWebRequestを使用して実装。
    /// Functions SDKに依存すると、他の手段との代替性が低下するため。
    /// </summary>
    public class FirebaseFunctionsApiClient : IFunctionApiClient
    {
        private readonly IAuthenticationInitializer _initializer;
        
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="initializer">イニシャライザ</param>
        [Inject]
        public FirebaseFunctionsApiClient(IAuthenticationInitializer initializer)
        {
            _initializer = initializer;
        }
        
        /// <summary>
        /// データを取得するFunction呼び出し
        /// </summary>
        /// <typeparam name="TResponseData">レスポンスデータの型</typeparam>
        /// <param name="request">APIリクエスト</param>
        /// <param name="cancellationToken">キャンセルトークン</param>
        /// <returns>APIレスポンス</returns>
        public async UniTask<IAPIResponse<TResponseData>> SendAsync<TResponseData>(
            IAPIRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // リクエストURLの構築
                var url = request.Uri;
                
                // UnityWebRequestの作成
                using var webRequest = await CreateWebRequestAsync(url, request.MethodType, "{}");
                
                // リクエスト送信
                await webRequest.SendWebRequest().ToUniTask(cancellationToken: cancellationToken);
                
                // レスポンスの処理
                return ProcessResponse<TResponseData>(webRequest, request.Uri);
            }
            catch (Exception ex)
            {
                return HandleException<TResponseData>(ex, request.Uri);
            }
        }
        
        /// <summary>
        /// データ付きリクエストを送信するFunction呼び出し
        /// </summary>
        /// <typeparam name="TDto">リクエストDTOの型</typeparam>
        /// <typeparam name="TResponseData">レスポンスデータの型</typeparam>
        /// <param name="request">DTO付きAPIリクエスト</param>
        /// <param name="cancellationToken">キャンセルトークン</param>
        /// <returns>APIレスポンス</returns>
        public async UniTask<IAPIResponse<TResponseData>> SendAsync<TDto, TResponseData>(
            IApiRequest<TDto> request,
            CancellationToken cancellationToken = default)
        where TDto : IQueryParamConvertible
        {
            try
            {
                string url = request.Uri;
                string jsonData = null;
                
                // GETリクエストの場合は、DTOをクエリパラメータに変換
                if (request.MethodType == APIDefinition.HttpMethodType.GET)
                {
                    var queryParams = request.Dto.ToQueryParams();
                    if (!string.IsNullOrEmpty(queryParams))
                    {
                        // URLに?が含まれているかチェックして、適切な接続文字を選択
                        url = url.Contains("?") ? $"{url}&{queryParams}" : $"{url}?{queryParams}";
                    }
                }
                else
                {
                    // GET以外のメソッドはJSONボディを使用
                    jsonData = JsonConvert.SerializeObject(value: request.Dto);
                }
                
                // UnityWebRequestの作成
                using var webRequest = await CreateWebRequestAsync(url, request.MethodType, jsonData);
                
                // リクエスト送信
                await webRequest.SendWebRequest().ToUniTask(cancellationToken: cancellationToken);
                
                // レスポンスの処理
                return ProcessResponse<TResponseData>(webRequest, request.Uri);
            }
            catch (Exception ex)
            {
                return HandleException<TResponseData>(ex, request.Uri);
            }
        }
        
        /// <summary>
        /// レスポンスデータ不要のFunction呼び出し
        /// </summary>
        /// <param name="request">APIリクエスト</param>
        /// <param name="cancellationToken">キャンセルトークン</param>
        /// <returns>APIレスポンス</returns>
        public async UniTask<IAPIResponse> SendAsync(
            IAPIRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // リクエストURLの構築
                var url = request.Uri;
                
                // UnityWebRequestの作成
                using var webRequest = await CreateWebRequestAsync(url, request.MethodType, "{}");
                
                // リクエスト送信
                await webRequest.SendWebRequest().ToUniTask(cancellationToken: cancellationToken);
                
                // エラー処理
                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    return new APIResponse(
                        status: APIStatus.Code.Error,
                        errorMessage: $"HTTP エラー: {webRequest.error}");
                }
                
                return new APIResponse(status: APIStatus.Code.Success);
            }
            catch (Exception ex)
            {
                return HandleExceptionWithoutResponse(ex, request.Uri);
            }
        }
        
        /// <summary>
        /// レスポンスデータ不要のDTO付きFunction呼び出し
        /// </summary>
        /// <typeparam name="TDto">リクエストDTOの型</typeparam>
        /// <param name="request">DTO付きAPIリクエスト</param>
        /// <param name="cancellationToken">キャンセルトークン</param>
        /// <returns>APIレスポンス</returns>
        public async UniTask<IAPIResponse> SendAsync<TDto>(
            IApiRequest<TDto> request,
            CancellationToken cancellationToken = default)
        where TDto : IQueryParamConvertible
        {
            try
            {
                string url = request.Uri;
                string jsonData = null;
                
                // GETリクエストの場合は、DTOをクエリパラメータに変換
                if (request.MethodType == APIDefinition.HttpMethodType.GET)
                {
                    var queryParams = request.Dto.ToQueryParams();
                    if (!string.IsNullOrEmpty(queryParams))
                    {
                        // URLに?が含まれているかチェックして、適切な接続文字を選択
                        url = url.Contains("?") ? $"{url}&{queryParams}" : $"{url}?{queryParams}";
                    }
                }
                else
                {
                    // GET以外のメソッドはJSONボディを使用
                    jsonData = JsonConvert.SerializeObject(value: request.Dto);
                }
                
                // UnityWebRequestの作成
                using var webRequest = await CreateWebRequestAsync(url, request.MethodType, jsonData);
                
                // リクエスト送信
                await webRequest.SendWebRequest().ToUniTask(cancellationToken: cancellationToken);
                
                // エラー処理
                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    return new APIResponse(
                        status: APIStatus.Code.Error,
                        errorMessage: $"HTTP エラー: {webRequest.error}");
                }
                
                return new APIResponse(status: APIStatus.Code.Success);
            }
            catch (Exception ex)
            {
                return HandleExceptionWithoutResponse(ex, request.Uri);
            }
        }
        
        
        /// <summary>
        /// UnityWebRequestを作成するメソッド
        /// </summary>
        private async UniTask<UnityWebRequest> CreateWebRequestAsync(string url, APIDefinition.HttpMethodType methodType, string jsonData)
        {
            UnityWebRequest webRequest;
            
            switch (methodType)
            {
                case APIDefinition.HttpMethodType.GET:
                    webRequest = UnityWebRequest.Get(url);
                    break;
                    
                case APIDefinition.HttpMethodType.POST:
                    webRequest = new UnityWebRequest(url, "POST");
                    if (!string.IsNullOrEmpty(jsonData))
                    {
                        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                        webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                        webRequest.downloadHandler = new DownloadHandlerBuffer();
                        webRequest.SetRequestHeader("Content-Type", "application/json");
                    }
                    else
                    {
                        webRequest.downloadHandler = new DownloadHandlerBuffer();
                    }
                    break;
                    
                case APIDefinition.HttpMethodType.PUT:
                    webRequest = new UnityWebRequest(url, "PUT");
                    if (!string.IsNullOrEmpty(jsonData))
                    {
                        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                        webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                        webRequest.downloadHandler = new DownloadHandlerBuffer();
                        webRequest.SetRequestHeader("Content-Type", "application/json");
                    }
                    else
                    {
                        webRequest.downloadHandler = new DownloadHandlerBuffer();
                    }
                    break;
                    
                case APIDefinition.HttpMethodType.DELETE:
                    webRequest = UnityWebRequest.Delete(url);
                    webRequest.downloadHandler = new DownloadHandlerBuffer();
                    break;
                    
                default:
                    throw new ArgumentException($"サポートされていないHTTPメソッド: {methodType}");
            }
            
            // Firebase認証トークンをヘッダーに追加
            await SetAuthorizationHeaderAsync(webRequest);
            
            return webRequest;
        }
        
        /// <summary>
        /// 認証ヘッダーを設定するメソッド
        /// </summary>
        private async UniTask SetAuthorizationHeaderAsync(UnityWebRequest webRequest)
        {
            try
            {
                // 現在のユーザーがいるか確認
                if (_initializer.Auth.CurrentUser != null)
                {
                    // 非同期でトークンを取得
                    string token = await GetAuthTokenAsync();
                    if (!string.IsNullOrEmpty(token))
                    {
                        webRequest.SetRequestHeader("Authorization", "Bearer " + token);
                        LogHelper.Debug("認証ヘッダーを設定しました。");
                    }
                    else
                    {
                        LogHelper.Error("認証トークンが空でした。");
                    }
                }
                else
                {
                    LogHelper.Warning("認証されたユーザーがいません。");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error($"認証ヘッダーの設定に失敗しました: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 認証トークンを非同期で取得するメソッド
        /// </summary>
        /// <returns>認証トークン</returns>
        private async UniTask<string> GetAuthTokenAsync()
        {
            try
            {
                // TaskをUniTaskに変換して待機
                var tokenTask = _initializer.Auth.CurrentUser.TokenAsync(forceRefresh: false).AsUniTask();
                return await tokenTask;
            }
            catch (Exception ex)
            {
                LogHelper.Error($"認証トークンの取得に失敗しました: {ex.Message}");
                return string.Empty;
            }
        }
        
        /// <summary>
        /// レスポンス処理の共通メソッド
        /// </summary>
        private IAPIResponse<TResponseData> ProcessResponse<TResponseData>(
            UnityWebRequest webRequest, 
            string uri)
        {
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                LogHelper.Error(message: $"リクエストエラー: {uri}, {webRequest.error}");
                return new APIResponse<TResponseData>(
                    status: APIStatus.Code.Error,
                    data: default,
                    errorMessage: $"HTTP エラー: {webRequest.error}");
            }
            
            try
            {
                var resultJson = webRequest.downloadHandler.text;
                var responseData = JsonConvert.DeserializeObject<TResponseData>(value: resultJson);
                return new APIResponse<TResponseData>(
                    status: APIStatus.Code.Success,
                    data: responseData);
            }
            catch (JsonException ex)
            {
                LogHelper.Error(message: $"レスポンスのデシリアライズに失敗: {uri}, {ex.Message}");
                return new APIResponse<TResponseData>(
                    status: APIStatus.Code.Error,
                    data: default,
                    errorMessage: "レスポンス形式が不正です");
            }
        }
        
        /// <summary>
        /// 例外処理の共通メソッド
        /// </summary>
        private IAPIResponse<TResponseData> HandleException<TResponseData>(
            Exception ex, 
            string uri)
        {
            if (ex is OperationCanceledException)
            {
                LogHelper.Error(message: $"リクエストがキャンセルされました: {uri}");
                return new APIResponse<TResponseData>(
                    status: APIStatus.Code.Error,
                    data: default,
                    errorMessage: "リクエストがキャンセルされました");
            }
            
            LogHelper.Error(message: $"リクエスト中にエラーが発生しました: {uri}, {ex.Message}");
            return new APIResponse<TResponseData>(
                status: APIStatus.Code.Error,
                data: default,
                errorMessage: $"エラーが発生しました: {ex.Message}");
        }
        
        /// <summary>
        /// レスポンスなしの例外処理共通メソッド
        /// </summary>
        private IAPIResponse HandleExceptionWithoutResponse(Exception ex, string uri)
        {
            if (ex is OperationCanceledException)
            {
                LogHelper.Error(message: $"リクエストがキャンセルされました: {uri}");
                return new APIResponse(
                    status: APIStatus.Code.Error,
                    errorMessage: "リクエストがキャンセルされました");
            }
            
            LogHelper.Error(message: $"リクエスト中にエラーが発生しました: {uri}, {ex.Message}");
            return new APIResponse(
                status: APIStatus.Code.Error,
                errorMessage: $"エラーが発生しました: {ex.Message}");
        }
    }
}