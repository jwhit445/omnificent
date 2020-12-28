using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Extensions;

namespace Core {
	public static class TaskUtils {

		////<summary>The private instance for the lock.</summary>
		private static List<Task> _listFiredTasks=new List<Task>();
		///<summary>A global list of fired tasks. This is ONLY to be used in Unit testing. If you are using this in production, something is going wrong.</summary>
		private static object _lockList=new object();

		///<summary>Fires off the task without awaiting on the result. This is the only async void method that we should ever have in our program. Async void methods
		///throw their exceptions directly on the main context (thread) as they are not awaited. Not only this, but the stack trace for an async void exception gets
		///messed up when it is thrown. To prevent this, we should always catch these cases. Generally, this fire and forget method will be used for event handlers,
		///aka the top of the async call stack.</summary>
		private static async void AwaitTask(Task task) {
			if(BuildUtils.IsDebug()) {
				lock(_lockList) {
					_listFiredTasks.Add(task);
				}
			}
			try {
				await task;
			}
			catch(Exception e) {
				if(BuildUtils.IsDebug()) {
					string exceptionText = ExUtils.GetExceptionText(e);
					Console.WriteLine(exceptionText);
					//Odds are something went terribly wrong and there is no good way to figure it out from a task without breaking the application.
					//For this reason, we are inserting a forced breakpoint to allow the programmer to see what unexpected issue happened.
					if(Debugger.IsAttached) {
						Debugger.Break();
					}
				}
				//Handle the Exception some how. A bug submission or some custom handler.
				throw;
			}
			finally {
				if(BuildUtils.IsDebug()) {
					lock(_lockList) {
						_listFiredTasks.Remove(task);
					}
				}
			}
		}

		///<summary>Fires off the task without awaiting on the result. This is the only async void method that we should ever have in our program. Async void methods
		///throw their exceptions directly on the main context (thread) as they are not awaited. Not only this, but the stack trace for an async void exception gets
		///messed up when it is thrown. To prevent this, we should always catch these cases. Generally, this fire and forget method will be used for event handlers,
		///aka the top of the async call stack.</summary>
		public static void FireAndForget(Func<Task> funcTask) {
			FireAndForget(funcTask());
		}

		///<summary>Fires off the task without awaiting on the result. This is the only async void method that we should ever have in our program. Async void methods
		///throw their exceptions directly on the main context (thread) as they are not awaited. Not only this, but the stack trace for an async void exception gets
		///messed up when it is thrown. To prevent this, we should always catch these cases. Generally, this fire and forget method will be used for event handlers,
		///aka the top of the async call stack.</summary>
		public static void FireAndForget(Task task) {
			if(task==null) {
				return;
			}
			AwaitTask(task);
		}

		///<summary>For each item in the list, will run the given func all in parallel.</summary>
		public static Task RunInParallel<T>(IEnumerable<T> enumerable ,Func<T ,Task> func ,bool onThreadPool = false) {
			List<Func<Task>> listToRun=new List<Func<Task>>();
			foreach(T item in enumerable) {
				listToRun.Add(async () => {
					if(onThreadPool) {
						await Task.Run(async () => {
							await func(item);
						});
					}
					else {
						await func(item);
					}
				});
			}
			return WhenAll(listToRun);
		}

		///<summary>Returns when all of the given tasks have finished.</summary>
		public static Task WhenAll(IEnumerable<Func<Task>> tasks) {
			if(tasks.IsNullOrEmpty()) {
				return Task.CompletedTask;
			}
			return WhenAll(tasks.ToArray());
		}

		///<summary>Returns when all of the given tasks have finished.</summary>
		public static Task WhenAll(params Func<Task>[] tasks) {
			if(tasks.IsNullOrEmpty()) {
				return Task.CompletedTask;
			}
			return Task.WhenAll(tasks.Select(x => x()));
		}

		///<summary>Returns when any of the given tasks have finished.</summary>
		public static Task WhenAny(params Func<Task>[] tasks) {
			if(tasks.IsNullOrEmpty()) {
				return Task.CompletedTask;
			}
			return Task.WhenAny(tasks.Select(x => x()));
		}

		///<summary>TESTING ONLY. Continually loops through fired tasks and returns when all are completed. If one fired and forgot task fires another one off, this
		///method will wait till both are complete.</summary>
		public static async Task WhenAllFiredTasksFinish() {
			if(!BuildUtils.IsDebug()) {
				throw new ApplicationException($@"Calling {nameof(WhenAllFiredTasksFinish)} in non-Debug mode.");
			}
			while(true) {
				List<Task> listTasksToAwait;
				lock(_lockList) {
					if(_listFiredTasks.Count==0) {
						break;
					}
					listTasksToAwait=new List<Task>(_listFiredTasks);
				}
				await Task.WhenAll(listTasksToAwait);
			}
		}
	}
}
