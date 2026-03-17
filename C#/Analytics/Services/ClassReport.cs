using SchoolResultSystem.Web.Areas.Analytics.Models;
using SchoolResultSystem.Web.Areas.Students.Models;
using SchoolResultSystem.Web.Areas.Analytics.Services;
using SchoolResultSystem.Web.Models;
using SchoolResultSystem.Web.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Text.Json;

namespace SchoolResultSystem.Web.Areas.Analytics.Services
{
    public class ClassReport(SchoolDbContext db, FindGrade findGrade)
    {


        public async Task<ApiResponse> GenerateReport(JsonElement data)
        {
            try
            {

                var req = data.Deserialize<MarksheetRequest>();

                if (req is null) return ApiResponse.Fail("Invalid request payload.");
                if (req.ClassId <= 0) return ApiResponse.Fail("Valid ClassId is required.");
                if (req.ExamId <= 0) return ApiResponse.Fail("Valid ExamId is required.");

                var extingReport = await db.AcademicReports.Where(r=>r.ClassId==req.ClassId && r.ExamId==req.ExamId).FirstOrDefaultAsync();
                if (extingReport is not null)
                {
                    var cachedResult = JsonSerializer.Deserialize<List<GradeReportDto>>(extingReport.ReportJson);
                    return ApiResponse.Ok(new ClassReportDto
                    {
                        Requested =req,
                        Report =cachedResult
                    });
                }

                // Get all active students in the class
                var nsns = await db.ClassStudent
                    .Where(s => s.ClassId == req.ClassId && s.IsActive)
                    .Select(s => s.NSN)
                    .ToListAsync();

                if (!nsns.Any())
                    return ApiResponse.Fail("No active students found in this class.");

                // Calculate grade for each student
                var results = new List<GradeReportDto>();

                foreach (var nsn in nsns)
                {
                    // Build a fresh JsonElement for each student
                    req.NSN = nsn;
                    var jsonElement = JsonSerializer.SerializeToElement(req); 

                    var grade = await findGrade.Grade(jsonElement);  

                    if (grade.Success)
                    {
                        if (grade.Data is GradeReportDto repor)  
                            results.Add(repor);
                    }
                        
                }

                var reportJson = JsonSerializer.Serialize(results, new JsonSerializerOptions
                {
                    WriteIndented = false
                });

                var report = new AcademicReports
                {
                    ClassId = req.ClassId,
                    ExamId = req.ExamId,
                    ReportJson = reportJson

                };

                await db.AcademicReports.AddAsync(report);
                await db.SaveChangesAsync();
                return ApiResponse.Ok(new ClassReportDto
                {
                    Requested = req,
                    Report = results
                });
            }
            catch
            {
                return ApiResponse.Fail("Error occured");
            }
        }
    }


}
