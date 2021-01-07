using Core;
using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Platform;
using Epic.OnlineServices.UserInfo;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OmniAntiCheat.EpicSDK {
	public class EpicClientWrapper {

		private const int POLLING_INTERVAL_MS = 100;
		private const string PRODUCT_ID = "9594306847064560bf3d3599c78de2e2";
		private const string SANDBOX_ID = "7eb5ff9678174213af523eba9f2d1875";
		private const string DEPLOYMENT_ID = "c73009b137e94bb1a1b4a28a4ba1b2c2";
		private const string CLIENT_ID = "xyza7891ckVbTimGreIChRRCQg9fVWYX";
		private const string CLIENT_SECRET = "";

		private PlatformInterface _platform { get; }
		public bool HasStopped { get; private set; }

		public EpicClientWrapper() {
			InitializeOptions initializeOptions = new InitializeOptions();
			initializeOptions.ProductName = "Omni Anti-Cheat";
			initializeOptions.ProductVersion = "1.0";
			var result = PlatformInterface.Initialize(initializeOptions);
			ClientCredentials clientCredentials = new ClientCredentials();
			clientCredentials.ClientId = CLIENT_ID;
			clientCredentials.ClientSecret = CLIENT_SECRET;
			Options options = new Options();
			options.ProductId = PRODUCT_ID;
			options.SandboxId = SANDBOX_ID;
			options.ClientCredentials = clientCredentials;
			options.IsServer = false;
			options.DeploymentId = DEPLOYMENT_ID;
			_platform = PlatformInterface.Create(options);
			if(_platform == null) {
				throw new ApplicationException("Unable to create Epic Client.");
			}
			TaskUtils.FireAndForget(StartSDKPolling());
		}

		public async Task<LoginCallbackInfo> Login(LoginCredentialType loginType) {
			Credentials credentials = new Credentials();
			credentials.Type = loginType;
			credentials.Id = null;
			credentials.Token = null;
			LoginOptions loginOptions = new LoginOptions();
			loginOptions.Credentials = credentials;
			loginOptions.ScopeFlags = AuthScopeFlags.BasicProfile | AuthScopeFlags.FriendsList | AuthScopeFlags.Presence;
			LoginCallbackInfo retVal = null;
			SemaphoreSlim slim = new SemaphoreSlim(1);
			slim.Wait();
			_platform.GetAuthInterface().Login(loginOptions, null, loginCallbackInfo => {
				retVal = loginCallbackInfo;
				if(loginCallbackInfo.ResultCode == Result.Success) {
					slim.Release();
				}
				else if(Helper.IsOperationComplete(loginCallbackInfo.ResultCode)) {
					if(loginCallbackInfo.ResultCode == Result.AuthExpired ||
						loginCallbackInfo.ResultCode == Result.InvalidAuth) 
					{
						var deletePersistentAuthOptions = new DeletePersistentAuthOptions();
						_platform.GetAuthInterface().DeletePersistentAuth(deletePersistentAuthOptions, null, (DeletePersistentAuthCallbackInfo deletePersistentAuthCallbackInfo) => { });
					}
					slim.Release();
				}
			});
			await slim.WaitAsync();
			return retVal;
		}

		///<summary>Returns the username of the given account if possible. Returns null if none.</summary>
		public string GetUsername(EpicAccountId epicAccount) {
			CopyUserInfoOptions options = new CopyUserInfoOptions();
			options.LocalUserId = epicAccount;
			options.TargetUserId = epicAccount;
			var result = _platform.GetUserInfoInterface().CopyUserInfo(options, out UserInfoData outUserInfo);
			if(result == Result.Success) {
				return outUserInfo.DisplayName;
			}
			else {
				return "";
			}
		}

		private void StopSDK() {
			HasStopped = true;
			_platform.Release();
			PlatformInterface.Shutdown();
		}

		private async Task StartSDKPolling() {
			while(!HasStopped) {
				_platform.Tick();
				await Task.Delay(POLLING_INTERVAL_MS);
			}
		}

	}
}
