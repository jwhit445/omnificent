using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Omni.API.Models {
	public class GetLogsForUserResponse {

		///<summary>A dictionary of users to a list of their most recent log events.</summary>
		public Dictionary<User, List<LogEvent>> RecentUserEvents { get; set; }

	}
}
