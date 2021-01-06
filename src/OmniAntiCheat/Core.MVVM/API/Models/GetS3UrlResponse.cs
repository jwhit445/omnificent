using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Omni.API.Models {
	public class GetS3UrlResponse {

		///<summary>The signed S3 url that allows the user to upload their logs.</summary>
		public string SignedS3Url { get; set; }

	}
}
