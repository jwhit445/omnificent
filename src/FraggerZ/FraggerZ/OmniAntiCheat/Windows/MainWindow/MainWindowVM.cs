using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Core;
using Core.Omni.API;
using Core.Omni.API.Models;
using Core.Omni.MVVM;
using Core.Omni.Utilities;

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
		private const string AWS_USER = "AKIASNWLQTJB7LD6PPDP";
		private const string AWS_SECRET = "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";
		private const string AWS_BUCKET_NAME = "omnificent-api-alpha-upload-log-events";
		private const int GAME_POLLING_SECONDS = 3;
		private const int HEARTBEAT_UNCHANGED_INTERVAL = 30;
		private const int HEARTBEAT_POLLING_INTERVAL = 2;

		public static MainWindowVM Inst { get; set; }

		private string _mossLocation = null;
		private Process _mossCurrentProcess = null;
		private Process _gameCurrentProcess = null;
		private Action _onRogueCompanyClosed = null;
		private DateTime _lastHeartbeatSent = DateTime.MinValue;
		private DateTime _lastMossStart = DateTime.MinValue;
		private bool _previousIsMossRunning = false;
		private bool _previousIsGameRunning = false;
		private RegionEndpoint AmazonEndpoint { get; } = RegionEndpoint.USEast2;
		private IOmniAPI _omniAPI { get; }

		public string EpicID {
			get { return GetBindableProperty(() => EpicID, ""); }
			set { SetBindableProperty(() => EpicID, value); }
		}

		public string EpicUsername {
			get { return GetBindableProperty(() => EpicUsername, ""); }
			set { SetBindableProperty(() => EpicUsername, value); }
		}

		public PlatformCode PlatformCode {
			get { return GetBindableProperty(() => PlatformCode, PlatformCode.Unknown); }
			set { SetBindableProperty(() => PlatformCode, value); }
		}

		public GameType GameType {
			get { return GetBindableProperty(() => GameType, GameType.Unknown); }
			set { SetBindableProperty(() => GameType, value); }
		}

		public bool IsLoggingIn {
			get { return GetBindableProperty(() => IsLoggingIn, true); }
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

		public bool HasDisallowedSettings {
			get {
				return !string.IsNullOrWhiteSpace(DisallowedSettingsMessage);
			}
		}

		[BindableProperty(nameof(HasDisallowedSettings), nameof(CanStartGame))]
		public string DisallowedSettingsMessage {
			get { return GetBindableProperty(() => DisallowedSettingsMessage); }
			set { SetBindableProperty(() => DisallowedSettingsMessage, value); }
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
				return MossStatus == NA_STRING 
					&& GameStatus == NA_STRING 
					&& !IsUploadingToS3 
					&& !HasDisallowedSettings 
					&& string.IsNullOrWhiteSpace(DisallowedSettingsMessage);
			}
		}

		public MainWindowVM(IOmniAPI omniAPI) {
			Inst = this;
			_omniAPI = omniAPI;
            if(!IsMossInstalled(out string errorMessage)
                || !IsLatestOmniAntiCheatVersion(out errorMessage)
                || !IsLatestMossVersion(out errorMessage))
			{
                MessageBox.Show($"Unable to start Omni Anti Cheat: \r\nError: { errorMessage }");
                Application.Current.Shutdown();
                return;
            }
            if(!TryGetScrapedEpicInfo(out errorMessage)) {
				MessageBox.Show($"Unable to find Epic user information.\r\nError: { errorMessage }");
				Environment.Exit(0);
				return;
			}
			CheckDisallowedSettings();
            TaskUtils.FireAndForget(StartPollForGameEvents());
            TaskUtils.FireAndForget(Login());

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
			_lastMossStart = DateTime.Now;
			MossStatus = STARTING_STRING;
			try {
				_mossCurrentProcess = Process.Start(_mossLocation, "ROG");
				_mossCurrentProcess.EnableRaisingEvents = true;
				_mossCurrentProcess.Exited += async (o, e) => {
					IsUploadingToS3 = true;
					MossStatus = NA_STRING;
					_mossCurrentProcess = null;
					if(_gameCurrentProcess != null) {
						//They closed moss without closing the game. Bad...
						ExUtils.SwallowAnyException(() => {
							//Fuckem.
							_gameCurrentProcess.Kill();
						});
					}
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

		private bool TryGetScrapedEpicInfo(out string errorMessage) {
			string appDataPath = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName;
			string logsFolder = Path.Combine(appDataPath, @"Local\EpicGamesLauncher\Saved\Logs");
			if(!Directory.Exists(logsFolder)) {
				errorMessage = "Unable to find log folder.";
				return false;
			}
			List<string> listFiles = Directory
				.GetFiles(logsFolder)
				.OrderByDescending(x => File.GetLastAccessTime(x))
				.ToList();
			foreach(string filePath in listFiles) {
                using FileStream logFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using StreamReader logFileReader = new StreamReader(logFileStream);
                string fileContents = logFileReader.ReadToEnd();
                MatchCollection matches = Regex.Matches(fileContents, @".*-epicusername=(.*) -epicuserid=([^\s]*)");
                if (matches != null && matches.Count > 0) {
                    Match match = matches[matches.Count - 1];
                    EpicUsername = match.Groups[1].Value.Trim('\"');
                    EpicID = match.Groups[2].Value;
                    _omniAPI.EpicID = EpicID;
                    errorMessage = "";
					PlatformCode = PlatformCode.Epic;
					_omniAPI.PlatformCode = PlatformCode.Epic;
					GameType = GameType.RogueCompany;
					return true;
                }
            }
			errorMessage = "Unable to find ID in logs.";
			return true;
		}

		///<summary>Login with Omni API.</summary>
		private async Task Login() {
			if(PlatformCode == PlatformCode.Unknown) {
				throw new Exception("UNKNOWN PLATFORM CODE.");
            }
			try {
				UpsertUserResponse response = await _omniAPI.UpsertUser(new UpsertUserRequest {
					User = new User {
						ID = EpicID,
						Username = EpicUsername,
						PlatformCode = PlatformCode,
					},
				});
				_omniAPI.AuthToken = response.AuthorizationToken;
			}
			catch(Exception e) {
				MessageBox.Show($"Unable to login with Omni API. {e.Message}");
				Environment.Exit(0);
				return;
			}
			IsLoggingIn = false;
			//Start the heartbeats going.
			TaskUtils.FireAndForget(StartHeartbeatEvents());
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
			string s3URL;
			try {
				s3URL = await UploadZipToAmazon(lastPathValue);
			}
			catch(Exception e) {
				MessageBox.Show($"Error uploading logs: {e.Message}");
				return;
			}
			try {
				await _omniAPI.CreateLogEvent(new CreateLogEventRequest {
					S3Url = s3URL,
					DateTimeStarted = _lastMossStart.ToUniversalTime(), 
				});
			}
			catch(Exception e) {
				MessageBox.Show($"Error creating log event: {e.Message}");
				return;
			}
        }

		public void OnExit() {
			Task.Run(async () => {
				if(_mossCurrentProcess != null) {
					MossStatus = CLOSING_STRING;
					_mossCurrentProcess.CloseMainWindow();
					_mossCurrentProcess.CloseMainWindow();
					_mossCurrentProcess.WaitForExit();
					_gameCurrentProcess?.WaitForExit();
				}
				do {
					await Task.Delay(100);
				} while(IsUploadingToS3);
			}).GetAwaiter().GetResult();
		}

		///<summary>Uploads the zip to amazon. Returns the publicly available S3 URL.</summary>
		private async Task<string> UploadZipToAmazon(string fullZipPath) {
			AmazonS3Client client = new AmazonS3Client(AWS_USER, AWS_SECRET, AmazonEndpoint);
			TransferUtility transferUtility = new TransferUtility(client);
			string filePath = $"{EpicID}/{DateTime.Today:yyyy-MM-dd}/{Path.GetFileName(fullZipPath)}";
			await transferUtility.UploadAsync(fullZipPath, AWS_BUCKET_NAME, filePath);
			return $"https://{AWS_BUCKET_NAME}.s3.{AmazonEndpoint.SystemName}.amazonaws.com/{filePath}";
		}

		private async Task UploadBytesToAmazon(string fileName, byte[] bytes) {
			AmazonS3Client client = new AmazonS3Client(AWS_USER, AWS_SECRET, AmazonEndpoint);
			TransferUtility transferUtility = new TransferUtility(client);
			using MemoryStream ms = new MemoryStream(bytes);
			await transferUtility.UploadAsync(ms, AWS_BUCKET_NAME, $"{EpicID}/{DateTime.Today.ToString("yyyy-MM-dd")}/{fileName}");
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
							IsGameRunning = isGameRunning,
							GameType = GameType,
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
		private bool IsMossInstalled(out string errorMsg) {
			errorMsg = "";
			_mossLocation = RegistryUtils.GetCurrentUserRegistryValue(MOSS_REGISTRY_PATH, MOSS_EXE_VALUE)?.Trim()?.Trim('\0');
			if(string.IsNullOrEmpty(_mossLocation) || !File.Exists(_mossLocation)) {
				errorMsg = "Moss is not installed. Please install and open Moss, and restart this program.";
				return false;
			}
			return true;
		}

		///<summary>Checks to ensure that Moss is the latest version.</summary>
		private bool IsLatestMossVersion(out string errorMsg) {
			errorMsg = "";
			//TODO: implement
			return true;
		}

		///<summary>Checks to ensure that Omni Anti Cheat is the latest version.</summary>
		private bool IsLatestOmniAntiCheatVersion(out string errorMsg) {
			errorMsg = "";
			GetLatestAntiCheatVersionResult result = null;
			Task.Run(async () => {
				 result = await _omniAPI.GetLatestAntiCheatVersion();
			}).GetAwaiter().GetResult();
			if(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version == result.Version) {
				return true;
            }
			errorMsg = "Your Omnificient Anti Cheat is not on the latest version. Please update and try again.";
			return false;
		}

		///<summary>Checks to ensure that Omni Anti Cheat is the latest version.</summary>
		private void CheckDisallowedSettings() {
			//TODO: implement
			string appDataPath = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName;
			string configFolder = Path.Combine(appDataPath, @"Local\RogueCompany\Saved\Config\WindowsNoEditor");
			//DisallowedSettingsMessage = $"Disallowed settings found:\r\nIn {configFolder}: \r\n\r\nIS_CHEATING=1. Expected: 0.";
		}

	}

}
