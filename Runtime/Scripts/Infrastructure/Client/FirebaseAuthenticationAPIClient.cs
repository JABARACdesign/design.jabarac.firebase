using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using JABARACdesign.Base.Application.Interface;
using JABARACdesign.Base.Application.Result.EmailExists;
using JABARACdesign.Base.Domain.Definition;
using JABARACdesign.Base.Infrastructure.API;
using JABARACdesign.Base.Infrastructure.API.CreateAnonymousUser;
using JABARACdesign.Base.Infrastructure.API.CreateUserWithEmailAndPassword;
using JABARACdesign.Base.Infrastructure.API.EmailExists;
using JABARACdesign.Base.Infrastructure.API.GetIsLoggedIn;
using JABARACdesign.Base.Infrastructure.API.LogInWithEmailAndPassword;
using JABARACdesign.Base.Infrastructure.API.UpgradeAnonymousAccount;
using JABARACdesign.Base.Infrastructure.Client;
using JABARACdesign.Firebase.Infrastructure.Network.Initializer;
using VContainer;

namespace JABARACdesign.Firebase.Infrastructure.Client
{
    /// <summary>
    /// Firebaseの認証クライアント
    /// </summary>
    public class FirebaseAuthenticationAPIClient : IAuthenticationClient
    {
        private readonly IAuthenticationInitializer _initializer;
        
        private FirebaseAuth Auth => _initializer.Auth;
        
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="initializer">イニシャライザ</param>
        [Inject]
        public FirebaseAuthenticationAPIClient(IAuthenticationInitializer initializer)
        {
            _initializer = initializer;
        }
        
        /// <summary>
        /// メールアドレスが存在するかどうかを取得する
        /// </summary>
        /// <param name="email">メールアドレス</param>
        /// <param name="cancellationToken">キャンセルトークン</param>
        /// <returns>レスポンス</returns>
        public async UniTask<IAPIResponse<EmailExistsResult>> GetIsEmailExistsAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var providers = await Auth.FetchProvidersForEmailAsync(email: email);
                
                // providersが空でない場合は、アカウントが存在する
                var enumerable = providers.ToList();
                var isExists = enumerable.Any();
                
                var entity = new EmailExistsResponseDTO(isExists: isExists)
                    .ToResult();
                
