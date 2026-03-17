import json
import os


def load_subject_data():
    """
    Reads the subject_data.json file and returns it as a Python dictionary.
    """
    # 1. Get the directory where THIS script is located
    base_dir = os.path.dirname(os.path.abspath(__file__))

    # 2. Construct the path to the json file
    # If the JSON is in the root 'NEB' folder:
    file_path = os.path.join(base_dir, "subref.json")

    try:
        with open(file_path, 'r') as file:
            data = json.load(file)
            return data
    except FileNotFoundError:
        print(f"Error: The file was not found at {file_path}")
        return {}
    except json.JSONDecodeError:
        print("Error: The JSON file is corrupted or formatted incorrectly.")
        return {}



if __name__=="__main__":

    # Usage
    subjects = load_subject_data()
    print(subjects)
