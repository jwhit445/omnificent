﻿using Amazon.S3.Transfer;
using Core;
using Core.Extensions;
using Core.Omni.API;
using Core.Omni.API.Models;
using Core.Omni.MVVM;
using Core.Omni.Utilities;
using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using ICSharpCode.SharpZipLib.Zip;
using OmniAntiCheat.EpicSDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace OmniAntiCheat.Windows {

	public class MainWindowVM : ViewModelBase {

		private const string MOSS_REGISTRY_PATH = @"SOFTWARE\Nohope92\Moss";
		private const string MOSS_EXE_VALUE = "LAST_PATH";
		private const string MOSS_LAST_LOG_VALUE = "LAST_LOG";
		private const string ROGUE_COMPANY_LAUNCH_URL = @"com.epicgames.launcher://apps/Pewee?action=launch&silent=true";
		private const string ROGUE_COMPANY_PROCESS_NAME = "RogueCompany";
		private const string NA_STRING = "N/A";
		private const string STARTING_STRING = "Starting...";
		private const string CLOSING_STRING = "Closing...";
		private const string RUNNING_STRING = "Running...";
		private const string FINDING_PROCESS = "Finding Process...";
		private const int GAME_POLLING_SECONDS = 3;
		private const int HEARTBEAT_UNCHANGED_INTERVAL = 30;
		private const int HEARTBEAT_POLLING_INTERVAL = 2;

		private string _mossLocation = null;
		private Process _mossCurrentProcess = null;
		private Process _gameCurrentProcess = null;
		private Action _onRogueCompanyClosed = null;
		private string _scrapedEpicID = "";
		private EpicClientWrapper _epicClient = null;
		private DateTime _lastHeartbeatSent = DateTime.MinValue;
		private bool _previousIsMossRunning = false;
		private bool _previousIsGameRunning = false;
		private IOmniAPI _omniAPI { get; }

		public string EpicID {
			get { return GetBindableProperty(() => EpicID, ""); }
			set { SetBindableProperty(() => EpicID, value); }
		}

		public bool IsLoggedIn {
			get { return GetBindableProperty(() => IsLoggedIn); }
			set { SetBindableProperty(() => IsLoggedIn, value); }
		}

		public bool IsLoggingIn {
			get { return GetBindableProperty(() => IsLoggingIn); }
			set { SetBindableProperty(() => IsLoggingIn, value); }
		}

		[BindableProperty(nameof(UploadStatusMessage), nameof(CanStartGame))]
		public bool IsUploadingToS3 {
			get { return GetBindableProperty(() => IsUploadingToS3, false); }
			set { SetBindableProperty(() => IsUploadingToS3, value); }
		}

		public string UploadStatusMessage {
			get {
				return IsUploadingToS3 ? "Uploading session information..." : "";
			}
		}

		[BindableProperty(nameof(CanStartGame))]
		public string MossStatus {
			get { return GetBindableProperty(() => MossStatus, NA_STRING); }
			set { SetBindableProperty(() => MossStatus, value); }
		}

		[BindableProperty(nameof(CanStartGame))]
		public string GameStatus {
			get { return GetBindableProperty(() => GameStatus, NA_STRING); }
			set { SetBindableProperty(() => GameStatus, value); }
		}

		public bool CanStartGame {
			get {
				return MossStatus == NA_STRING && GameStatus == NA_STRING && !IsUploadingToS3;
			}
		}

		public MainWindowVM(IOmniAPI omniAPI) {
			_omniAPI = omniAPI;
			CheckForMossInstallation();
			CheckScrapedEpicID();
			_epicClient = new EpicClientWrapper();
			TaskUtils.FireAndForget(StartPollForGameEvents());
			TaskUtils.FireAndForget(Login(LoginCredentialType.PersistentAuth, true));
		}

		///<summary>Starts Moss and the game client.</summary>
		public IAsyncCommand StartGameCommand => GetCommandAsync(() => StartGameCommand, true, async () => {
			if(_mossCurrentProcess != null || ProcessUtils.IsProcessRunning("moss")) {
				MessageBox.Show("Moss is currently running. Please close before proceeding.");
				return;
			}
			if(_gameCurrentProcess != null || ProcessUtils.IsProcessRunning("roguecompany")) {
				MessageBox.Show("Rougue Company is currently running. Please close before proceeding.");
				return;
			}
			MossStatus = STARTING_STRING;
			try {
				_mossCurrentProcess = Process.Start(_mossLocation, "ROG");
				_mossCurrentProcess.EnableRaisingEvents = true;
				_mossCurrentProcess.Exited += async (o, e) => {
					MossStatus = NA_STRING;
					_mossCurrentProcess = null;
					if(_gameCurrentProcess != null) {
						//They closed moss without closing the game. Bad...
						ExUtils.SwallowAnyException(() => {
							//Fuckem.
							_gameCurrentProcess.Kill();
						});
					}
					IsUploadingToS3 = true;
					await TransferSession();
					IsUploadingToS3 = false;
				};
				_onRogueCompanyClosed = () => {
					GameStatus = NA_STRING;
					if(_mossCurrentProcess != null) {
						MossStatus = CLOSING_STRING;
						_mossCurrentProcess.CloseMainWindow();
						_mossCurrentProcess.CloseMainWindow();
					}
				};
			}
			catch(Exception e) {
				MossStatus = NA_STRING;
				MessageBox.Show($"Error starting Moss. Error Message: {e.Message}");
				return;
			}
			await Task.Run(() => {
				//Blocking call.
				_mossCurrentProcess.WaitForInputIdle();
			});
			MossStatus = RUNNING_STRING;
			ProcessStartInfo psi = new ProcessStartInfo {
				FileName = ROGUE_COMPANY_LAUNCH_URL,
				UseShellExecute = true,
			};
			try {
				GameStatus = STARTING_STRING;
				//The process that returns from Process.Start is the Epic Launcher, not the rougue company process.
				Process.Start(psi);
			}
			catch(Exception e) {
				GameStatus = NA_STRING;
				_mossCurrentProcess?.CloseMainWindow();
				_mossCurrentProcess?.CloseMainWindow();
				MessageBox.Show($"Error starting Rogue Company. Please resolve the issue and try again. Error Message: {e.Message}");
				return;
			}
			GameStatus = FINDING_PROCESS;
		});

		///<summary>Starts Moss and the game client.</summary>
		public IAsyncCommand LoginCommand => GetCommandAsync(() => LoginCommand, true, async () => {
			await Login(LoginCredentialType.AccountPortal);
		});

		private async Task Login(LoginCredentialType credentialType, bool isSilent = false) {
			IsLoggingIn = true;
			try {
				LoginCallbackInfo retVal = await _epicClient.Login(credentialType);
				if(retVal?.ResultCode != Result.Success) {
					if(!isSilent) {
						MessageBox.Show($"Login unsuccessful. Error: {retVal?.ResultCode.ToString() ?? "NULL"}");
					}
					return;
				}
				retVal.LocalUserId.ToString(out string epicID);
				string username = _epicClient.GetUsername(retVal.LocalUserId);
				//Now we need to successfully hit our register user endpoint before letting them through.
				try {
					UpsertUserResponse response = await _omniAPI.UpsertUser(new UpsertUserRequest {
						User = new User {
							EpicID = epicID,
							Username = username,
						}
					});
					_omniAPI.AuthToken = response.AuthorizationToken;
				}
				catch(Exception e) {
					MessageBox.Show($"Unable to login with Omni API. {e.Message}");
					return;
				}
				EpicID = epicID;
				IsLoggedIn = true;
				//Start the heartbeats going.
				TaskUtils.FireAndForget(StartHeartbeatEvents());
				if(!string.IsNullOrWhiteSpace(_scrapedEpicID) && EpicID != _scrapedEpicID && !isSilent) {
					MessageBox.Show("Different Epic ID scraped then used to log in.");
				}
			}
			finally {
				IsLoggingIn = false;
			}
		}

		private void CheckScrapedEpicID() {
			string appDataPath = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName;
			string folderWithIDPath = Path.Combine(appDataPath, @"Local\EpicGamesLauncher\Saved\Data");
			if(Directory.Exists(folderWithIDPath)) {
				List<string> listFiles = Directory.GetFiles(folderWithIDPath).Select(x => Path.GetFileName(x)).ToList();
				List<string> listIds = listFiles
					.Select(x => Regex.Match(x, @"^([0-9a-zA-Z]{32})\.dat$"))
					.Where(x => x != null)
					.Select(x => x.Groups[1].Value)
					.Where(x => !string.IsNullOrWhiteSpace(x))
					.ToList();
				if(listIds.Count == 1) {
					_scrapedEpicID = listIds.First();
				}
			}
		}

		///<summary>Finds the latest session and attempts to parse/transfer the data to S3.</summary>
		private async Task TransferSession() {
			string lastPathValue = RegistryUtils.GetCurrentUserRegistryValue(MOSS_REGISTRY_PATH, MOSS_LAST_LOG_VALUE)?.Trim()?.Trim('\0');
			if(string.IsNullOrEmpty(lastPathValue)) {
				MessageBox.Show("Unable to find the logs for the latest session.");
				return;
			}
			if(!File.Exists(lastPathValue)) {
				MessageBox.Show($"Unable to transfer logs. File at: {lastPathValue} does not exist.");
				return;
			}
			ZipInputStream zipInputStream = null;
			try {
				try {
					zipInputStream = new ZipInputStream(File.OpenRead(lastPathValue));
				}
				catch(Exception e) {
					MessageBox.Show($"Unable to open zip file. Error Message: {e.Message}");
					return;
				}
				string amazonUploadUrl = "";
				try {
					GetS3UrlResponse response = await _omniAPI.GetS3UrlForLogs();
					amazonUploadUrl = response.SignedS3Url;
				}
				catch(Exception e) {
					MessageBox.Show($"Error retrieving URL: {e.Message}");
					return;
				}
				List<Func<Task>> listUploadTasks = new List<Func<Task>>();
				byte[] data = new byte[4096];
				ZipEntry zipEntry = null;
				List<string> listErrorMessages = new List<string>();
				while((zipEntry = zipInputStream.GetNextEntry()) != null) {
					string fileName = zipEntry.Name;
					int size = zipInputStream.Read(data, 0, data.Length);
					List<byte> fileBytes = new List<byte>();
					while(size > 0) {
						fileBytes.AddRange(data);
						size = zipInputStream.Read(data, 0, data.Length);
					}
					listUploadTasks.Add(async () => {
						try {
							await UploadBytesToAmazon(amazonUploadUrl, fileName, fileBytes.ToArray());
						}
						catch(Exception e) {
							listErrorMessages.Add($"File: {fileName} failed to upload. Error Message: {e.Message}");
						}
					});
				}
				await TaskUtils.WhenAll(listUploadTasks);
				if(listErrorMessages.IsNullOrEmpty()) {
					//Perfect upload. We can clean up the zip for them.
					ExUtils.SwallowAnyException(() => {
						File.Delete(lastPathValue);
					});
				}
				else {
					MessageBox.Show(string.Join("\r\n", listErrorMessages));
				}
			}
			finally {
				zipInputStream.Dispose();
			}
		}

		private async Task UploadBytesToAmazon(string amazonUrl, string fileName, byte[] bytes) {
			//Todo: Implement endpoint. May need to change to allow organizing in S3 such as organizing by date, match id, epic username, etc.
			await Task.CompletedTask;
		}

		///<summary>Begins indefinitely sending heartbeats to the server about the status of this client.</summary>
		private async Task StartHeartbeatEvents() {
			while(true) {
				DateTime timeNow = DateTime.Now;
				bool isMossRunning = MossStatus != NA_STRING;
				bool isGameRunning = GameStatus != NA_STRING;
				//Either it's been too long since the last heartbeat or something changed from last time.
				if(_lastHeartbeatSent.AddSeconds(HEARTBEAT_UNCHANGED_INTERVAL) < timeNow
					|| _previousIsMossRunning != isMossRunning
					|| _previousIsGameRunning != isGameRunning) 
				{
					//If this throws from the api call, it will not update the datetime or _previous variables.
					await ExUtils.SwallowAnyExceptionAsync(async () => {
						await _omniAPI.UploadUserInfo(new UploadUserInfoRequest {
							IsMossRunning = isMossRunning,
							IsRogueCompanyRunning = isGameRunning,
						});
						_lastHeartbeatSent = timeNow;
						_previousIsMossRunning = isMossRunning;
						_previousIsGameRunning = isGameRunning;
					});
				}
				await Task.Delay(TimeSpan.FromSeconds(HEARTBEAT_POLLING_INTERVAL));
			}
		}

		///<summary>Indefinitely polls for the game process closing and opening. We do not use the Process class events because technically the
		///Epic Launcher starts the process for us, so we do not own it.</summary>
		private async Task StartPollForGameEvents() {
			while(true) {
				Process matchingProcess = Process.GetProcessesByName(ROGUE_COMPANY_PROCESS_NAME).FirstOrDefault();
				if(matchingProcess != null && _gameCurrentProcess == null) {
					//Found the proces for the first time.
					if(GameStatus == FINDING_PROCESS) {
						GameStatus = RUNNING_STRING;
					}
				}
				else if(matchingProcess == null && _gameCurrentProcess != null) {
					//Process closed down.
					_onRogueCompanyClosed?.Invoke();
					_onRogueCompanyClosed = null;
				}
				_gameCurrentProcess = matchingProcess;
				await Task.Delay(TimeSpan.FromSeconds(GAME_POLLING_SECONDS));
			}
		}

		///<summary>Checks to ensure that moss is installed.</summary>
		private void CheckForMossInstallation() {
			_mossLocation = RegistryUtils.GetCurrentUserRegistryValue(MOSS_REGISTRY_PATH, MOSS_EXE_VALUE)?.Trim()?.Trim('\0');
			if(string.IsNullOrEmpty(_mossLocation) || !File.Exists(_mossLocation)) {
				MessageBox.Show("Moss is not installed. Please install and open Moss, and restart this program.");
				Application.Current.Shutdown();
			}
		}

	}

}
