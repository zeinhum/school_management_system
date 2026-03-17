import {populateSheet} from "./grade.js"


const marksheetContainer = document.querySelector(".marksheet-container");
const searchButton = document.querySelector(".search");
const printButton  = document.querySelector(".print");

// 1. Define function as a standard function so it is hoisted correctly
async function searchFunction(e) {
    const symbolInput = document.querySelector(".symbol");
    const symbol = parseInt(symbolInput.value);


    if (!symbol) return alert("Please enter a symbol number");

    await populateSheet(symbol);
}

searchButton.addEventListener("click", searchFunction);

function printMarksheet() {
    // Optional: Add a title to the browser tab so the PDF file saves with a good name
    const studentName = document.querySelector(".student-info strong")?.innerText || "Student";
    const originalTitle = document.title;

    document.title = `Gradesheet_${studentName}`;
    window.print();
    document.title = originalTitle; // Restore title after print dialog closes
}
printButton.addEventListener("click",printMarksheet)


// upload excell
const uploadButton = document.querySelector(".upload");
const fileInput = document.querySelector('input[type="file"]');

uploadButton.addEventListener("click", async () => {
    const file = fileInput.files[0];

    // 1. Basic Validation
    if (!file) {
        alert("Please select an Excel file first!");
        return;
    }

    // 2. Prepare the data
    const formData = new FormData();
    formData.append("excel_file", file);

    try {
        // 3. Send to Backend
        const response = await fetch("/upload", {
            method: "POST",
            body: formData, // Do NOT set Content-Type header, browser does it automatically for FormData
        });

        if (response.ok) {
            //const result = await response.json();
            alert("File uploaded successfully! You can now search for students.");
            //console.log(result);
        } else {
            alert("Upload failed. Make sure the file structure is correct.");
        }
    } catch (error) {
        console.error("Error uploading file:", error);
        alert("An error occurred during upload.");
    }
});

