using System.Threading.Tasks;
using System.Windows.Input;

namespace Core.Omni.MVVM {

	///<summary>An Async Command that can be binded in XAML. When not being called by XAML events, it exposes the task
	///to be awaited on. This allows custom controls to know when an async event really finished. This also allows simpler unit testing.</summary>
	public interface IAsyncCommand : ICommand {
		///<summary>The task to occur when executing the command.</summary>
		Task ExecuteAsync();
		///<summary>Indicates if this command can be executed.</summary>
		bool CanExecute();
		///<summary>Fires the execute changed event.</summary>
		void ExecuteChanged();
	}

	///<summary>Same as above but with a command parameter.</summary>
	public interface IAsyncCommand<T> : ICommand {
		///<summary>The task to occur when executing the command.</summary>
		Task ExecuteAsync(T parameter);
		///<summary>Indicates if this command can be executed.</summary>
		bool CanExecute(T parameter);
		///<summary>Fires the execute changed event.</summary>
		void ExecuteChanged();
	}

}
