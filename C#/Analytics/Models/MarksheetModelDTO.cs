using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchoolResultSystem.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace SchoolResultSystem.Web.Areas.Analytics.Models
{
    public class StdReq
    {
        public string NSN { get; set; } = null!;
        public int exYear { get; set; }
        public string exName { get; set; } = null!;
    }


    public class Req
    {
        public string ClassName { get; set; } = null!;
        public int ExamId { get; set; }
    }

    public class MarkSheetDto
    {
        public string Schoolname { get; set; } = null!;
        public string SchoolAddress { get; set; } = null!;
        public string ClassGrade { get; set; } = null!;
        public ExamModel Exam { get; set; } = new ExamModel();
        public StudentModel Student { get; set; } = new StudentModel();
        public List<GPADto> gpas { get; set; } = new List<GPADto>();
        public (string gpaL, decimal gpa) GPA { get; set; }
    }

    public class ObtainedMarks
    {
        public SubjectModel TheorySub { get; set; } = new SubjectModel();

        public SubjectModel PracticalSub { get; set; } = new SubjectModel();

        public bool HasPr { get; set; } = false;

        public decimal TheoryMark { get; set; }

        public decimal PracticalMark { get; set; }
    }

    public class ExamSubjects
    {
        public string theorySName{get;set;}=null!;
        public string practicalSName{get;set;}=null!;
        public string theoryCode{get; set;}= null!;
        public string practicalCode{get; set;}= null!;
        public decimal theoryMark{get; set;}
        public decimal theoryCredit{get; set;}
        public decimal practicalMark{get; set;}
        public decimal practicalCredit{get; set;}
    }
    public class GPADto
    {
        public SubjectModel ThSub { get; set; } = new SubjectModel();//
        public SubjectModel PrSub { get; set; } = new SubjectModel();
        public (string Grade, decimal GradePoint, decimal credithr) ThGrade { get; set; }
        public (string Grade, decimal GradePoint, decimal credithr) PrGrade { get; set; }
        
        public (string TotalGrade, decimal TotalGradePoint) TotalGrade { get; set; }
    }
}
