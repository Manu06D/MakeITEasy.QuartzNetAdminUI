console.log('inside');


fetch("/quartzAdmin-api?action=GetJobs&argument=")
    .then((response) => response.json())
    .then((groups) => console.log(groups));