using System;

namespace Core.Omni.API.Models {

	public class UploadUserInfoRequest {

		///<summary>Indicates if moss anti-cheat is running.</summary>
		public bool IsMossRunning { get; set; }

		///<summary>Indicates if the rougue company process is running.</summary>
		public bool IsRogueCompanyRunning { get; set; }

	}

}
