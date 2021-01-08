using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Omni.API.Models;

namespace Core.Omni.API {
	public class OmniAPIMock : IOmniAPI {

		public string AuthToken { get; set; }

		public Task<GetLogsForUserResponse> GetLogsForUsers(GetLogsForUserRequest request) {
			return Task.FromResult(new GetLogsForUserResponse {
				RecentUserEvents = new Dictionary<User, List<LogEvent>> {
					{ new User() { }, new List<LogEvent> { } },
				},
			});
		}

		public Task<GetS3UrlResponse> GetS3UrlForLogs() {
			throw new NotImplementedException();
		}

		public Task<GetUserStatusesResponse> GetUserStatuses(GetUserStatusesRequest request) {
			throw new NotImplementedException();
		}

		public Task UploadUserInfo(UploadUserInfoRequest request) {
			return Task.CompletedTask;
		}

		public Task<UpsertUserResponse> UpsertUser(UpsertUserRequest request) {
			return Task.FromResult(new UpsertUserResponse {
				AuthorizationToken = "AuthToken",
			});
		}
	}
}
