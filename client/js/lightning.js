const inputField = document.querySelector(".copy-link-input");
const copyButton = document.querySelector(".copy-link-button");

const base_url = 'https://localhost:7205/api';

const urlParams = new URLSearchParams(window.location.search);
console.log(urlParams);
const invoice = urlParams.get('invoice');
const url = urlParams.get('invoiceurl');
inputField.value = invoice;
const text = inputField.value;

inputField.addEventListener("focus", () => inputField.select());
copyButton.addEventListener("click", () => {
    console.log(`Text before copy: ${text}`);
    inputField.select();
    navigator.clipboard.writeText(text);
    inputField.value = "Copied!";
    setTimeout(() => (inputField.value = text), 2000);
});


async function fetchMoviesJSON() {
    console.log("running errands");
    const myBody = {}
    const response = await fetch(`${base_url}/Auth/listenforpayment`, {
        method: 'POST',
        body: JSON.stringify(myBody),
        headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
            }
        });
    const dataa = await response.json();
    console.log(dataa);
    if (dataa.succeeded) {
        window.location.href = "login.html"
    }
    else{
        alert(dataa.message)
    }
    return false;
}
  
fetchMoviesJSON();