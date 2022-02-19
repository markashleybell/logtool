const getFilesForm = document.getElementById('getfiles');
const getFilesFolderInput = document.getElementById('folder');
const selectFilesForm = document.getElementById('selectfiles');
const selectFilesFilesInput = document.getElementById('files');
const loadedFiles = document.getElementById('loadedfiles');
function getFormAction(e) {
    return e.target.action;
}
function post(endpoint, data, callback) {
    fetch(endpoint, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
    })
        .then(response => response.json())
        .then(callback)
        .catch((error) => window.alert(error));
}
getFilesForm.addEventListener('submit', function (e) {
    e.preventDefault();
    const request = {
        folder: getFilesFolderInput.value
    };
    post(getFormAction(e), request, response => {
        selectFilesFilesInput.innerHTML = response.map((f) => '<option>' + f + '</option>');
    });
});
selectFilesForm.addEventListener('submit', function (e) {
    e.preventDefault();
    const selectedFiles = Array
        .from(selectFilesFilesInput.selectedOptions)
        .map(o => o.value);
    const request = {
        files: selectedFiles
    };
    loadedFiles.innerHTML = 'Loading...';
    post(getFormAction(e), request, response => {
        console.log(response);
        loadedFiles.innerHTML = '<ul>' + response.files.map((f) => '<li>' + f + '</li>').join('') + '</ul>';
    });
});
//# sourceMappingURL=site.js.map