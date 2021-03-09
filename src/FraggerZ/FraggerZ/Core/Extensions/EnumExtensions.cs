using System;
using System.ComponentModel;
using System.Reflection;

namespace Core.Extensions {
	public static class EnumExtensions {

		///<summary>Returns the enums description if there is one. Otherwise, returns enum.ToString().</summary>
		public static string GetDescription(this Enum value) {
			FieldInfo fieldInfo = value.GetType().GetField(value.ToString());
			DescriptionAttribute[] attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
			if(!attributes.IsNullOrEmpty()) {
				return attributes[0].Description;
			}
			else {
				return value.ToString();
			}
		}

	}
}
