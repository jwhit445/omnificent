using System;

namespace Core.Omni.API.Models {
	public class LogEvent {

		///<summary>The date/time of the log in UTC.</summary>
		public DateTime StartDateTime { get; set; }

		///<summary>The date/time of the log in UTC.</summary>
		public DateTime EndDateTime { get; set; }

		///<summary>The link to the log.</summary>
		public string DownloadLink { get; set; }

	}
}
