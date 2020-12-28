using Core;
using Core.Extensions;
using Core.Omni.MVVM;
using Core.Omni.Utilities;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OmniAntiCheat.Windows {

	public class MainWindowVM : ViewModelBase {

		private const string MOSS_REGISTRY_PATH = @"SOFTWARE\Nohope92\Moss";
		private const string MOSS_EXE_VALUE = "LAST_PATH";
		private const string MOSS_LAST_LOG_VALUE = "LAST_LOG";
		private const string ROGUE_COMPANY_LAUNCH_URL = @"com.epicgames.launcher://apps/Pewee?action=launch&silent=true";
		private const string ROGUE_COMPANY_PROCESS_NAME = "RogueCompany";
		private const int GAME_POLLING_SECONDS = 3;

		private string _mossLocation = null;
		private Process _mossCurrentProcess = null;
		private Process _gameCurrentProcess = null;
		private Action _onRogueCompanyClosed = null;

		public MossStatus CurrentMossStatus {
			get { return GetBindableProperty(() => CurrentMossStatus, MossStatus.NotRunning); }
			set { SetBindableProperty(() => CurrentMossStatus, value); }
		}

		public string GameStatusMessage {
			get { return GetBindableProperty(() => GameStatusMessage, "N/A"); }
			set { SetBindableProperty(() => GameStatusMessage, value); }
		}

		public string LoginStatusMessage {
			get { return GetBindableProperty(() => LoginStatusMessage, "Not logged in to Epic."); }
			set { SetBindableProperty(() => LoginStatusMessage, value); }
		}

		public MainWindowVM() {
			CheckForMossInstallation();
			TaskUtils.FireAndForget(StartPollForGameEvents());
		}

		///<summary>Starts Moss and the game client.</summary>
		public IAsyncCommand StartGameCommand => GetCommandAsync(() => StartGameCommand, true, async () => {
			if(_mossCurrentProcess != null) {
				MessageBox.Show("Moss is currently running. Please close before proceeding.");
				return;
			}
			if(_gameCurrentProcess != null) {
				MessageBox.Show("Rougue Company is currently running. Please close before proceeding.");
				return;
			}
			GameStatusMessage = "Starting Moss...";
			try {
				_mossCurrentProcess = Process.Start(_mossLocation, "ROG");
				CurrentMossStatus = MossStatus.Running;
				_mossCurrentProcess.EnableRaisingEvents = true;
				_mossCurrentProcess.Exited += async (o, e) => {
					CurrentMossStatus = MossStatus.NotRunning;
					_mossCurrentProcess = null;
					GameStatusMessage = "Uploading session logs...";
					await TransferSession();
					GameStatusMessage = "N/A";
				};
				_onRogueCompanyClosed = () => {
					GameStatusMessage = "Moss is closing...";
					_mossCurrentProcess?.CloseMainWindow();
					_mossCurrentProcess?.CloseMainWindow();
				};
			}
			catch(Exception e) {
				GameStatusMessage = "N/A";
				MessageBox.Show($"Error starting Moss. Error Message: {e.Message}");
				return;
			}
			await Task.Run(() => {
				//Blocking call.
				_mossCurrentProcess.WaitForInputIdle();
			});
			ProcessStartInfo psi = new ProcessStartInfo {
				FileName = ROGUE_COMPANY_LAUNCH_URL,
				UseShellExecute = true,
			};
			try {
				GameStatusMessage = "Starting Rogue Company...";
				//The process that returns from Process.Start is the Epic Launcher, not the rougue company process.
				Process.Start(psi);
			}
			catch(Exception e) {
				GameStatusMessage = "N/A";
				_mossCurrentProcess?.CloseMainWindow();
				_mossCurrentProcess?.CloseMainWindow();
				MessageBox.Show($"Error starting Rogue Company. Please resolve the issue and try again. Error Message: {e.Message}");
				return;
			}
			GameStatusMessage = "Moss and Rogue Company running...";
		});

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
							await UploadBytesToAmazon(fileName, fileBytes.ToArray());
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

		private async Task UploadBytesToAmazon(string fileName, byte[] bytes) {
			//Todo: Implement endpoint. May need to change to allow organizing in S3 such as organizing by date, match id, epic username, etc.
			await Task.CompletedTask;
		}

		///<summary>Indefinitely polls for the game process closing and opening. We do not use the Process class events because technically the
		///Epic Launcher starts the process for us, so we do not own it.</summary>
		private async Task StartPollForGameEvents() {
			while(true) {
				Process matchingProcess = Process.GetProcessesByName(ROGUE_COMPANY_PROCESS_NAME).FirstOrDefault();
				if(matchingProcess != null && _gameCurrentProcess == null) {
					//Found the proces for the first time.
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

	public enum MossStatus {
		NotRunning,
		Running,
	}
}
