using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SchoolResultSystem.Web.Areas.Analytics.Models;
using SchoolResultSystem.Web.Areas.Students.Models;
using SchoolResultSystem.Web.Data;
using SchoolResultSystem.Web.Models;

namespace SchoolResultSystem.Web.Areas.Analytics.Services
{
    public class FindGrade(SchoolDbContext db)
    {
        // -------------------------------------------------------------------------
        // Public entry point
        // -------------------------------------------------------------------------

        public async Task<ApiResponse> Grade(JsonElement payload)
        {
            var dto = payload.Deserialize<MarksheetRequest>();

            if (dto is null)                         return ApiResponse.Fail("Invalid request payload.");
            if (string.IsNullOrWhiteSpace(dto.NSN))  return ApiResponse.Fail("Student NSN is required.");
            if (dto.ClassId <= 0)                    return ApiResponse.Fail("Valid ClassId is required.");
            if (dto.ExamId  <= 0)                    return ApiResponse.Fail("Valid ExamId is required.");

            // 1. Student info
            var student = await db.ClassStudent
                .Where(s => s.NSN == dto.NSN && s.ClassId == dto.ClassId)
                .Include(s => s.Student)
                .Include(s => s.Class)
                .Select(s => new StudentInfo
                {
                    Name         = s.Student.StudentName,
                    ClassName    = s.Class.ClassGrade,
                    DOB          = s.Student.D_O_B,
                    Registration = s.Student.RegistrationN,
                    NSN = dto.NSN
                })
                .FirstOrDefaultAsync();

            if (student is null)
                return ApiResponse.Fail($"No student found with NSN '{dto.NSN}' in class {dto.ClassId}.");

            // 2. Subject codes for this class
            var subjectCodes = await db.CST
                .Where(s => s.ClassId == dto.ClassId)
                .Select(s => s.SCode)
                .ToListAsync();

            if (!subjectCodes.Any())
                return ApiResponse.Fail("No subjects found for this class.");

            // 3. Exam rubric — subjects in this exam with full marks and credit hours
            //    Both theory and practical are loaded here.
            //    Theory rows have LinkedPr pointing to their practical SCode.
            var rubric = await db.ExamRubrick
                .Where(r => r.ExamId == dto.ExamId && subjectCodes.Contains(r.SCode))
                .Select(r => new
                {
                    r.SCode,
                    SName      = r.Subs.SName,
                    SType      = r.Subs.SType,      // "theory" or "practical"
                    LinkedPr   = r.Subs.LinkedPr,   // practical SCode linked to this theory, or null
                    r.FullMark,
                    r.CreditHour
                })
                .ToListAsync();

            if (!rubric.Any())
                return ApiResponse.Fail("No exam rubric found for this exam.");

            // 4. Student's obtained marks (all subjects — theory + practical)
            var marks = await db.Marksheet
                .Where(m => m.ExamId == dto.ExamId && subjectCodes.Contains(m.SCode))
                .Select(m => new { m.SCode, m.Mark })
                .ToListAsync();

            // Index both collections for O(1) lookup inside the loop
            var rubricIndex = rubric.ToDictionary(r => r.SCode);
            var marksIndex  = marks.ToDictionary(m => m.SCode, m => m.Mark);

            // -------------------------------------------------------------------------
            // Grade calculation
            // Loop only theory subjects — practical is pulled from index when needed
            // -------------------------------------------------------------------------

            var subjectGrades   = new List<SubjectGradeDto>();
            decimal totalWeight = 0m;
            decimal totalCredits= 0m;

            foreach (var theory in rubric.Where(r => r.SType == "theory"))
            {
                // --- Theory grade ---
                bool    theoryAbsent   = !marksIndex.ContainsKey(theory.SCode);
                decimal theoryObtained = theoryAbsent ? 0m : marksIndex[theory.SCode];
                decimal theoryFullSafe = theory.FullMark == 0 ? 1 : theory.FullMark;

                var (theoryGrade, theoryPoint) = (theoryAbsent || theoryObtained <= 0)
                    ? ("AB", 0.0m)
                    : AssignGrade(theoryObtained / theoryFullSafe * 100);

                // --- Practical grade (only if theory has a linked practical) ---
                string? prCode     = null;
                string? prName     = null;
                decimal prObtained = 0m;
                decimal prFull     = 0m;
                decimal prCredit   = 0m;
                string  prGrade    = "N/A";
                decimal prPoint    = 0m;

                if (!string.IsNullOrWhiteSpace(theory.LinkedPr) &&
                    rubricIndex.TryGetValue(theory.LinkedPr, out var practical))
                {
                    prCode     = practical.SCode;
                    prName     = practical.SName;
                    prFull     = practical.FullMark;
                    prCredit   = practical.CreditHour;

                    bool prAbsent = !marksIndex.ContainsKey(practical.SCode);
                    prObtained    = prAbsent ? 0m : marksIndex[practical.SCode];

                    decimal prFullSafe = prFull == 0 ? 1 : prFull;
                    (prGrade, prPoint) = (prAbsent || prObtained <= 0)
                        ? ("AB", 0.0m)
                        : AssignGrade(prObtained / prFullSafe * 100);
                }

                // --- Combined subject GPA ---
                //     weight = sum of (credit × grade point) for theory + practical
                //     subject GPA = weight / total credits
                decimal subjectCredit = theory.CreditHour + prCredit;
                decimal subjectWeight = (theory.CreditHour * theoryPoint) + (prCredit * prPoint);

                totalCredits += subjectCredit;
                totalWeight  += subjectWeight;

                decimal subjectGradePoint  = subjectCredit == 0 ? 0 : subjectWeight / subjectCredit;
                var (combinedGrade, _)     = AssignGrade(subjectGradePoint * 25);

                subjectGrades.Add(new SubjectGradeDto
                {
                    TheoryCode     = theory.SCode,
                    TheoryName     = theory.SName,
                    TheoryObtained = theoryObtained,
                    TheoryFull     = theory.FullMark,
                    TheoryCredit   = theory.CreditHour,
                    TheoryGrade    = theoryGrade,
                    TheoryPoint    = theoryPoint,

                    PracticalCode     = prCode,
                    PracticalName     = prName,
                    PracticalObtained = prObtained,
                    PracticalFull     = prFull,
                    PracticalCredit   = prCredit,
                    PracticalGrade    = prGrade,
                    PracticalPoint    = prPoint,

                    CombinedGrade = combinedGrade,
                    GradePoint    = Math.Round(subjectGradePoint, 2)
                });
            }

            // --- Final GPA across all subjects ---
            decimal finalGradePoint = totalCredits == 0 ? 0 : totalWeight / totalCredits;
            var (finalGrade, _)     = AssignGrade(finalGradePoint * 25);

            var exam = await db.ExamList.Where(e=>e.ExamId==dto.ExamId).FirstOrDefaultAsync();
            var school = await db.SchoolInfo.FirstOrDefaultAsync();

            return ApiResponse.Ok(new GradeReportDto
            {
                Student    = student,
                Exam     = exam!,
                Subjects   = subjectGrades,
                FinalGrade = finalGrade,
                GradePoint = Math.Round(finalGradePoint, 2),
                School=school!
            });
        }

        // -------------------------------------------------------------------------
        // Grade table — percentage → letter grade + grade point
        // -------------------------------------------------------------------------

        private static (string Grade, decimal Point) AssignGrade(decimal percentage)
        {
            return percentage switch
            {
                >= 90 => ("A+", 4.0m),
                >= 80 => ("A",  3.6m),
                >= 70 => ("B+", 3.2m),
                >= 60 => ("B",  2.8m),
                >= 50 => ("C+", 2.4m),
                >= 40 => ("C",  2.0m),
                >= 35 => ("D",  1.6m),
                _     => ("NG", 0.0m)
            };
        }
    }

}