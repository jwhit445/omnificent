using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Core.Omni.API.Models;

namespace Core.Omni.API {
	public interface IOmniAPI {

		///<summary>The auth token used for API calls.</summary>
		string AuthToken { get; set; }

		///<summary>Upserts the user info and returns an authentication token if successful.</summary>
		Task<UpsertUserResponse> UpsertUser(UpsertUserRequest request);

		///<summary>Uploads the state of the current user. Must be authenticated.</summary>
		Task UploadUserInfo(UploadUserInfoRequest request);

		///<summary>Returns the URL needed to upload files to S3.</summary>
		Task<GetS3UrlResponse> GetS3UrlForLogs();

		///<summary>Returns logs for the given users.</summary>
		Task<GetLogsForUserResponse> GetLogsForUsers(GetLogsForUserRequest request);

		///<summary>Returns statuses for the given users.</summary>
		Task<GetUserStatusesResponse> GetUserStatuses(GetUserStatusesRequest request);

	}
}
