// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.Sessions
{
	public sealed class SessionSearch : Handle
	{
		public SessionSearch()
		{
		}

		public SessionSearch(System.IntPtr innerHandle) : base(innerHandle)
		{
		}

		/// <summary>
		/// The most recent version of the <see cref="CopySearchResultByIndex" /> API.
		/// </summary>
		public const int SessionsearchCopysearchresultbyindexApiLatest = 1;

		/// <summary>
		/// The most recent version of the <see cref="Find" /> API.
		/// </summary>
		public const int SessionsearchFindApiLatest = 2;

		/// <summary>
		/// The most recent version of the <see cref="GetSearchResultCount" /> API.
		/// </summary>
		public const int SessionsearchGetsearchresultcountApiLatest = 1;

		/// <summary>
		/// The most recent version of the <see cref="RemoveParameter" /> API.
		/// </summary>
		public const int SessionsearchRemoveparameterApiLatest = 1;

		/// <summary>
		/// The most recent version of the <see cref="SetMaxResults" /> API.
		/// </summary>
		public const int SessionsearchSetmaxsearchresultsApiLatest = 1;

		/// <summary>
		/// The most recent version of the <see cref="SetParameter" /> API.
		/// </summary>
		public const int SessionsearchSetparameterApiLatest = 1;

		/// <summary>
		/// The most recent version of the <see cref="SetSessionId" /> API.
		/// </summary>
		public const int SessionsearchSetsessionidApiLatest = 1;

		/// <summary>
		/// The most recent version of the <see cref="SetTargetUserId" /> API.
		/// </summary>
		public const int SessionsearchSettargetuseridApiLatest = 1;

		/// <summary>
		/// <see cref="CopySearchResultByIndex" /> is used to immediately retrieve a handle to the session information from a given search result.
		/// If the call returns an <see cref="Result.Success" /> result, the out parameter, OutSessionHandle, must be passed to <see cref="SessionDetails.Release" /> to release the memory associated with it.
		/// <seealso cref="SessionSearchCopySearchResultByIndexOptions" />
		/// <seealso cref="SessionDetails.Release" />
		/// </summary>
		/// <param name="options">Structure containing the input parameters</param>
		/// <param name="outSessionHandle">out parameter used to receive the session handle</param>
		/// <returns>
		/// <see cref="Result.Success" /> if the information is available and passed out in OutSessionHandle
		/// <see cref="Result.InvalidParameters" /> if you pass an invalid index or a null pointer for the out parameter
		/// <see cref="Result.IncompatibleVersion" /> if the API version passed in is incorrect
		/// </returns>
		public Result CopySearchResultByIndex(SessionSearchCopySearchResultByIndexOptions options, out SessionDetails outSessionHandle)
		{
			System.IntPtr optionsAddress = new System.IntPtr();
			Helper.TryMarshalSet<SessionSearchCopySearchResultByIndexOptionsInternal, SessionSearchCopySearchResultByIndexOptions>(ref optionsAddress, options);

			var outSessionHandleAddress = System.IntPtr.Zero;

			var funcResult = EOS_SessionSearch_CopySearchResultByIndex(InnerHandle, optionsAddress, ref outSessionHandleAddress);

			Helper.TryMarshalDispose(ref optionsAddress);

			Helper.TryMarshalGet(outSessionHandleAddress, out outSessionHandle);

			return funcResult;
		}

		/// <summary>
		/// Find sessions matching the search criteria setup via this session search handle.
		/// When the operation completes, this handle will have the search results that can be parsed
		/// </summary>
		/// <param name="options">Structure containing information about the search criteria to use</param>
		/// <param name="clientData">Arbitrary data that is passed back to you in the CompletionDelegate</param>
		/// <param name="completionDelegate">A callback that is fired when the search operation completes, either successfully or in error</param>
		/// <returns>
		/// <see cref="Result.Success" /> if the find operation completes successfully
		/// <see cref="Result.NotFound" /> if searching for an individual session by sessionid or targetuserid returns no results
		/// <see cref="Result.InvalidParameters" /> if any of the options are incorrect
		/// </returns>
		public void Find(SessionSearchFindOptions options, object clientData, SessionSearchOnFindCallback completionDelegate)
		{
			System.IntPtr optionsAddress = new System.IntPtr();
			Helper.TryMarshalSet<SessionSearchFindOptionsInternal, SessionSearchFindOptions>(ref optionsAddress, options);

			var clientDataAddress = System.IntPtr.Zero;

			var completionDelegateInternal = new SessionSearchOnFindCallbackInternal(OnFindCallbackInternalImplementation);
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, completionDelegateInternal);

			EOS_SessionSearch_Find(InnerHandle, optionsAddress, clientDataAddress, completionDelegateInternal);

			Helper.TryMarshalDispose(ref optionsAddress);
		}

		/// <summary>
		/// Get the number of search results found by the search parameters in this search
		/// </summary>
		/// <param name="options">Options associated with the search count</param>
		/// <returns>
		/// return the number of search results found by the query or 0 if search is not complete
		/// </returns>
		public uint GetSearchResultCount(SessionSearchGetSearchResultCountOptions options)
		{
			System.IntPtr optionsAddress = new System.IntPtr();
			Helper.TryMarshalSet<SessionSearchGetSearchResultCountOptionsInternal, SessionSearchGetSearchResultCountOptions>(ref optionsAddress, options);

			var funcResult = EOS_SessionSearch_GetSearchResultCount(InnerHandle, optionsAddress);

			Helper.TryMarshalDispose(ref optionsAddress);

			return funcResult;
		}

		/// <summary>
		/// Release the memory associated with a session search. This must be called on data retrieved from <see cref="SessionsInterface.CreateSessionSearch" />.
		/// <seealso cref="SessionsInterface.CreateSessionSearch" />
		/// </summary>
		/// <param name="sessionSearchHandle">- The session search handle to release</param>
		public void Release()
		{
			EOS_SessionSearch_Release(InnerHandle);
		}

		/// <summary>
		/// Remove a parameter from the array of search criteria.
		/// 
		/// @params Options a search parameter key name to remove
		/// </summary>
		/// <returns>
		/// <see cref="Result.Success" /> if removing this search parameter was successful
		/// <see cref="Result.InvalidParameters" /> if the search key is invalid or null
		/// <see cref="Result.NotFound" /> if the parameter was not a part of the search criteria
		/// <see cref="Result.IncompatibleVersion" /> if the API version passed in is incorrect
		/// </returns>
		public Result RemoveParameter(SessionSearchRemoveParameterOptions options)
		{
			System.IntPtr optionsAddress = new System.IntPtr();
			Helper.TryMarshalSet<SessionSearchRemoveParameterOptionsInternal, SessionSearchRemoveParameterOptions>(ref optionsAddress, options);

			var funcResult = EOS_SessionSearch_RemoveParameter(InnerHandle, optionsAddress);

			Helper.TryMarshalDispose(ref optionsAddress);

			return funcResult;
		}

		/// <summary>
		/// Set the maximum number of search results to return in the query, can't be more than <see cref="SessionsInterface.MaxSearchResults" />
		/// </summary>
		/// <param name="options">maximum number of search results to return in the query</param>
		/// <returns>
		/// <see cref="Result.Success" /> if setting the max results was successful
		/// <see cref="Result.InvalidParameters" /> if the number of results requested is invalid
		/// <see cref="Result.IncompatibleVersion" /> if the API version passed in is incorrect
		/// </returns>
		public Result SetMaxResults(SessionSearchSetMaxResultsOptions options)
		{
			System.IntPtr optionsAddress = new System.IntPtr();
			Helper.TryMarshalSet<SessionSearchSetMaxResultsOptionsInternal, SessionSearchSetMaxResultsOptions>(ref optionsAddress, options);

			var funcResult = EOS_SessionSearch_SetMaxResults(InnerHandle, optionsAddress);

			Helper.TryMarshalDispose(ref optionsAddress);

			return funcResult;
		}

		/// <summary>
		/// Add a parameter to an array of search criteria combined via an implicit AND operator. Setting SessionId or TargetUserId will result in <see cref="Find" /> failing
		/// <seealso cref="AttributeData" />
		/// <seealso cref="ComparisonOp" />
		/// </summary>
		/// <param name="options">a search parameter and its comparison op</param>
		/// <returns>
		/// <see cref="Result.Success" /> if setting this search parameter was successful
		/// <see cref="Result.InvalidParameters" /> if the search criteria is invalid or null
		/// <see cref="Result.IncompatibleVersion" /> if the API version passed in is incorrect
		/// </returns>
		public Result SetParameter(SessionSearchSetParameterOptions options)
		{
			System.IntPtr optionsAddress = new System.IntPtr();
			Helper.TryMarshalSet<SessionSearchSetParameterOptionsInternal, SessionSearchSetParameterOptions>(ref optionsAddress, options);

			var funcResult = EOS_SessionSearch_SetParameter(InnerHandle, optionsAddress);

			Helper.TryMarshalDispose(ref optionsAddress);

			return funcResult;
		}

		/// <summary>
		/// Set a session ID to find and will return at most one search result. Setting TargetUserId or SearchParameters will result in <see cref="Find" /> failing
		/// </summary>
		/// <param name="options">A specific session ID for which to search</param>
		/// <returns>
		/// <see cref="Result.Success" /> if setting this session ID was successful
		/// <see cref="Result.InvalidParameters" /> if the session ID is invalid or null
		/// <see cref="Result.IncompatibleVersion" /> if the API version passed in is incorrect
		/// </returns>
		public Result SetSessionId(SessionSearchSetSessionIdOptions options)
		{
			System.IntPtr optionsAddress = new System.IntPtr();
			Helper.TryMarshalSet<SessionSearchSetSessionIdOptionsInternal, SessionSearchSetSessionIdOptions>(ref optionsAddress, options);

			var funcResult = EOS_SessionSearch_SetSessionId(InnerHandle, optionsAddress);

			Helper.TryMarshalDispose(ref optionsAddress);

			return funcResult;
		}

		/// <summary>
		/// Set a target user ID to find and will return at most one search result. Setting SessionId or SearchParameters will result in <see cref="Find" /> failing
		/// @note a search result will only be found if this user is in a public session
		/// </summary>
		/// <param name="options">a specific target user ID to find</param>
		/// <returns>
		/// <see cref="Result.Success" /> if setting this target user ID was successful
		/// <see cref="Result.InvalidParameters" /> if the target user ID is invalid or null
		/// <see cref="Result.IncompatibleVersion" /> if the API version passed in is incorrect
		/// </returns>
		public Result SetTargetUserId(SessionSearchSetTargetUserIdOptions options)
		{
			System.IntPtr optionsAddress = new System.IntPtr();
			Helper.TryMarshalSet<SessionSearchSetTargetUserIdOptionsInternal, SessionSearchSetTargetUserIdOptions>(ref optionsAddress, options);

			var funcResult = EOS_SessionSearch_SetTargetUserId(InnerHandle, optionsAddress);

			Helper.TryMarshalDispose(ref optionsAddress);

			return funcResult;
		}

		[MonoPInvokeCallback(typeof(SessionSearchOnFindCallbackInternal))]
		internal static void OnFindCallbackInternalImplementation(System.IntPtr data)
		{
			SessionSearchOnFindCallback callback;
			SessionSearchFindCallbackInfo callbackInfo;
			if (Helper.TryGetAndRemoveCallback<SessionSearchOnFindCallback, SessionSearchFindCallbackInfoInternal, SessionSearchFindCallbackInfo>(data, out callback, out callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[System.Runtime.InteropServices.DllImport(Config.BinaryName)]
		internal static extern Result EOS_SessionSearch_CopySearchResultByIndex(System.IntPtr handle, System.IntPtr options, ref System.IntPtr outSessionHandle);

		[System.Runtime.InteropServices.DllImport(Config.BinaryName)]
		internal static extern void EOS_SessionSearch_Find(System.IntPtr handle, System.IntPtr options, System.IntPtr clientData, SessionSearchOnFindCallbackInternal completionDelegate);

		[System.Runtime.InteropServices.DllImport(Config.BinaryName)]
		internal static extern uint EOS_SessionSearch_GetSearchResultCount(System.IntPtr handle, System.IntPtr options);

		[System.Runtime.InteropServices.DllImport(Config.BinaryName)]
		internal static extern void EOS_SessionSearch_Release(System.IntPtr sessionSearchHandle);

		[System.Runtime.InteropServices.DllImport(Config.BinaryName)]
		internal static extern Result EOS_SessionSearch_RemoveParameter(System.IntPtr handle, System.IntPtr options);

		[System.Runtime.InteropServices.DllImport(Config.BinaryName)]
		internal static extern Result EOS_SessionSearch_SetMaxResults(System.IntPtr handle, System.IntPtr options);

		[System.Runtime.InteropServices.DllImport(Config.BinaryName)]
		internal static extern Result EOS_SessionSearch_SetParameter(System.IntPtr handle, System.IntPtr options);

		[System.Runtime.InteropServices.DllImport(Config.BinaryName)]
		internal static extern Result EOS_SessionSearch_SetSessionId(System.IntPtr handle, System.IntPtr options);

		[System.Runtime.InteropServices.DllImport(Config.BinaryName)]
		internal static extern Result EOS_SessionSearch_SetTargetUserId(System.IntPtr handle, System.IntPtr options);
	}
}