from flask import Flask, render_template, request, url_for, redirect, jsonify
import os
import sys
import signal
import webbrowser
from threading import Timer
from NEB.machine import main_engine

maneger = main_engine.Manager()


app = Flask(__name__)

@app.route("/")
def home():
    return  render_template("index.html")


from flask import request, jsonify


@app.route("/gradesheet", methods=["POST"])
def gradesheet():
    data = request.get_json()

    # 1. Safety check: Did the frontend actually send data?
    if not data or 'symbol' not in data:
        return jsonify({"error": "No symbol provided"}), 400

    symbol = data.get('symbol')
    print(symbol)

    try:
        # 2. Attempt to create the grade sheet
        student_sheet = maneger.create_grade(symbol)

        # 3. Check if the manager actually found the student
        if not student_sheet:
            return jsonify({"error": "Student not found"}), 404

        return jsonify(student_sheet)

    except Exception as e:
        # 4. Handle unexpected code errors (like Excel file missing)
        print(f"Error: {e}")
        return jsonify({"error": "Internal server error"}), 500




@app.route("/upload", methods=["POST"])
def upload_file():
    # 1. FIND THE REAL LOCATION OF THE EXE
    if getattr(sys, 'frozen', False):
        # We are running as an .exe
        base_dir = os.path.dirname(sys.executable)
        base_dirr = os.path.join(base_dir,"_internal/NEB")
    else:
        # We are running as a normal .py script
        base_dirr = os.path.dirname(os.path.abspath(__file__))


    UPLOAD_FOLDER = os.path.join(base_dirr, 'excel_data')

    if not os.path.exists(UPLOAD_FOLDER):
        os.makedirs(UPLOAD_FOLDER)

    if 'excel_file' not in request.files:
        return jsonify({"error": "No file part"}), 400

    file = request.files['excel_file']

    if file.filename == '':
        return jsonify({"error": "No selected file"}), 400

    if file:
        file_path = os.path.join(UPLOAD_FOLDER, "current_data.xlsm")
        file.save(file_path)

        # 3. VERIFY BEFORE SENDING SUCCESS
        if os.path.exists(file_path):
            maneger.reload()
            return jsonify({"message": "File uploaded and saved!"}), 200
        else:
            return jsonify({"error": "Failed to write file to disk"}), 500
    return jsonify({"error": "Unknown error occurred"})


@app.route('/shutdown', methods=['GET'])
def shutdown():
    # This kills the process running the server
    os.kill(os.getpid(), signal.SIGTERM)
    return "Server shut down. You can close this tab."

def open_browser():
    webbrowser.open_new("http://127.0.0.1:5000")


if __name__ == '__main__':
    # Wait 1.5 seconds to give the Flask server time to start
    Timer(1.5, open_browser).start()

    # Run the app (ensure debug is False for the .exe version)
    app.run(host='127.0.0.1', port=5000, debug=False)
    #app.run(debug=True)
