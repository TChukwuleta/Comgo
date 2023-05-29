document.addEventListener("DOMContentLoaded", function(event) {

    function OTPInput() {
    const inputs = document.querySelectorAll('#otp > *[id]');
    for (let i = 0; i < inputs.length; i++) 
    { 
        inputs[i].addEventListener('keydown', function(event) { if (event.key==="Backspace" ) { inputs[i].value='' ; if (i !==0) inputs[i - 1].focus(); } else { if (i===inputs.length - 1 && inputs[i].value !=='' ) { return true; } else if (event.keyCode> 47 && event.keyCode < 58) { inputs[i].value=event.key; if (i !==inputs.length - 1) inputs[i + 1].focus(); event.preventDefault(); } else if (event.keyCode> 64 && event.keyCode < 91) { inputs[i].value=String.fromCharCode(event.keyCode); if (i !==inputs.length - 1) inputs[i + 1].focus(); event.preventDefault(); } } }); } } OTPInput(); 
    });


const base_url = 'https://localhost:7205/api';
const userId = localStorage.getItem('userid');
const token = localStorage.getItem('jwt');

function ValidateUser(){
    let otp = document.getElementById("first").value + document.getElementById("second").value + document.getElementById("third").value + document.getElementById("fourth").value + document.getElementById("fifth").value + document.getElementById("sixth").value;
    const email = document.getElementById("otp_email").innerHTML;
    const myBody = {
        "email": email,
        "otp": otp
    }
    console.log(JSON.stringify(myBody))
    fetch(`${base_url}/Auth/emailverification`, {
    method: 'POST',
    redirect: 'manual',
    body: JSON.stringify(myBody),
    headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json'
        }
    })
    .then(res => res.json())
    .then((data) => {
        console.log(data)
        if (data.succeeded) {
            alert(data.message);
            const queryString = `?email=${email}`
            window.location.href = `servicecharge.html${queryString}`;
        }
        else{
            alert(data.message);
        }
    });
    return false;
}


function ConfirmPayment(){
    let otp = document.getElementById("first").value + document.getElementById("second").value + document.getElementById("third").value + document.getElementById("fourth").value + document.getElementById("fifth").value + document.getElementById("sixth").value;
    const reference = document.getElementById("reference").value;
    const myBody = {
        "userId": userId,
        "otp": otp,
        "reference": reference
    }
    console.log(JSON.stringify(myBody))
    fetch(`${base_url}/User/validatepaymentrequest`, {
    method: 'POST',
    redirect: 'manual',
    body: JSON.stringify(myBody),
    headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
        'Accept': 'application/json'
        }
    })
    .then(res => res.json())
    .then((data) => {
        console.log(data)
        if (data.succeeded) {
            alert(data.message);
        }
        else{
            alert(data.message);
        }
    });
    return false;
}




function OTPPage() {
    const urlParams = new URLSearchParams(window.location.search);
    console.log(urlParams);
    const email = urlParams.get('email');
    const inputField = document.getElementById('otp_email');
    inputField.innerHTML = email;
}

OTPPage()