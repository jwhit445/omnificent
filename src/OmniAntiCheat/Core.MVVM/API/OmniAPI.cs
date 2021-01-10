using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Core.Omni.API.Models;
using Newtonsoft.Json;

namespace Core.Omni.API {

	public class OmniAPI : IOmniAPI {

		private const string API_BASE_URL = "https://lcgqxx62z0.execute-api.us-east-2.amazonaws.com/alpha/";

		private HttpClient _client { get; } = new HttpClient();

		public string AuthToken { get; set; }

		public string EpicID { get; set; }

		public async Task<UpsertUserResponse> UpsertUser(UpsertUserRequest request) {
			return await RunApiCall<UpsertUserResponse, UpsertUserRequest>($"{API_BASE_URL}user", HttpMethod.PUT, request);
		}

		public async Task UploadUserInfo(UploadUserInfoRequest request) {
			await RunApiCall<string, UploadUserInfoRequest>($"{API_BASE_URL}user/info", HttpMethod.PUT, request);
		}

		public async Task<GetS3UrlResponse> GetS3UrlForLogs() {
			return await RunApiCall<GetS3UrlResponse, string>($"{API_BASE_URL}logevent/url", HttpMethod.GET);
		}

		public async Task<GetLogsForUserResponse> GetLogsForUsers(GetLogsForUserRequest request) {
			return await RunApiCall<GetLogsForUserResponse, GetLogsForUserRequest>($"{API_BASE_URL}logevent/getmany", HttpMethod.POST, request);
		}

		public async Task<GetUserStatusesResponse> GetUserStatuses(GetUserStatusesRequest request) {
			return await RunApiCall<GetUserStatusesResponse, GetUserStatusesRequest>($"{API_BASE_URL}user/statuses", HttpMethod.POST, request);
		}

		private async Task<T> RunApiCall<T, R>(string url, HttpMethod method, R body = null) where R: class {
			_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{EpicID}:{AuthToken}")));
			HttpResponseMessage responseMessage = null;
			StringContent content = body != null ? new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json") : null;
			switch(method) {
				case HttpMethod.GET:
					responseMessage = await _client.GetAsync(url);
					break;
				case HttpMethod.POST:
					responseMessage = await _client.PostAsync(url, content);
					break;
				case HttpMethod.PUT:
					responseMessage = await _client.PutAsync(url, content);
					break;
				case HttpMethod.DELETE:
					responseMessage = await _client.DeleteAsync(url);
					break;
			}
			string bodyResponse = "";
			await ExUtils.SwallowAnyExceptionAsync(async () => {
				bodyResponse = await responseMessage.Content.ReadAsStringAsync();
			});
			if(!responseMessage.IsSuccessStatusCode) {
				throw new ApplicationException($"Error making API Call.\n{(int)responseMessage.StatusCode} - {responseMessage.StatusCode}\n{bodyResponse}");
			}
			return JsonConvert.DeserializeObject<T>(bodyResponse);
		}

		private enum HttpMethod {
			GET,
			POST,
			PUT,
			DELETE,
		}


	}
}
