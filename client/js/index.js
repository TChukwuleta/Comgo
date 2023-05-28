var jwt = localStorage.getItem("jwt");
if (jwt != null) {
  window.location.href = './index.html'
}

const base_url = 'https://localhost:7205/api';

// User registration method
function register() {
    const username = document.getElementById("name").value;
    const email = document.getElementById("email").value;
  const password = document.getElementById("password").value;
    const myBody = {
        "email": email,
        "password": password,
        "name": username
    }
    fetch(`${base_url}/Auth/createuser`, {
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
            alert(data.message)
        }
    });
return false;
}

// User login method
function login() {
  const username = document.getElementById("username").value;
  const password = document.getElementById("password").value;
    const myBody = {
        "email": username,
        "password": password
    }
    fetch(`${base_url}/Auth/login`, {
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
            window.location = "profile.html"
        }
        else{
            alert(data.message)
        }
    });
return false;
}

function payForServiceCharge() {
    const email = document.getElementById("username").value;
    const servicetype = document.getElementById("payment_type").value;
    const myBody = {
        "email": email,
        "paymentModeType": parseInt(servicetype)
    }
    console.log(myBody);
    fetch(`${base_url}/Auth/servicepayment`, {
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
            switch (parseInt(servicetype)) {
                case 1:
                    alert(data.message)
                case 2:
                    break;
                case 3:
                    window.location = data.entity.authorization_url
                default:
                    break;
            }
        }
        else{
            alert(data.message)
        }
    });
    return false;
}