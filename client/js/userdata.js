const base_url = 'https://localhost:7205/api';

function loadIntoTable() {
    const userId = localStorage.getItem('userid');
    const token = localStorage.getItem('jwt');
    
    // Get UserById
    fetch(`${base_url}/User/getuserbyid/${userId}`, {
        headers:{
            'Authorization': `Bearer ${token}`
        }
    })
    .then(response => response.json())
    .then(data => {
        console.log(data)
        const inputField = document.getElementById('fullName');
        inputField.value = data.entity.name;
    })
    .catch(error => {
    console.error('Error:', error);
    });

    fetch('https://jsonplaceholder.typicode.com/todos')
    .then(res => {return res.json()})
    .then(data => {
        if (data.length > 0) {
            var temp = ""
            for (let i = 0; i < data.length; i++) {
                temp += "<tr>";
                temp += "<td>" + data[i].completed + "</td>";
                temp += "<td>" + data[i].id + "</td>";
                temp += "<td>" + data[i].title + "</td>";
                temp += "<td>" + data[i].userId + "</td>";
                temp += "</tr>"
            }
            document.getElementById('tablebody').innerHTML = temp;
        }
        else{

        }
    })
    .catch(error => console.log(error)); 
}

// User login method
function GetNewBitcoinAddress() {
    const userId = document.getElementById("settings_userId").value;

      fetch(`${base_url}/User/generateaddress/${userId}`, {
        headers:{
            Authentication: `Bearer {token}`
        }
      })
    .then(data => {
    return data.json();
    })
    .then(post => {
    console.log(post);
    document.getElementById('bitcoin_address').innerHTML = post.address;
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
