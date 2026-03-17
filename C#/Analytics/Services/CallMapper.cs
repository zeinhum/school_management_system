using SchoolResultSystem.Web.Areas.Attendence.Services;
using SchoolResultSystem.Web.Areas.Analytics.Models;
using System.Text.Json;
namespace SchoolResultSystem.Web.Areas.Analytics.Services
{
    
    public class CallMaper
    {
        public Dictionary<string, Func<JsonElement, Task<ApiResponse>>> map { get; }

        public CallMaper(
            DisplayAttendence attendence,
            FindGrade grade

        )
        {
            map = new Dictionary<string, Func<JsonElement, Task<ApiResponse>>>(StringComparer.OrdinalIgnoreCase)
            {
                ["AttendenceReport"] = attendence.ExtractAttendenceData,
                ["FindGrade"] = grade.Grade,
            };

        }
    }
}