using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Firebase.Auth;
using JABARACdesign.Base.Application.Interface;
using JABARACdesign.Base.Domain.Entity.API;
using JABARACdesign.Base.Infrastructure.Dto.API;
using JABARACdesign.Base.Infrastructure.Network.API;
using JABARACdesign.Base.Infrastructure.Network.Client;
using JABARACdesign.Firebase.Infrastructure.Network.Initializer;
using VContainer;

namespace JABARACdesign.Firebase.Infrastructure.Network.Client
{
    /// <summary>
    /// Firebaseの認証クライアント
    /// </summary>
    public class FirebaseAuthenticationApiClient : IAuthenticationClient
    {
        private readonly IAuthenticationInitializer _initializer;
        
        private FirebaseAuth Auth => _initializer.Auth;
        
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="initializer">イニシャライザ</param>
        [Inject]
        public FirebaseAuthenticationApiClient(IAuthenticationInitializer initializer)
        {
            _initializer = initializer;
        }
        
        /// <summary>
        /// メールアドレスが存在するかどうかを取得する
        /// </summary>
        /// <param name="email">メールアドレス</param>
        /// <param name="cancellationToken">キャンセルトークン</param>
        /// <returns>レスポンス</returns>
        public async UniTask<IAPIResponse<EmailExistsResponse>> GetIsEmailExistsAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var providers = await Auth.FetchProvidersForEmailAsync(email: email);
                
                // providersが空でない場合は、アカウントが存在する
                var enumerable = providers.ToList();
                var isExists = enumerable.Any();
                
                var entity = new EmailExistsResponseDto(isExists: isExists)
                    .ToEntity();
                
