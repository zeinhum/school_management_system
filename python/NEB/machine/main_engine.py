from datetime import datetime
import re

from NEB.machine import calculate_grade
from NEB.machine import read_excel
from NEB.machine import read_json
import pandas as pd


class Manager:
    def __init__(self):

        self.excel = read_excel.ReadExcel("./excel_data/current_data.xlsm")
        self.sub_ref = read_json.load_subject_data()
        #print(self.sub_ref)

        self.subject_mappings = [
            ("COPULSORY 1", "TH", "PR"),
            ("COMPULSORY 2", "TH.1", "PR.1"),
            ("COMPULSORY   3", "TH.2", "PR.2"),
            ("OPTIONAL 1", "TH.3", "PR.3"),
            ("OPTIONAL 2", "TH.4", "PR.4"),
            ("OPTIONAL 3", "TH.5", "PR.5"),
            ("OPTIONAL 4", "TH.6", "PR.6"),
            ]

    def reload(self):
        self.excel = read_excel.ReadExcel("./excel_data/current_data.xlsm")

    def _obtained_marks(self,symbol):
        student_data = self.excel.student_data(symbol)
        student_demography = {
            "symbol":int(student_data["SYMBOL NO."]),
            "registration":int(student_data["REGISTRATION NO."]),
            "name":student_data["NAME"],
            "bs":self.date_to_int(student_data["DOB B.S"]),
            "ad":self.date_to_int(student_data["DOB A.D"])
        }
        obtained = []

        for sub, th, pr in self.subject_mappings:
            subject_name = student_data.get(sub)

            if pd.notna(subject_name):
                raw_th = student_data.get(th, 0)
                raw_pr = student_data.get(pr, 0)

                clean_th = int(raw_th) if pd.notna(raw_th) else 0
                clean_pr = int(raw_pr) if pd.notna(raw_pr) else 0
                subj = str(subject_name).replace("(TH)", "").strip()

                obtained.append({
                    "subject": subj,
                    "Th": clean_th,
                    "Pr": clean_pr,
                    "code":self.sub_ref[subj]["code"]
                })
        return obtained, student_demography

    def create_grade(self,symbol):

        calculator = calculate_grade.GradeCalculator()
        _marks, demography = self._obtained_marks(symbol)
        #print(obtained_marks)
        #print(self.sub_ref)

        grades, final_g= calculator.student_gpa(_marks,self.sub_ref)
        student={
            "demography":demography,
            "grades":grades,
            "final_g":final_g
        }
        return student


    def date_to_int(self, date_val):

        if pd.isna(date_val):
            return 0

            # Convert everything to a string first to handle all types (Timestamp, Date, String)
            # Then strip out anything that isn't a number
        clean_str = re.sub(r'\D', '', str(date_val))

        # If the string contains a time (like 20640808000000), take only the first 8 digits (YYYYMMDD)
        if len(clean_str) > 8:
            clean_str = clean_str[:8]

        date_num = int(clean_str) if clean_str else 0
        date_str = str(date_num)

        year = date_str[:4]  # 2064
        month = date_str[4:6]  # 08
        day = date_str[6:]  # 08

        formatted_date = f"{year}-{month}-{day}"
        return formatted_date

if __name__=="__main__":
    manager = Manager()
    print(manager.create_grade(14911507))