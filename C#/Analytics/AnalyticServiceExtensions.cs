using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchoolResultSystem.Web.Areas.Analytics.Services;
using SchoolResultSystem.Web.Areas.Attendence.Services;
namespace SchoolResultSystem.Web.Areas.Analytics
{
    public static class AnalyticServiceExtensions
    {
        public static IServiceCollection AnalyticService(this IServiceCollection services)
        {
            services.AddScoped<CallMaper>();
            services.AddScoped<ApiRouter>();
            services.AddScoped<DisplayAttendence>();
            services.AddScoped<FindGrade>();
            services.AddScoped<ClassReport>();
            return services;
        }
    }
}