using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Omni.API.Models {
	public class GetUserStatusesRequest {

		///<summary>The user for this request. Uses epic id if both username and epic id are provided.</summary>
		public User User { get; set; }

	}
}
