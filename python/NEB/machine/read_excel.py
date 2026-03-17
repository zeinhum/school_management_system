import pandas as pd
import os


class ReadExcel:
    def __init__(self, file_name):
        current_dir = os.path.dirname(os.path.abspath(__file__))
        parent_dir = os.path.dirname(current_dir)
        self.file_path = os.path.join(parent_dir, file_name)

    def student_data(self, symbolnumber):
        try:
            # 1. Load the specific sheet directly
            df = pd.read_excel(self.file_path, sheet_name="DATA ENTRY", engine="openpyxl")

            # 2. Filter the dataframe by Symbol Number
            # Note: Ensure the column name matches exactly what is in Excel (e.g., "SYMBOL NO.")
            match = df[df["SYMBOL NO."] == symbolnumber]

            # 3. Handle "Out of Bounds" by checking if match is empty
            if match.empty:
                print(f"Symbol {symbolnumber} not found.")
                return None

            # 4. Return the first matching row as a dictionary
            # This is where the indexer used to fail; now we know it exists.
            return match.iloc[0].to_dict()

        except Exception as e:
            print(f"Error reading Excel: {e}")
            return None




if __name__=="__main__":
    reader = ReadExcel("./excel_data/current_data.xlsm")
    read = reader.student_data(14911507)
    print(read)
