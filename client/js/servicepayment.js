var jwt = localStorage.getItem("jwt");
if (jwt != null) {
  window.location.href = './index.html'
}

const base_url = 'https://localhost:7205/api';

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


function ServiceChargePage() {
    const urlParams = new URLSearchParams(window.location.search);
    console.log(urlParams);
    const email = urlParams.get('email');
    const inputField = document.getElementById('username');
    inputField.value = email;
}

ServiceChargePage()