const base_url = 'https://localhost:7205/api';
const userId = localStorage.getItem('userid');
const token = localStorage.getItem('jwt');

function loadIntoTable() {
    // Get UserById
    fetch(`${base_url}/User/getuserbyid/${userId}`, {
        headers:{
            'Authorization': `Bearer ${token}`
        }
    })
    .then(response => response.json())
    .then(data => {
        if (!data.succeeded) {
            alert(data.message);
        }
        // Full name
        const inputField = document.getElementById('fullName');
        inputField.value = data.entity.name;
        // Bio
        const bio = document.getElementById('bio');
        bio.value = data.entity.bio;
        // Location
        const location = document.getElementById('location');
        location.value = data.entity.location;
        // Email
        const userEmail = document.getElementById('user_email');
        userEmail.value = data.entity.email;
        // Descriptor
        const descriptor = document.getElementById('descriptor');
        descriptor.value = data.entity.descriptor;
    })
    .catch(error => {
    console.error('Error:', error);
    });

    // Get wallet balance
    fetch(`${base_url}/User/getwalletbalance/${userId}`, {
        headers:{
            'Authorization': `Bearer ${token}`
        }
    })
    .then(response => response.json())
    .then(data => {
        console.log(data)
        if (data.succeeded) {
            const walletBalance = document.getElementById('wallet_balance');
            walletBalance.innerHTML = data.entity;
        }
    })
    .catch(error => {
    console.error('Error:', error);
    });

    // Security question
    const securityQuestions = document.getElementById('security_questions');
    let defaultOption = new Option("Select question", null, true, true);
    defaultOption.disabled = true;
    securityQuestions.add(defaultOption);
    fetch(`${base_url}/SecurityQuestion/getall/0/0`, {
      headers:{
        'Content-Type': 'application/json',
        'Accept': 'application/json'
      }
    })
    .then(res => {return res.json()})
    .then(questions => {
        for (let i = 0; i < questions.entity.length; i++) {
            let option = new Option(questions.entity[i].question, questions.entity[i].id);
            securityQuestions.add(option);
        }
    })
    .catch(error => {
        console.error('Error:', error);
    });;


    // Get users transactions
    fetch(`${base_url}/Transaction/gettransactionsbyid/0/0/${userId}`, {
        headers:{
            'Authorization': `Bearer ${token}`
        }
    })
    .then(res => {return res.json()})
    .then(data => {
        if (data.succeeded) {
            const entityData = data.entity.entity;
            if (entityData.length > 0) {
                var temp = ""
                for (let i = 0; i < entityData.length; i++) {
                    temp += "<tr>";
                    temp += "<td>" + entityData[i].creditAddress + "</td>";
                    temp += "<td>" + entityData[i].amount + "</td>";
                    temp += "<td>" + entityData[i].transactionReference + "</td>";
                    temp += "<td>" + entityData[i].narration + "</td>";
                    temp += "<td>" + entityData[i].transactionStatusDesc + "</td>";
                    temp += "<td>" + entityData[i].createdDate + "</td>";
                    temp += "</tr>"
                }
                document.getElementById('tablebody').innerHTML = temp;
            }
        }
    })
    return false;
}

// Get bitcoin address method
function GetNewBitcoinAddress() {
      fetch(`${base_url}/User/generateaddress/${userId}`, {
        headers:{
            'Authorization': `Bearer ${token}`
        }
      })
    .then(res => res.json())
    .then(addressResponse => {
    if (!addressResponse.succeeded) {
        alert(addressResponse.message);
    }
    document.getElementById('bitcoin_address').innerHTML = addressResponse.entity;
    });
  return false; 
}

// Updateity Quuestions
function UpdateSecurityQuestion() {
    const response = document.getElementById("security_question_response").value;
    const securityQuestion = document.getElementById("security_questions").value;
    const myBody = {
        "securityQuestionId": parseInt(securityQuestion),
        "securityQuestionResponse": response,
        "userId": userId
    }
    fetch(`${base_url}/User/updatesecurityquestion`, {
      headers:{
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
        'Accept': 'application/json'
      },
      method: 'POST',
      body: JSON.stringify(myBody),
    })
    .then(res => res.json())
    .then(res => {
        document.getElementById("security_question_response").value = '';
        alert(res.message)
    });
    return false;
}


// Update user profile
function UpdateUserProfile(){
    const username = document.getElementById("fullName").value;
    const bio = document.getElementById("bio").value;
    const location = document.getElementById("location").value;
    const myBody = {
        "name": username,
        "bio": bio,
        "location": location,
        "userId": userId
    }
    console.log(JSON.stringify(myBody))
    fetch(`${base_url}/User/update`, {
        headers:{
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        },
        method: 'POST',
        redirect: 'manual',
        body: JSON.stringify(myBody),
    })
    .then(res => res.json())
    .then((data) => {
        console.log(data)
        if (data.succeeded) {
        }
        else{
            alert(data.message)
        }
    });
  return false;
}

// Change user password
function ChangeUserPassword(){
    const oldPassword = document.getElementById("old_password").value;
    const newPassword = document.getElementById("new_password").value;
    const myBody = {
        "newPassword": newPassword,
        "oldPassword": oldPassword,
        "userId": userId
    }
    console.log(JSON.stringify(myBody))
    fetch(`${base_url}/User/changepassword`, {
        headers:{
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        },
        method: 'POST',
        redirect: 'manual',
        body: JSON.stringify(myBody),
    })
    .then(res => res.json())
    .then((data) => {
        console.log(data)
        if (data.succeeded) {
            localStorage.removeItem('jwt');
            localStorage.removeItem('userid');
            window.location = "login.html"
        }
        else{
            alert(data.message)
        }
    });
  return false;
}

// Make payment
function MakePayment(){
    const description = document.getElementById("payment_description").value;
    const amount = document.getElementById("payment_amount").value;
    const destination = document.getElementById("payment_destination").value;
    const myBody = {
        "amountInBtc": amount,
        "description": description,
        "destinationAddress": destination,
        "userId": userId
    }
    console.log(JSON.stringify(myBody))
    fetch(`${base_url}/User/paymentrequest`, {
        headers:{
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        },
        method: 'POST',
        redirect: 'manual',
        body: JSON.stringify(myBody),
    })
    .then(res => res.json())
    .then((data) => {
        console.log(data)
        if (data.succeeded) {
            console.log(data);
            alert(data.message);
            window.location.href = `paymentotp.html`
        }
        else{
            alert(data.message)
        }
    });
  return false;
}

// Get User Information
function GetUser() {
    const username = document.getElementById("fullName").value;
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
          }
          else{
              alert(data.message)
          }
      });
  return false;
}



loadIntoTable()
