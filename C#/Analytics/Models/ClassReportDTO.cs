using SchoolResultSystem.Web.Areas.Students.Models;
namespace SchoolResultSystem.Web.Areas.Analytics.Models
{
    public class ClassReportDto
    {
        public MarksheetRequest? Requested{get; set;}
        public List<GradeReportDto>? Report {get;set;}
    }
}
