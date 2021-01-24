using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Omni.API.Models {
	public class GetLogsForUserResponse {

		///<summary>A dictionary of usernames to a list of their most recent log events.</summary>
		public Dictionary<string, List<LogEvent>> RecentUserEvents { get; set; }

	}
}
