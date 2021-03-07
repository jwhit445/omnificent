using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Omni.API.Models {
	public class User {
		//TODO: PlatFormCode is required, I think Luke did it but I can't pull latest code because internet is down.

		///<summary>The current epic username for this account/user.</summary>
		public string Username { get; set; }

		///<summary>The epic id for this account/user.</summary>
		public string ID { get; set; }

		[Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
		///<summary>The Platform that this user exists on. (i.e. Epic, Blizzard)</summary>
		public PlatformCode PlatformCode { get; set; }

	}

	public enum PlatformCode {
		Unknown,
		Epic,
		Blizzard,
    }
}
