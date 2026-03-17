class GradeCalculator:

    def __init__(self):
        self.GRADING_SCALE = [
            (90, "A+", 4.0),
            (80, "A", 3.6),
            (70, "B+", 3.2),
            (60, "B", 2.8),
            (50, "C+", 2.4),
            (40, "C", 2.0),
            (35, "D", 1.6),
            (0, "NG", 0.0)
        ]


    def assign_grade(self, mark, fullmark):
        
        if fullmark=="point":
            for threshold, letter, gp in self.GRADING_SCALE:
                if mark >= gp:
                    return letter

        if mark==0:
            return "AB", 0

        percentage = (mark / fullmark) * 100
        for threshold, letter, gp in self.GRADING_SCALE:
            if percentage >= threshold:
                return letter, gp

        return "NG", 0

    def subject_gpa(self,obtained_marks, full_marks):
        #theory garde*credit_hour + practical grade x practical credit / subject total credit hour
        graded_subjects = []
        for subject in obtained_marks:
            current_sub = subject["subject"]
            if subject != "NaN":
                crt =full_marks[current_sub]["theory crh"]
                crp=full_marks[current_sub]["practical crh"]
                th_letter, th_point = self.assign_grade(subject["Th"],full_marks[current_sub]["Th"])
                pr_letter, pr_point = self.assign_grade(subject["Pr"], full_marks[current_sub]["Pr"])
                obtained_point =th_point*crt + pr_point*crp
                subject_credit =crp +crt
                final_point = round(obtained_point/subject_credit,2)

                final_letter = self.assign_grade(final_point,"point")

                subject_grade = {"subject":subject["subject"],"code":subject["code"],"crt":crt, "crp":crp,
                                 "Th":(th_letter,th_point), "Pr":(pr_letter,pr_point),"final":(final_letter,final_point)}
                graded_subjects.append(subject_grade)
        return graded_subjects


    def student_gpa(self, obtained_marks, full_marks):
        student_gpas =0
        subjects_crh=0
        overall_grade=[]
        grades = self.subject_gpa(obtained_marks, full_marks)
        for subs in grades:
            cr_sub = subs["subject"]
            sub_cr = full_marks[cr_sub]["theory crh"]+full_marks[cr_sub]["practical crh"]
            student_gpas +=subs["Th"][1]*full_marks[cr_sub]["theory crh"] + subs["Pr"][1]*full_marks[cr_sub]["practical crh"]
            subjects_crh += sub_cr

        student_gpa = student_gpas/subjects_crh
        student_letter = self.assign_grade(student_gpa,"point")
        overall_grade.append(student_letter)
        overall_grade.append(round(student_gpa,2))
        return grades , overall_grade

if __name__=="__main__":
    obtained_marks = [
        {"subject": "Science", "Th": 40, "Pr": 14},
        {"subject": "Math", "Th": 30, "Pr": 15},
        {"subject": "Social", "Th": 0, "Pr": 0}
    ]
    full_marks = {
        "Science": {"Th": 40, "Pr": 20, "theory crh": 3, "practical crh": 1.5},
        "Math": {"Th": 40, "Pr": 20, "theory crh": 3, "practical crh": 2},
        "Social": {"Th": 40, "Pr": 20, "theory crh": 3, "practical crh": 2}
    }
    graded_subjects = [
        {"subject": "science", "Th": ("A", 3.8), "Pr": ("B", 2.6), "final": ("B", 2.6)},
    ]

    grader = GradeCalculator()
    result=grader.student_gpa(obtained_marks, full_marks)
    print(result)