                return new APIResponse<EmailExistsResponse>(
                    status: APIStatus.Code.Success,
                    data: entity,
                    errorMessage: null);
            }
            catch (OperationCanceledException)
            {
                // キャンセルされた場合
                return new APIResponse<EmailExistsResponse>(
                    status: APIStatus.Code.Error,
                    data: null,
                    errorMessage: "キャンセルされました。");
            }
            catch (Exception)
            {
                // エラーが発生した場合
                return new APIResponse<EmailExistsResponse>(
                    status: APIStatus.Code.Error,
                    data: null,
                    errorMessage: "メールアドレスの存在チェックに失敗しました。");
            }
        }
        
        /// <summary>
        /// 匿名でユーザー登録を行う。
        /// </summary>
        /// <param name="cancellationToken">キャンセルトークン</param>
        /// <returns>レスポンス</returns>
        public async UniTask<IAPIResponse<CreateAnonymousUserResponseDto>>
            CreateAnonymousUserAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var userCredential = await Auth.SignInAnonymouslyAsync()
                    .AsUniTask()
                    .AttachExternalCancellation(cancellationToken);
                
                var user = userCredential.User;
                
                var dto = new CreateAnonymousUserResponseDto(
                    userId: user.UserId,
                    user.DisplayName ?? string.Empty);
                
                return new APIResponse<CreateAnonymousUserResponseDto>(
                    status: APIStatus.Code.Success,
                    data: dto,
                    errorMessage: null);
            }
            catch (OperationCanceledException)
            {
                // キャンセルされた場合
                return new APIResponse<CreateAnonymousUserResponseDto>(
                    status: APIStatus.Code.Error,
                    data: null,
                    errorMessage: "匿名ユーザー登録がキャンセルされました。");
            }
            catch (Exception e)
            {
                // エラーが発生した場合
                return new APIResponse<CreateAnonymousUserResponseDto>(
                    status: APIStatus.Code.Error,
                    data: null,
                    errorMessage: $"匿名ユーザー登録に失敗しました。:{e.Message}");
            }
        }
        
        /// <summary>
        /// ユーザー登録を行う
        /// </summary>
        /// <param name="email">メールアドレス</param>
        /// <param name="password">パスワード</param>
        /// <param name="displayName">表示名</param>
        /// <param name="cancellationToken">キャンセルトークン</param>
        /// <returns>レスポンス</returns>
        public async UniTask<IAPIResponse<CreateUserWithEmailAndPasswordResponseDto>>
            CreateUserWithEmailAndPasswordAsync(
                string email,
                string password,
                string displayName,
                CancellationToken cancellationToken = default)
        {
            try
            {
                var userCredential = await Auth.CreateUserWithEmailAndPasswordAsync(
                    email: email,
                    password: password)
                    .AsUniTask()
                    .AttachExternalCancellation(cancellationToken);
                
                var user = userCredential.User;
                
                var userProfile = new UserProfile
                {
                    DisplayName = displayName
                };
                
                await user.UpdateUserProfileAsync(profile: userProfile);
                
                var dto = new CreateUserWithEmailAndPasswordResponseDto(
                    userId: user.UserId,
                    email: user.Email,
                    displayName: user.DisplayName);
                
                return new APIResponse<CreateUserWithEmailAndPasswordResponseDto>(
                    status: APIStatus.Code.Success,
                    data: dto,
                    errorMessage: null);
            }
            catch (OperationCanceledException)
            {
                // キャンセルされた場合
                return new APIResponse<CreateUserWithEmailAndPasswordResponseDto>(
                    status: APIStatus.Code.Error,
                    data: null,
                    errorMessage: "ユーザー登録がキャンセルされました。");
            }
            catch (Exception e)
            {
                // エラーが発生した場合
                return new APIResponse<CreateUserWithEmailAndPasswordResponseDto>(
                    status: APIStatus.Code.Error,
                    data: null,
                    errorMessage: $"ユーザー登録に失敗しました。:{e.Message}");
            }
        }
        
        /// <summary>
        /// ログイン状態かどうかを判定する
        /// </summary>
        /// <returns>ログイン状態の場合はtrue、それ以外の場合はfalse</returns>
        public IAPIResponse<GetIsLoggedInDto> GetIsLoggedIn()
        {
            try
            {
                var user = Auth.CurrentUser;
                var isLoggedIn = user != null;
                
                if (!isLoggedIn)
                    return new APIResponse<GetIsLoggedInDto>(
                        status: APIStatus.Code.Success,
                        data: new GetIsLoggedInDto(
                            isLoggedIn: false,
                            userId: null,
                            displayName: null),
                        errorMessage: null);
                
                return new APIResponse<GetIsLoggedInDto>(
                    status: APIStatus.Code.Success,
                    data: new GetIsLoggedInDto(
                        isLoggedIn: true,
                        userId: user.UserId,
                        displayName: user.DisplayName),
                    errorMessage: null);
            }
            catch (Exception e)
            {
                // エラーが発生した場合
                return new APIResponse<GetIsLoggedInDto>(
                    status: APIStatus.Code.Error,
                    data: null,
                    errorMessage: $"ユーザー登録に失敗しました。:{e.Message}");
            }
        }
        
        /// <summary>
        /// メールアドレスとパスワードでログインする
        /// </summary>
        /// <param name="email">メールアドレス</param>
        /// <param name="password">パスワード</param>
        /// <param name="cancellationToken">キャンセルトークン</param>
        /// <returns>レスポンス</returns>
        public async UniTask<IAPIResponse<LogInWithEmailAndPasswordResponseDto>> SignInWithEmailAndPasswordAsync(
            string email,
            string password,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var userCredential = await Auth.SignInWithEmailAndPasswordAsync(
                    email: email,
                    password: password)
                    .AsUniTask()
                    .AttachExternalCancellation(cancellationToken);
                
                var user = userCredential.User;
                
                var dto = new LogInWithEmailAndPasswordResponseDto(
                    userId: user.UserId,
                    email: user.Email,
                    displayName: user.DisplayName);
                
                return new APIResponse<LogInWithEmailAndPasswordResponseDto>(
                    status: APIStatus.Code.Success,
                    data: dto,
                    errorMessage: null);
            }
            catch (OperationCanceledException)
            {
                // キャンセルされた場合
                return new APIResponse<LogInWithEmailAndPasswordResponseDto>(
                    status: APIStatus.Code.Error,
                    data: null,
                    errorMessage: "ログインがキャンセルされました。");
            }
            catch (Exception e)
            {
                // エラーが発生した場合
                return new APIResponse<LogInWithEmailAndPasswordResponseDto>(
                    status: APIStatus.Code.Error,
                    data: null,
                    errorMessage: $"ログインに失敗しました。:{e.Message}");
            }
        }
    }
}