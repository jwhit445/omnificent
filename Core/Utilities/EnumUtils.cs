using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Core {

	public class EnumUtils {

		///<summary>Tries to parse the given value to match the description of any enum values of type T. If unable to find any values that match the description, it will 
		///attempt to normally parse the value.</summary>
		public static T ParseWithDescription<T>(string enumVal, T defaultValue) where T : struct,Enum {
			if(string.IsNullOrEmpty(enumVal)) {
				throw new ArgumentException($"Invalid argument in {nameof(ParseWithDescription)}.", nameof(enumVal));
			}
			enumVal = enumVal.ToLower();
			foreach(T enumInst in Enum.GetValues(typeof(T))) {
				FieldInfo fieldInfo = typeof(T).GetField(enumInst.ToString());
				if(fieldInfo == null) {
					continue;
				}
				DescriptionAttribute attr = fieldInfo.GetCustomAttribute<DescriptionAttribute>();
				if(attr == null) {
					continue;
				}
				if(attr.Description.ToLower() == enumVal) {
					return enumInst;
				}
			}
			//No matching description. Lets parse like normal.
			if(!Enum.TryParse(enumVal, true, out T retVal)) {
				return defaultValue;
			}
			return retVal;
		}

		public static List<T> GetAll<T>() where T : struct,Enum {
			return new List<T>((T[])Enum.GetValues(typeof(T)));
		}
	}

}
