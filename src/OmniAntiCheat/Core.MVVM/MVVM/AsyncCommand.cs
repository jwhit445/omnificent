using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Core.Omni.MVVM {
	///<summary>The real implementation of IAsyncCommand.</summary>
	public class AsyncCommand : IAsyncCommand {

		///<summary>An event that is fired when CanExecute has changed.</summary>
		public event EventHandler CanExecuteChanged;
		///<summary>The Task to run on execution.</summary>
		private readonly Func<Task> _execute;
		///<summary>A custom implementation for if the command can execute.</summary>
		private readonly Func<bool> _canExecute;
		///<summary>Indicates if the command can be reentered while it is still running.</summary>
		private readonly bool _doBlockReentrance;
		///<summary>Indicates if the command is currently executing.</summary>
		private bool _isExecuting = false;

		[Obsolete("Are you sure you want to call the constructor? Use ViewModelBase.GetCommand functions instead.")]
		public AsyncCommand(Func<Task> execute, bool doBlockReentrance, Func<bool> canExecute = null) {
			_execute = execute;
			_canExecute = canExecute;
			_doBlockReentrance = doBlockReentrance;
		}

		///<summary>Indicates if the command can be executed.</summary>
		public bool CanExecute() {
			return _canExecute?.Invoke()??true;
		}

		///<summary>Executes the command.</summary>
		public async Task ExecuteAsync() {
			if(CanExecute() && !(_isExecuting&&_doBlockReentrance)) {
				try {
					_isExecuting = true;
					await _execute();
				}
				finally {
					_isExecuting = false;
				}
			}
		}

		public void ExecuteChanged() {
			CanExecuteChanged?.Invoke(this, new EventArgs());
		}

		#region ICommand Implementations

		bool ICommand.CanExecute(object parameter) {
			return CanExecute();
		}

		void ICommand.Execute(object parameter) {
			//If called via ICommand.Execute, it is probably being autocalled by the Xaml. We should FireAndForget this.
			TaskUtils.FireAndForget(ExecuteAsync);
		}

		#endregion
	}

	///<summary>The real implementation of IAsyncCommand.</summary>
	public class AsyncCommand<T> : IAsyncCommand<T> {

		///<summary>An event that is fired when CanExecute has changed.</summary>
		public event EventHandler CanExecuteChanged;
		///<summary>The Task to run on execution.</summary>
		private readonly Func<T, Task> _execute;
		///<summary>A custom implementation for if the command can execute.</summary>
		private readonly Func<T, bool> _canExecute;
		///<summary>Indicates if the command can be reentered while it is still running.</summary>
		private readonly bool _doBlockReentrance;
		///<summary>Indicates if the command is currently executing.</summary>
		private bool _isExecuting = false;

		[Obsolete("Are you sure you want to call the constructor? Use ViewModelBase.GetCommand functions instead.")]
		public AsyncCommand(Func<T, Task> execute ,bool doBlockReentrance ,Func<T ,bool> canExecute = null) {
			_execute = execute;
			_canExecute = canExecute;
			_doBlockReentrance = doBlockReentrance;
		}

		///<summary>Indicates if the command can be executed.</summary>
		public bool CanExecute(T parameter) {
			return _canExecute?.Invoke(parameter)??true;
		}

		///<summary>Executes the command.</summary>
		public async Task ExecuteAsync(T parameter) {
			if(CanExecute(parameter) || !(_isExecuting&&_doBlockReentrance)) {
				try {
					_isExecuting = true;
					await _execute(parameter);
				}
				finally {
					_isExecuting = false;
				}
			}
		}

		public void ExecuteChanged() {
			CanExecuteChanged?.Invoke(this, new EventArgs());
		}

		#region ICommand Implementations

		bool ICommand.CanExecute(object parameter) {
			if(parameter != null && !(parameter is T)) {
				throw new ApplicationException($"AsyncCommand<T>.CanExecute. Parameter is not a T. Excpected: {typeof(T).Name} Received: {parameter.GetType().Name}");
			}
			return CanExecute((T)parameter);
		}

		void ICommand.Execute(object parameter) {
			if(parameter != null && !(parameter is T)) {
				throw new ApplicationException($"AsyncCommand<T>.Execute. Parameter is not a T. Excpected: {typeof(T).Name} Received: {parameter.GetType().Name}");
			}
			//If called via ICommand.Execute, it is probably being autocalled by the Xaml. We should FireAndForget this.
			TaskUtils.FireAndForget(ExecuteAsync((T)parameter));
		}

		#endregion
	}
}
