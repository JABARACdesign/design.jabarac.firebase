using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Firebase.Database;
using JABARACdesign.Base.Application.Interface;
using JABARACdesign.Base.Domain.Entity.API;
using JABARACdesign.Base.Domain.Helper;
using JABARACdesign.Base.Infrastructure.Network;
using JABARACdesign.Base.Infrastructure.Network.API;
using JABARACdesign.Base.Infrastructure.Network.Client;
using JABARACdesign.Firebase.Infrastructure.Network.Initializer;
using UnityEngine;
using VContainer;

namespace JABARACdesign.Firebase.Infrastructure.Network.Client
{
    /// <summary>
    /// FirebaseのRealtimeDatabaseを用いたマスターデータに関するクライアントクラス。
    /// </summary>
    public class RealtimeDatabaseMstDataApiClient : IMstDataApiClient
    {
        private FirebaseDatabase Database => _initializer.Database;
        
        private readonly IPathProvider _pathProvider;
        
        private readonly IRealtimeDatabaseInitializer _initializer;
        
        /// <summary>
        /// コンストラクタ(DI)。
        /// </summary>
        /// <param name="pathProvider">パスプロパイダ</param>
        /// <param name="initializer"></param>
        [Inject]
        public RealtimeDatabaseMstDataApiClient(
            IPathProvider pathProvider,
            IRealtimeDatabaseInitializer initializer)
        {
            _pathProvider = pathProvider;
            _initializer = initializer;
        }
        
        /// <summary>
        /// 指定したクラスに対応するマスターデータをリストですべて取得する。
        /// </summary>
        /// <typeparam name="TDto">マスターデータのアイテムの型</typeparam>
        /// <returns>マスターデータのリスト</returns>
        /// <exception cref="Exception">データ取得に失敗した際のエラー</exception>
        public async UniTask<IAPIResponse<List<TDto>>> GetMstDataAsync<TDto, TEnum>(TEnum identifier)
        where TEnum : struct, Enum
        {
            var targetReference = GetDatabaseReference(identifier);
            return await GetDataListAsync<TDto>(reference: targetReference);
        }

        /// <summary>
        /// 指定したクラスの特定のIDのデータを取得する (文字列IDの場合)。
        /// </summary>
        /// <typeparam name="TDto">指定クラス</typeparam>
        /// <typeparam name="TEnum">データの型</typeparam>
        ///　/// <param name="identifier">識別子</param>
        /// <param name="id">指定ID</param>
        /// <returns>指定IDのデータ</returns>
        /// <exception cref="Exception">データ取得に失敗した際のエラー</exception>
        public async UniTask<IAPIResponse<TDto>> GetMstDataByIdAsync<TDto,TEnum>(TEnum identifier, string id)
        where TEnum : struct, Enum
        {
            var targetReference = GetDatabaseReference(identifier)
                .Child(pathString: id);
            
            return await GetDataAsync<TDto>(reference: targetReference);
        }

        /// <summary>
        /// 指定したクラスの特定のIDのデータを取得する (整数IDの場合)。
        /// </summary>
        /// <typeparam name="TDto">指定クラス</typeparam>
        /// <typeparam name="TEnum">データの型</typeparam>
        /// <param name="identifier">識別子</param>
        /// <param name="id">指定ID</param>
        /// <returns>指定IDのデータ</returns>
        /// <exception cref="Exception">データ取得に失敗した際のエラー</exception>
        public async UniTask<IAPIResponse<TDto>> GetMstDataByIdAsync<TDto, TEnum>(
            TEnum identifier,
            int id)
        where TEnum : struct, Enum
        {
            var targetReference = GetDatabaseReference(identifier)
                .Child(pathString: id.ToString());
            
            return await GetDataAsync<TDto>(reference: targetReference);
        }
        
        /// <summary>
        /// データベース参照を取得する
        /// </summary>
        /// <typeparam name="TEnum">データの型</typeparam>
        /// <returns>データベース参照</returns>
        private DatabaseReference GetDatabaseReference<TEnum>(TEnum identifier)
        where TEnum : struct, Enum
        {
            var path = _pathProvider.GetFilePath(identifier);
            var reference = Database.GetReference(path: path);
            return reference;
        }
        
        /// <summary>
        /// データリストを取得する
        /// </summary>
        /// <param name="reference">データベース参照</param>
        /// <typeparam name="TDto">リストのアイテムの型</typeparam>
        /// <returns>データリスト</returns>
        private async UniTask<IAPIResponse<List<TDto>>> GetDataListAsync<TDto>(
            DatabaseReference reference)
        {
            try
            {
                var snapshot = await GetDataSnapshotAsync(reference: reference);
                var json = snapshot.GetRawJsonValue();
                var data = JsonHelper.FromJsonList<TDto>(json: json);
                
                return new APIResponse<List<TDto>>(
                    status: APIStatus.Code.Success,
                    data: data,
                    errorMessage: null
                );
            }
            catch (Exception e)
            {
                return new APIResponse<List<TDto>>(
                    status: APIStatus.Code.Error,
                    data: null,
                    errorMessage: $"データの取得に失敗しました。:{e.Message}"
                );
            }
        }
        
        /// <summary>
        /// データを取得する
        /// </summary>
        /// <param name="reference">データベース参照</param>
        /// <typeparam name="TDto">データの型</typeparam>
        /// <returns>データ</returns>
        private async UniTask<IAPIResponse<TDto>> GetDataAsync<TDto>(DatabaseReference reference)
        {
            try
            {
                var snapshot = await GetDataSnapshotAsync(reference: reference);
                var json = snapshot.GetRawJsonValue();
                var dto = JsonUtility.FromJson<TDto>(json: json);
                
                return new APIResponse<TDto>(
                    status: APIStatus.Code.Success,
                    data: dto,
                    errorMessage: null
                );
            }
            catch (Exception e)
            {
                return new APIResponse<TDto>(
                    status: APIStatus.Code.Error,
                    data: default,
                    errorMessage: $"データの取得に失敗しました。:{e.Message}"
                );
            }
        }
        
        /// <summary>
        /// データスナップショットを取得する
        /// </summary>
        /// <param name="reference">データベース参照</param>
        /// <returns>データスナップショット</returns>
        /// <exception cref="Exception">データの取得に失敗した際のエラー</exception>
        private static async UniTask<DataSnapshot> GetDataSnapshotAsync(DatabaseReference reference)
        {
            try
            {
                return await reference.GetValueAsync().AsUniTask();
            }
            catch
            {
                throw new Exception(message: "データの取得に失敗しました。");
            }
        }
    }
}