                return new APIResponse<EmailExistsResult>(
                    status: APIDefinition.Code.Success,
                    data: entity,
                    errorMessage: null);
            }
            catch (OperationCanceledException)
            {
                // キャンセルされた場合
                return new APIResponse<EmailExistsResult>(
                    status: APIDefinition.Code.Error,
                    data: null,
                    errorMessage: "キャンセルされました。");
            }
            catch (Exception)
            {
                // エラーが発生した場合
                return new APIResponse<EmailExistsResult>(
                    status: APIDefinition.Code.Error,
                    data: null,
                    errorMessage: "メールアドレスの存在チェックに失敗しました。");
            }
        }
        
        /// <summary>
        /// 匿名でユーザー登録を行う。
        /// </summary>
        /// <param name="cancellationToken">キャンセルトークン</param>
        /// <returns>レスポンス</returns>
        public async UniTask<IAPIResponse<CreateAnonymousUserResponseDTO>>
            CreateAnonymousUserAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var userCredential = await Auth.SignInAnonymouslyAsync()
                    .AsUniTask()
                    .AttachExternalCancellation(cancellationToken);
                
                var user = userCredential.User;
                
                var dto = new CreateAnonymousUserResponseDTO(
                    userId: user.UserId,
                    user.DisplayName ?? string.Empty);
                
                return new APIResponse<CreateAnonymousUserResponseDTO>(
                    status: APIDefinition.Code.Success,
                    data: dto,
                    errorMessage: null);
            }
            catch (OperationCanceledException)
            {
                // キャンセルされた場合
                return new APIResponse<CreateAnonymousUserResponseDTO>(
                    status: APIDefinition.Code.Error,
                    data: null,
                    errorMessage: "匿名ユーザー登録がキャンセルされました。");
            }
            catch (Exception e)
            {
                // エラーが発生した場合
                return new APIResponse<CreateAnonymousUserResponseDTO>(
                    status: APIDefinition.Code.Error,
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
        public async UniTask<IAPIResponse<CreateUserWithEmailAndPasswordResponseDTO>>
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
                
                var dto = new CreateUserWithEmailAndPasswordResponseDTO(
                    userId: user.UserId,
                    email: user.Email,
                    displayName: user.DisplayName);
                
                return new APIResponse<CreateUserWithEmailAndPasswordResponseDTO>(
                    status: APIDefinition.Code.Success,
                    data: dto,
                    errorMessage: null);
            }
            catch (OperationCanceledException)
            {
                // キャンセルされた場合
                return new APIResponse<CreateUserWithEmailAndPasswordResponseDTO>(
                    status: APIDefinition.Code.Error,
                    data: null,
                    errorMessage: "ユーザー登録がキャンセルされました。");
            }
            catch (Exception e)
            {
                // エラーが発生した場合
                return new APIResponse<CreateUserWithEmailAndPasswordResponseDTO>(
                    status: APIDefinition.Code.Error,
                    data: null,
                    errorMessage: $"ユーザー登録に失敗しました。:{e.Message}");
            }
        } 
        
        /// <summary>
        /// 匿名アカウントをメールアドレス認証にアップグレードする
        /// </summary>
        /// <param name="email">メールアドレス</param>
        /// <param name="password">パスワード</param>
        /// <param name="displayName">表示名</param>
        /// <param name="cancellationToken">キャンセルトークン</param>
        /// <returns>APIレスポンス</returns>
        public async UniTask<IAPIResponse<UpgradeAnonymousAccountResponseDTO>> UpgradeAnonymousAccountAsync(
            string email,
            string password,
            string displayName,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // 現在のユーザーが匿名かチェック
                var user = Auth.CurrentUser;
                if (user == null)
                {
                    return new APIResponse<UpgradeAnonymousAccountResponseDTO>(
                        status: APIDefinition.Code.Error,
                        data: null,
                        errorMessage: "ログインしていません。");
                }

                if (!user.IsAnonymous)
                {
                    return new APIResponse<UpgradeAnonymousAccountResponseDTO>(
                        status: APIDefinition.Code.Error,
                        data: null,
                        errorMessage: "匿名ユーザーではありません。");
                }

                // EmailCredentialの作成
                var credential = EmailAuthProvider.GetCredential(email, password);

                // 匿名アカウントと認証情報をリンク
                var result = await user.LinkWithCredentialAsync(credential)
                    .AsUniTask()
                    .AttachExternalCancellation(cancellationToken);

                // プロフィール更新
                if (!string.IsNullOrEmpty(displayName))
                {
                    var userProfile = new UserProfile
                    {
                        DisplayName = displayName
                    };
                    await user.UpdateUserProfileAsync(profile: userProfile);
                }

                // アップグレード成功
                var dto = new UpgradeAnonymousAccountResponseDTO(
                    userId: result.User.UserId,
                    email: result.User.Email,
                    displayName: result.User.DisplayName ?? displayName);

                return new APIResponse<UpgradeAnonymousAccountResponseDTO>(
                    status: APIDefinition.Code.Success,
                    data: dto,
                    errorMessage: null);
            }
            catch (OperationCanceledException)
            {
                return new APIResponse<UpgradeAnonymousAccountResponseDTO>(
                    status: APIDefinition.Code.Error,
                    data: null,
                    errorMessage: "アカウントアップグレードがキャンセルされました。");
            }
            catch (FirebaseException ex)
            {
                return new APIResponse<UpgradeAnonymousAccountResponseDTO>(
                    status: APIDefinition.Code.Error,
                    data: null,
                    errorMessage: $"アカウントアップグレードに失敗しました：{ex.Message}");
            }
            catch (Exception e)
            {
                return new APIResponse<UpgradeAnonymousAccountResponseDTO>(
                    status: APIDefinition.Code.Error,
                    data: null,
                    errorMessage: $"アカウントアップグレードに失敗しました：{e.Message}");
            }
        }
        
        /// <summary>
        /// ログイン状態かどうかを判定する
        /// </summary>
        /// <returns>ログイン状態の場合はtrue、それ以外の場合はfalse</returns>
        public IAPIResponse<GetIsLoggedInDTO> GetIsLoggedIn()
        {
            try
            {
                var user = Auth.CurrentUser;
                var isLoggedIn = user != null;
                
                if (!isLoggedIn)
                    return new APIResponse<GetIsLoggedInDTO>(
                        status: APIDefinition.Code.Success,
                        data: new GetIsLoggedInDTO(
                            isLoggedIn: false,
                            userId: null,
                            displayName: null),
                        errorMessage: null);
                
                return new APIResponse<GetIsLoggedInDTO>(
                    status: APIDefinition.Code.Success,
                    data: new GetIsLoggedInDTO(
                        isLoggedIn: true,
                        userId: user.UserId,
                        displayName: user.DisplayName),
                    errorMessage: null);
            }
            catch (Exception e)
            {
                // エラーが発生した場合
                return new APIResponse<GetIsLoggedInDTO>(
                    status: APIDefinition.Code.Error,
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
        public async UniTask<IAPIResponse<LogInWithEmailAndPasswordResponseDTO>> SignInWithEmailAndPasswordAsync(
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
                
                var dto = new LogInWithEmailAndPasswordResponseDTO(
                    userId: user.UserId,
                    email: user.Email,
                    displayName: user.DisplayName);
                
                return new APIResponse<LogInWithEmailAndPasswordResponseDTO>(
                    status: APIDefinition.Code.Success,
                    data: dto,
                    errorMessage: null);
            }
            catch (OperationCanceledException)
            {
                // キャンセルされた場合
                return new APIResponse<LogInWithEmailAndPasswordResponseDTO>(
                    status: APIDefinition.Code.Error,
                    data: null,
                    errorMessage: "ログインがキャンセルされました。");
            }
            catch (Exception e)
            {
                // エラーが発生した場合
                return new APIResponse<LogInWithEmailAndPasswordResponseDTO>(
                    status: APIDefinition.Code.Error,
                    data: null,
                    errorMessage: $"ログインに失敗しました。:{e.Message}");
            }
        }
    }
}