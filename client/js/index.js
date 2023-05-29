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
        if (data.succeeded) {
            alert(data.message);
            const queryString = `?email=${email}`
            window.location.href = `otp.html${queryString}`;
        }
        else{
            alert(data.message);
            const queryString = `?email=${email}`
            window.location.href = `otp.html${queryString}`;
        }
    });
return false;
}

