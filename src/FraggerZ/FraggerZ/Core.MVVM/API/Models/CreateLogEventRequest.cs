using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Omni.API.Models {
	public class CreateLogEventRequest {

		public DateTime DateTimeStarted { get; set; }

		public string S3Url { get; set; }

	}
}
