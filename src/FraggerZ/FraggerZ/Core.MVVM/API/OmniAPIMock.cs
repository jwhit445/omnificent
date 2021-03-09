using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Omni.API.Models;

namespace Core.Omni.API {
	public class OmniAPIMock : IOmniAPI {

		public string AuthToken { get; set; }

		public string EpicID { get; set; }
		public PlatformCode PlatformCode { get; set; }

		public Task CreateLogEvent(CreateLogEventRequest request) {
			throw new NotImplementedException();
		}

		public Task<GetLogsForUserResponse> GetLogsForUsers(GetLogsForUserRequest request) {
			throw new NotImplementedException();
		}

		public Task<GetUserStatusesResponse> GetUserStatuses(GetUserStatusesRequest request) {
			throw new NotImplementedException();
		}

        public Task ReportUser(ReportUserRequest request) {
			return Task.CompletedTask;
		}

		public Task<GetLatestAntiCheatVersionResult> GetLatestAntiCheatVersion() {
			return Task.FromResult(new GetLatestAntiCheatVersionResult {
				Version = new Version(1, 0, 0, 0)
			});
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
