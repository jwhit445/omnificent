using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Omni.MVVM {

	///<summary>This attribute can be used to link a bindable property using our GetBindableProperty/SetBindableProperty to other properties. When the property this attribute
	///is on changes, all properties attached will fire their on property changed event as well. Does not work on overridden virtual properties.</summary>
	public class BindablePropertyAttribute : Attribute {

		///<summary>A list of names that are dependent on a bindable property. These can be used to fire all property changed events
		///when one changed.</summary>
		public List<string> ListDependentPropertyNames = new List<string>();

		public BindablePropertyAttribute(params string[] arrayDependentProperties) {
			ListDependentPropertyNames = arrayDependentProperties.ToList();
		}

	}

}
