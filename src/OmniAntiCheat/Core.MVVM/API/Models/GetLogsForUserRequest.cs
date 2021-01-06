using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Omni.API.Models {
	public class GetLogsForUserRequest {

		///<summary>A list of usernames to retrieve the most recent logs for.</summary>
		public List<string> ListUsername { get; set; }

		///<summary>A list of epic ids to retrieve the most recent logs for.</summary>
		public List<string> ListEpicID { get; set; }

	}
}
