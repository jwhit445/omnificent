using Newtonsoft.Json.Converters;
using System;

namespace Core.Omni.API.Models {

	public class UploadUserInfoRequest {

		///<summary>Indicates if moss anti-cheat is running.</summary>
		public bool IsMossRunning { get; set; }

		///<summary>Indicates if the tracked game process is running.</summary>
		public bool IsGameRunning { get; set; }

		[Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
		///<summary>Indicates which game is running.</summary>
		public GameType GameType { get; set; }

	}

	public enum GameType {
		Unknown,
		RogueCompany,
	}

}
