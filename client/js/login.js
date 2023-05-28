var jwt = localStorage.getItem("jwt");
if (jwt != null) {
  window.location.href = './profile.html'
}

const base_url = 'https://localhost:7205/api';

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
          console.log(data.entity.token);
          localStorage.setItem('jwt', data.entity.token);
          localStorage.setItem('userid', data.entity.userId);
            window.location = "profile.html"
        }
        else{
            alert(data.message)
        }
    });
  return false;
}

function LoginPage() {
  const urlParams = new URLSearchParams(window.location.search);
  console.log(urlParams);
  const email = urlParams.get('email');
  const inputField = document.getElementById('username');
  inputField.value = email;
}

LoginPage()