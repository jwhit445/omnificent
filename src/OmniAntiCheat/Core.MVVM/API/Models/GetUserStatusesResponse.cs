using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Omni.API.Models {
	public class GetUserStatusesResponse {

		///<summary>A dictionary of EpicIDs to their statuses.</summary>
		public Dictionary<string, UserStatus> UserStatuses { get; set; }

	}

	public class UserStatus {

		///<summary>The last heartbeat of the user in UTC.</summary>
		public DateTime LastHeartbeat { get; set; }

		///<summary>Indicates if moss anti-cheat is running.</summary>
		public bool IsMossRunning { get; set; }

		///<summary>Indicates if a tracked game process is running.</summary>
		public bool IsGameRunning { get; set; }

		///<summary>Indicates which game is running.</summary>
		public GameType GameType { get; set; }

		///<summary>Indicates if the status of the user in regards to the anti cheat.</summary>
		public ReportStatus ReportStatus { get; set; }

	}

public enum ReportStatus {
		NotReported,
		Pending,
		Cleared,
		Confirmed,
	}
}
