const getFilesForm = document.getElementById('getfiles') as HTMLFormElement;
const getFilesFolderInput = document.getElementById('folder') as HTMLInputElement;

const selectFilesForm = document.getElementById('selectfiles') as HTMLFormElement;
const selectFilesFilesInput = document.getElementById('files') as HTMLSelectElement;

const loadedFiles = document.getElementById('loadedfiles') as HTMLDivElement;

function getFormAction(e: SubmitEvent) {
    return (e.target as HTMLFormElement).action;
}

function post(endpoint: string, data: any, callback: (data: any) => void) {
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
        selectFilesFilesInput.innerHTML = response.map((f: string) => '<option>' + f + '</option>');
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

        loadedFiles.innerHTML = '<ul>' + response.files.map((f: string) => '<li>' + f + '</li>').join('') + '</ul>';
    });
});
