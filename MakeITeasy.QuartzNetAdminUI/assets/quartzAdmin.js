function displayClock(elementID) {
    var date = new Date();

    if (document.getElementById(elementID) != null)
    {
        document.getElementById(elementID).innerText = date;
        document.getElementById(elementID).textContent = date;
    }

    setTimeout(displayClock.bind(null, elementID), 1000);
}

function formatDateFromApi(input) {
    let date = new Date(input);
    return date.getFullYear() > 1 ? date.toLocaleString() : "";
}