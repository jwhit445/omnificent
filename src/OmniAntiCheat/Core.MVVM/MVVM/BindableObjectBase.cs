using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Core.Extensions;
using Prism.Mvvm;

namespace Core.Omni.MVVM {
	public class BindableObjectBase : BindableBase {

		#region Get/Set Bindable Properties

		///<summary>A dictionary that contains the private properties for bindable public properties. Using this dictionary avoids having to
		///have a public/private variable for every bindable property.</summary>
		private readonly ConcurrentDictionary<string,object> _dictionaryPrivateProperties = new ConcurrentDictionary<string,object>();

		///<summary>Gets the private instance of the public, bindable property.</summary>
		///<param name="defaultValue">The default value to return if the property has never been set before.</param>
		protected T GetBindableProperty<T>(Expression<Func<T>> property ,T defaultValue = default) {
			return (T)_dictionaryPrivateProperties.GetOrAdd(MemberUtils.GetMemberInfo(property).Name ,defaultValue);
		}

		///<summary>Sets the given value to the private instance of the public, bindable property. Automatically calls RaisePropertyChanged when using
		///this method.</summary>
		protected void SetBindableProperty<T>(Expression<Func<T>> property ,T value) {
			_dictionaryPrivateProperties[MemberUtils.GetMemberInfo(property).Name]=value;
			//Calling RaisePropertyChanged will call OnPropertyChanged for all the dependent properties.
			RaisePropertyChanged(property);
		}

		///<summary>Fires INotifyPropertyChanged.OnPropertyChanged for this given property Expression. 
		///Call this from a setter property to notify bindables that this property has changed.</summary>
		public void RaisePropertyChanged<T>(Expression<Func<T>> property) {
			MemberInfo member=MemberUtils.GetMemberInfo(property);
#pragma warning disable 0618
			OnPropertyChanged(new PropertyChangedEventArgs(member.Name));
#pragma warning restore 0618
			BindablePropertyAttribute attr=member.GetCustomAttribute<BindablePropertyAttribute>();
			if(attr!=null && !attr.ListDependentPropertyNames.IsNullOrEmpty()) {
				//Raise property changed on any dependent properties.
				attr.ListDependentPropertyNames.ForEach(x => RaisePropertyChanged(x));
			}
		}

		///<summary>Fires INotifyPropertyChanged.OnPropertyChanged for this given property Expression. 
		///Call this from a setter property to notify bindables that this property has changed.</summary>
		public new void RaisePropertyChanged(string propertyName) {
			PropertyInfo prop = GetType().GetProperty(propertyName);
#pragma warning disable 0618
			OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
#pragma warning restore 0618
			BindablePropertyAttribute attr=prop.GetCustomAttribute<BindablePropertyAttribute>();
			if(attr!=null && !attr.ListDependentPropertyNames.IsNullOrEmpty()) {
				//Raise property changed on any dependent properties.
				attr.ListDependentPropertyNames.ForEach(x => RaisePropertyChanged(x));
			}
		}

		#endregion

		#region GetAsyncCommands

#pragma warning disable 612, 618
		///<summary>Gets the command for the given property. There should be NO async code in here. This command cannot be reentered as the code is run
		///synchronously.</summary>
		protected IAsyncCommand GetCommand(Expression<Func<IAsyncCommand>> property, Action action) {
			return GetBindableProperty(property ,new AsyncCommand(() => { action(); return Task.CompletedTask; } ,true));
		}

		[Obsolete("Using GetCommand with an async () => {}. Use GetCommandAsync instead." ,true)]
		protected IAsyncCommand GetCommand(Expression<Func<IAsyncCommand>> property, Func<Task> task) {
			throw new ApplicationException("Using GetCommand with an async () => {}. Use GetCommandAsync instead.");
		}

		///<summary>Gets the command for the given property. Indicate if the user should be able to fire off the command while it is still running.</summary>
		protected IAsyncCommand GetCommandAsync(Expression<Func<IAsyncCommand>> property, bool blockReentrance, Func<Task> task, Func<bool> canExecute = null) {
			return GetBindableProperty(property, new AsyncCommand(task, blockReentrance, canExecute));
		}

		///<summary>Gets the command for the given property. There should be NO async code in here. This command cannot be reentered as the code is run
		///synchronously.</summary>
		protected IAsyncCommand<T> GetCommand<T>(Expression<Func<IAsyncCommand<T>>> property ,Action<T> action) {
			return GetBindableProperty(property, new AsyncCommand<T>((param) => { action(param); return Task.CompletedTask; } ,true));
		}

		[Obsolete("Using GetCommand<T> with an async () => {}. Use GetCommandAsync instead." ,true)]
		protected IAsyncCommand GetCommand<T>(Expression<Func<IAsyncCommand<T>>> property ,Func<T ,Task> task) {
			throw new ApplicationException("Using GetCommand<T> with an async () => {}. Use GetCommandAsync instead.");
		}

		///<summary>Gets the command for the given property. Indicate if the user should be able to fire off the command while it is still running.
		///NOTE: For commands that have no asynchronous code, reentrance has no effect. IAsyncCommands will always be called on the UI thread.</summary>
		protected IAsyncCommand<T> GetCommandAsync<T>(Expression<Func<IAsyncCommand<T>>> property ,bool blockReentrance ,Func<T ,Task> task) {
			return GetBindableProperty(property ,new AsyncCommand<T>(task ,blockReentrance));
		}
#pragma warning restore 612, 618
		#endregion

	}
}
