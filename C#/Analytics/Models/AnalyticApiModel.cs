using System.Text.Json;
using SchoolResultSystem.Web.Areas.Attendence.Services;

namespace SchoolResultSystem.Web.Areas.Analytics.Models
{
    public class ApiRequest
    {

        public string ApiAction { get; set; } = null!;
        public bool JsonRequired {get; set;} = false;
        public JsonElement Payload { get; set; }
    }
    public class ApiResponse
    {
        public bool Success { get; set; }
        public object? Data { get; set; }
        public string? Error { get; set; }

        // Static helpers to create responses cleanly
        public static ApiResponse Ok(object data) => new() { Success = true, Data = data };
        public static ApiResponse Fail(string error) => new() { Success = false, Error = error };
    }

    // call maper

}