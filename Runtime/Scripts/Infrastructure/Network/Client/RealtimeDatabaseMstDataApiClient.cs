using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Firebase.Database;
using JABARACdesign.Base.Application.Interface;
using JABARACdesign.Base.Domain.Entity.API;
using JABARACdesign.Base.Infrastructure.Helper;
using JABARACdesign.Base.Infrastructure.Network.API;
using JABARACdesign.Base.Infrastructure.Network.Client;
using JABARACdesign.Base.Infrastructure.PathProvider;
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
        
        private readonly IMstDataPathProvider _pathProvider;
        
        private readonly IRealtimeDatabaseInitializer _initializer;
        
        [Inject]
        public RealtimeDatabaseMstDataApiClient(
            IMstDataPathProvider pathProvider,
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
        public async UniTask<IAPIResponse<List<TDto>>> GetMstDataAsync<TDto>()
        {
            var targetReference = GetDatabaseReference<TDto>();
            return await GetDataListAsync<TDto>(reference: targetReference);
        }
        
        /// <summary>
        /// 指定したクラスの特定のIDのデータを取得する (文字列IDの場合)。
        /// </summary>
        /// <typeparam name="TDto">指定クラス</typeparam>
        /// <param name="id">指定ID</param>
        /// <returns>指定IDのデータ</returns>
        /// <exception cref="Exception">データ取得に失敗した際のエラー</exception>
        public async UniTask<IAPIResponse<TDto>> GetMstDataByIdAsync<TDto>(string id)
        {
            var targetReference = GetDatabaseReference<TDto>().Child(pathString: id);
            return await GetDataAsync<TDto>(reference: targetReference);
        }
        
        /// <summary>
        /// 指定したクラスの特定のIDのデータを取得する (整数IDの場合)。
        /// </summary>
        /// <typeparam name="TDto">指定クラス</typeparam>
        /// <param name="id">指定ID</param>
        /// <returns>指定IDのデータ</returns>
        /// <exception cref="Exception">データ取得に失敗した際のエラー</exception>
        public async UniTask<IAPIResponse<TDto>> GetMstDataByIdAsync<TDto>(int id)
        {
            var targetReference = GetDatabaseReference<TDto>().Child(pathString: id.ToString());
            return await GetDataAsync<TDto>(reference: targetReference);
        }
        
        /// <summary>
        /// データベース参照を取得する
        /// </summary>
        /// <typeparam name="TDto">データの型</typeparam>
        /// <returns>データベース参照</returns>
        private DatabaseReference GetDatabaseReference<TDto>()
        {
            var reference = _pathProvider.GetPath<TDto>(rootReference: Database.RootReference);
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
                    data: default,
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