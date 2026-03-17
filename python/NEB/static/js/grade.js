
async function getData(symbol) {
    try {
        const res = await fetch("./gradesheet", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({ symbol: symbol })
        });

        // Check if the server actually sent back a "Success" status (200 OK)
        if (!res.ok) {
            alert("Invalid Symbol");
        }

        const data = await res.json();
        console.log(data)
        return data;

    } catch (error) {
        // This handles both network errors AND our custom "Invalid Symbol" error
        alert("Error! &#128514; \n\nPlease check if the symbol number is valid or if the server is running.");

    }
}


export async function populateSheet(symbol) {
    const data = await getData(symbol);
    const marksheetContainer = document.querySelector(".marksheet-container");
    let dateDisplay = document.querySelector(".date");
    dateDisplay = dateDisplay.value;
    if(!dateDisplay){

    // Fallback to English Date if library is missing or fails
    dateDisplay = new Date().toLocaleDateString('en-GB');
}




    // 3. Use .map().join('') instead of .forEach() inside template literals
    const gradeRows = data.grades.map(sub => `
        <tr>
            <td>${sub.code}</td>
            <td class="text-left">${sub.subject.replace(/opt/i, "").trim()} (TH)</td>
            <td>${sub.crt}</td>
            <td>${sub.Th[1]}</td>
            <td>${sub.Th[0]}</td>
            <td rowspan="2">${sub.final[0] || 'C'}</td>
            <td></td>
        </tr>
        <tr>
            <td>${(Number(sub.code) + 1).toString().padStart(4, '0')}</td>
            <td class="text-left">${sub.subject.replace(/opt/i, "").trim()} (PR)</td>
            <td>${sub.crp}</td>
            <td>${sub.Pr[1]}</td>
            <td>${sub.Pr[0]}</td>
            <td></td>
        </tr>
    `).join('');

    marksheetContainer.innerHTML = `
        <header>
            <div class="logo-area">
                <div class="school-name">
                    <h1>SHREE KARMAHAWA SECONDARY SCHOOL KARMAHAWA</h1>
                    <p>LUMBINI CULTURAL MUNICIPALITY-7, RUPANDEHI</p>
                </div>
            </div>
            <div class="grade-sheet-title">GRADE-SHEET</div>
            <div class="student-info">
                <div class="info-row">
                    <span>THE FOLLOWING ARE THE GRADES OBTAINED BY: <strong>${data.demography.name}</strong></span>
                </div>
                <div class="info-row">
                    <span>DATE OF BIRTH: <u>${data.demography.bs}</u> B.S &nbsp;&nbsp; <u>${data.demography.ad}</u> A.D</span>
                </div>
                <div class="info-row">
                    <span>REGISTRATION NUMBER: <u>${data.demography.registration}</u></span>
                    <span>SYMBOL NUMBER: <u>${data.demography.symbol}</u></span>
                    <span>GRADE: 11</span>
                </div>
            </div>
        </header>

        <table>
            <thead>
                <tr>
                    <th width="10%">SUBJECT CODE</th>
                    <th width="45%">SUBJECTS</th>
                    <th width="10%">CREDIT HOUR</th>
                    <th width="10%">GRADE POINT</th>
                    <th width="10%">GRADE</th>
                    <th width="10%">FINAL GRADE</th>
                    <th width="5%">REMARK</th>
                </tr>
            </thead>
            <tbody>
                ${gradeRows}
            </tbody>
            <tfoot>
                <tr>
                    <td colspan="3" class="text-right"><strong>GRADE POINT AVERAGE (GPA)  </strong></td>
                    <td colspan="4">${data.final_g[1]}</td>
                </tr>
            </tfoot>
        </table>

        <div class="footer-signatures">
            <div class="sig-box"><p>PREPARED BY:</p></div>
            <div class="sig-box"><p>CHECKED BY<br>CLASS TEACHER</p></div>
            <div class="sig-box"><p>HEADMASTER/CAMPUS CHIEF</p></div>
        </div>
        <div class="date-issued">
            DATE OF ISSUE: ${dateDisplay}
        </div>

        <div class="notes">
            <hr>
            <p>NOTE: ONE CREDIT HOUR IS 32 CLOCK HOURS</p>
            <div class="note-columns">
                <span>TH: THEORY</span>
                <span>PR: PRACTICAL</span>
                <span>AB: ABSENT</span>
                <span>W: WITHHELD</span>
            </div>
        </div>
    `;
}