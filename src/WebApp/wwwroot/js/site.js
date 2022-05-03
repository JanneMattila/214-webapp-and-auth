console.log(`WebApp.ApiRoot: ${WebApp.ApiRoot}`);

fetch(`${WebApp.ApiRoot}/values`)
    .then(response => {
        return response.json();
    })
    .then(data => {
        console.log(data);
    })
    .catch(error => {
        console.log(error);
    });
