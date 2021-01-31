using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Omni.API.Models {
	public class GetUserStatusesRequest {

		///<summary>The user for this request.</summary>
		public List<string> ListUsernames { get; set; }

	}
}
