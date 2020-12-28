using System;
using System.Threading.Tasks;

namespace Core {
	public static class ExUtils {

		///<summary>Swallows any exception that occurs in the given action.</summary>
		public static void SwallowAnyException(Action action) {
			try {
				action();
			}
			catch(Exception) {
				//Do nothing.
			}
		}

		///<summary>Swallows any exception that occurs in the given asynchronous action.</summary>
		public static async Task SwallowAnyExceptionAsync(Func<Task> func) {
			try {
				await func();
			}
			catch(Exception) {
				//Do nothing.
			}
		}

		///<summary>Returns exception string that includes the exception type and up to 5 inner exceptions.</summary>
		public static string GetExceptionText(Exception e) {
			string text="Unhandled exception:  "
				+(string.IsNullOrEmpty(e.Message)?"No Exception Message":e.Message+"\r\n")
				+(string.IsNullOrEmpty(e.GetType().ToString())?"No Exception Type":e.GetType().ToString())+"\r\n"
				+(string.IsNullOrEmpty(e.StackTrace)?"No StackTrace":e.StackTrace);
			if(e is AggregateException) {
				foreach(Exception innerEx in ((AggregateException)e).InnerExceptions) {
					text+=InnerExceptionToString(innerEx);
				}
			}
			else {
				text+=InnerExceptionToString(e.InnerException);//New lines handled in method.
			}
			return text;
		}

		///<summary>Formats the inner exception (and all its inner exceptions) as a readable string. Okay to pass in an exception with no inner 
		///exception.</summary>
		///<param name="depth">The recursive depth of the current method call.</param>
		private static string InnerExceptionToString(Exception innerEx ,int depth = 0) {
			if(innerEx==null
				||depth>=5)//Limit to 5 inner exceptions to prevent infinite recursion
			{
				return "";
			}
			return "\r\n-------------------------------------------\r\n"
				+"Inner exception:  "+innerEx.Message+"\r\n"+innerEx.GetType().ToString()+"\r\n"
				+innerEx.StackTrace
				+InnerExceptionToString(innerEx.InnerException ,++depth);
		}

	}
}
