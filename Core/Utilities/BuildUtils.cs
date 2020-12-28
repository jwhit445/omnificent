using System;
namespace Core {

	public class BuildUtils {
		///<summary>Indicates if the current session is running with the DEBUG compilation flag.</summary>
		public static bool IsDebug() {
#if DEBUG
			return true;
#else
			return false;
#endif
		}
	}

}
