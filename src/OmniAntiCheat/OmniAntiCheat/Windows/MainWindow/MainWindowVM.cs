using Core.Omni.MVVM;
using Core.Omni.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;

namespace OmniAntiCheat.Windows {

	public class MainWindowVM : ViewModelBase {

		private const string MOSS_REGISTRY_PATH = @"SOFTWARE\Nohope92\Moss";
		private const string MOSS_EXE_VALUE = @"LAST_PATH";

		private string _mossLocation = null;
		private Process _mossCurrentProcess = null;

		public MossStatus CurrentMossStatus {
			get { return GetBindableProperty(() => CurrentMossStatus, MossStatus.NotRunning); }
			set { SetBindableProperty(() => CurrentMossStatus, value); }
		}

		public MainWindowVM() {
			CheckForMossInstallation();
		}

		public IAsyncCommand StartMossCommand => GetCommand(() => StartMossCommand, () => {
			if(_mossCurrentProcess != null) {
				return;
			}
			try {
				_mossCurrentProcess = Process.Start(_mossLocation, "ROG");
				CurrentMossStatus = MossStatus.Running;
				_mossCurrentProcess.EnableRaisingEvents = true;
				_mossCurrentProcess.Exited += (o, e) => {
					CurrentMossStatus = MossStatus.NotRunning;
					_mossCurrentProcess = null;
				};
				Thread.Sleep(15000);
				//The first close stops recording. The second close tells it to close even closier.
				_mossCurrentProcess.CloseMainWindow();
				_mossCurrentProcess.CloseMainWindow();
			}
			catch(Exception e) {

			}
		});

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
