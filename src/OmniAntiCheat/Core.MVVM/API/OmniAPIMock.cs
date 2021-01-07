using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Core.Omni.API.Models;

namespace Core.Omni.API {
	public class OmniAPIMock : IOmniAPI {
		public string AuthToken {
			get {
				throw new NotImplementedException();
			}

			set {
				throw new NotImplementedException();
			}
		}

		public Task<GetLogsForUserResponse> GetLogsForUsers(GetLogsForUserRequest request) {
			throw new NotImplementedException();
		}

		public Task<GetS3UrlResponse> GetS3UrlForLogs() {
			throw new NotImplementedException();
		}

		public Task<GetUserStatusesResponse> GetUserStatuses(GetUserStatusesRequest request) {
			throw new NotImplementedException();
		}

		public Task UploadUserInfo(UploadUserInfoRequest request) {
			throw new NotImplementedException();
		}

		public Task<UpsertUserResponse> UpsertUser(UpsertUserRequest request) {
			throw new NotImplementedException();
		}
	}
}
