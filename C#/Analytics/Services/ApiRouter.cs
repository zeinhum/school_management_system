using System.Text.Json;
using SchoolResultSystem.Web.Areas.Analytics.Models;
using SchoolResultSystem.Web.Areas.Attendence.Services;

namespace SchoolResultSystem.Web.Areas.Analytics.Services
{
    public class ApiRouter(CallMaper mapper)
    {
        public async Task<ApiResponse> Map(ApiRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ApiAction))
            {
                return ApiResponse.Fail("ApiAction cannot be empty.");
            }
            // Look up the action in the dictionary
            if (!mapper.map.TryGetValue(request.ApiAction, out var handler))
                return ApiResponse.Fail($"Unknown action: '{request.ApiAction}'");

            // Call the matched handler, passing only the request
            return await handler(request.Payload);
        }
    }

    
}