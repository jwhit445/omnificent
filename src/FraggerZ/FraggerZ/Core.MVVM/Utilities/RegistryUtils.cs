using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Omni.Utilities {
	public class RegistryUtils {

		public static string GetCurrentUserRegistryValue(string path, string key) {
			RegistryKey rootKey = Registry.CurrentUser.OpenSubKey(path);
			return rootKey?.GetValue(key) as string;
		}

	}
}
