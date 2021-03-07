using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Core.Omni.API.Models;

namespace Core.Omni.API {
	public interface IOmniAPI {

		///<summary>The auth token used for API calls.</summary>
		string AuthToken { get; set; }

		///<summary>The epic ID used for API calls.</summary>
		string EpicID { get; set; }
		///<summary>The Platform used for API calls.</summary>
		PlatformCode PlatformCode { get; set; }

		///<summary>Upserts the user info and returns an authentication token if successful.</summary>
		Task<UpsertUserResponse> UpsertUser(UpsertUserRequest request);

		Task<GetLatestAntiCheatVersionResult> GetLatestAntiCheatVersion();

		///<summary>Uploads the state of the current user. Must be authenticated.</summary>
		Task UploadUserInfo(UploadUserInfoRequest request);

		///<summary>Creates a log event for the given information.</summary>
		Task CreateLogEvent(CreateLogEventRequest request);

		///<summary>Reports the given user for suspected cheating.</summary>
		Task ReportUser(ReportUserRequest request);

		///<summary>Returns logs for the given users.</summary>
		Task<GetLogsForUserResponse> GetLogsForUsers(GetLogsForUserRequest request);

		///<summary>Returns statuses for the given users.</summary>
		Task<GetUserStatusesResponse> GetUserStatuses(GetUserStatusesRequest request);

	}
}
