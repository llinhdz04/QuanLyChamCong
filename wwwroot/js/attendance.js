document.getElementById("btnCheckIn").onclick = () => {
    fetch("/Attendance/CheckIn", { method: "POST" })
        .then(r => r.json())
        .then(d => alert(d.success ? "Check in OK" : d.message));
};

document.getElementById("btnCheckOut").onclick = () => {
    fetch("/Attendance/CheckOut", { method: "POST" })
        .then(r => r.json())
        .then(d => alert("Check out OK"));
};
