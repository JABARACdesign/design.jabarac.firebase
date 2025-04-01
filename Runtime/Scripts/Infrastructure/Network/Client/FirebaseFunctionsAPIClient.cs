using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Firebase.Functions;
using JABARACdesign.Base.Application.Interface;
using JABARACdesign.Base.Domain.Definition;
using JABARACdesign.Base.Domain.Entity.API;
using JABARACdesign.Base.Domain.Entity.Helper;
using JABARACdesign.Base.Domain.Interface;
using JABARACdesign.Base.Infrastructure.Network.API;
using JABARACdesign.Base.Infrastructure.Network.Client;
using JABARACdesign.Firebase.Infrastructure.Network.Initializer;
using Unity.Plastic.Newtonsoft.Json;
using VContainer;

namespace JABARACdesign.Firebase.Infrastructure.Network.Client
{
    /// <summary>
    /// Firebase Functions APIクライアントクラス。
    /// </summary>
    public class FirebaseFunctionsApiClient : IFunctionApiClient
    {
        private readonly IFirebaseFunctionsInitializer _initializer;
        
        private FirebaseFunctions Functions => _initializer.Functions;

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="initializer">Functionsのイニシャライザ</param>
        [Inject]
        public FirebaseFunctionsApiClient(
            IFirebaseFunctionsInitializer initializer)
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
                // Cloud Functionの参照を取得（Uriから関数名を抽出）
                var functionName = ExtractFunctionName(request.Uri);
                var callable = Functions.GetHttpsCallable(name: functionName);
                
                // GETリクエストの場合は空のオブジェクトを送信
                var jsonData = request.MethodType == APIDefinition.HttpMethodType.GET ? "{}" : "{}";
                
                // Firebase Functionsを呼び出し
                var result = await callable.CallAsync(data: jsonData)
                    .AsUniTask()
                    .AttachExternalCancellation(cancellationToken);
                
                // レスポンスの処理
                return ProcessResponse<TResponseData>(result, functionName);
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
        {
            try
            {
                // Cloud Functionの参照を取得（Uriから関数名を抽出）
                var functionName = ExtractFunctionName(request.Uri);
                var callable = Functions.GetHttpsCallable(name: functionName);
                
                // DTOをJSONに変換
                var jsonData = JsonConvert.SerializeObject(value: request.Dto);
                
                // Firebase Functionsを呼び出し
                var result = await callable.CallAsync(data: jsonData)
                    .AsUniTask()
                    .AttachExternalCancellation(cancellationToken);
                
                // レスポンスの処理
                return ProcessResponse<TResponseData>(result, functionName);
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
                // Cloud Functionの参照を取得（Uriから関数名を抽出）
                var functionName = ExtractFunctionName(request.Uri);
                var callable = Functions.GetHttpsCallable(name: functionName);
                
                // GETリクエストの場合は空のオブジェクトを送信
                var jsonData = request.MethodType == APIDefinition.HttpMethodType.GET ? "{}" : "{}";
                
                // Firebase Functionsを呼び出し
                await callable.CallAsync(data: jsonData)
                    .AsUniTask()
                    .AttachExternalCancellation(cancellationToken);
                
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
        {
            try
            {
                // Cloud Functionの参照を取得（Uriから関数名を抽出）
                var functionName = ExtractFunctionName(request.Uri);
                var callable = Functions.GetHttpsCallable(name: functionName);
                
                // DTOをJSONに変換
                var jsonData = JsonConvert.SerializeObject(value: request.Dto);
                
                // Firebase Functionsを呼び出し
                await callable.CallAsync(data: jsonData)
                    .AsUniTask()
                    .AttachExternalCancellation(cancellationToken);
                
                return new APIResponse(status: APIStatus.Code.Success);
            }
            catch (Exception ex)
            {
                return HandleExceptionWithoutResponse(ex, request.Uri);
            }
        }
        
        /// <summary>
        /// UriからFunction名を抽出するメソッド
        /// </summary>
        private string ExtractFunctionName(string uri)
        {
            // 単純にUriの最後のセグメントを取得する例
            // 実際の要件に合わせて調整が必要
            var segments = uri.TrimEnd('/').Split('/');
            return segments[segments.Length - 1];
        }
        
        /// <summary>
        /// レスポンス処理の共通メソッド
        /// </summary>
        private IAPIResponse<TResponseData> ProcessResponse<TResponseData>(
            HttpsCallableResult result, 
            string functionName)
        {
            if (result.Data is string resultJson)
            {
                try
                {
                    var responseData = JsonConvert.DeserializeObject<TResponseData>(value: resultJson);
                    return new APIResponse<TResponseData>(
                        status: APIStatus.Code.Success,
                        data: responseData);
                }
                catch (JsonException ex)
                {
                    LogHelper.Error(message: $"Cloud Functionからのレスポンスデシリアライズに失敗: {functionName}, {ex.Message}");
                    return new APIResponse<TResponseData>(
                        status: APIStatus.Code.Error,
                        data: default,
                        errorMessage: "レスポンス形式が不正です");
                }
            }
            
            LogHelper.Error(message: $"Cloud Functionからの応答形式が不正: {functionName}");
            return new APIResponse<TResponseData>(
                status: APIStatus.Code.Error,
                data: default,
                errorMessage: "応答形式が不正です");
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
                LogHelper.Error(message: $"Cloud Function実行がキャンセルされました: {uri}");
                return new APIResponse<TResponseData>(
                    status: APIStatus.Code.Error,
                    data: default,
                    errorMessage: "リクエストがキャンセルされました");
            }
            
            LogHelper.Error(message: $"Cloud Function実行中にエラーが発生しました: {uri}, {ex.Message}");
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
                LogHelper.Error(message: $"Cloud Function実行がキャンセルされました: {uri}");
                return new APIResponse(
                    status: APIStatus.Code.Error,
                    errorMessage: "リクエストがキャンセルされました");
            }
            
            LogHelper.Error(message: $"Cloud Function実行中にエラーが発生しました: {uri}, {ex.Message}");
            return new APIResponse(
                status: APIStatus.Code.Error,
                errorMessage: $"エラーが発生しました: {ex.Message}");
        }
    }
}