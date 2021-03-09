using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Omni.API.Models {
	public class UpsertUserResponse {

		///<summary>A token used to authenticate the user with for future requests.</summary>
		public string AuthorizationToken { get; set; }

	}
}
