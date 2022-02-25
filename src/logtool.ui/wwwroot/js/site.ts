interface IDatabaseColumnSource {
    index: number;
    name: string;
}

interface IDatabaseColumn {
    index: number;
    name: string;
    dataType: number;
    sources: IDatabaseColumnSource[];
    dataTypeString: string;
    parameterName: string;
}

interface ISelectFilesResponse {
    files: string[];
    databaseColumns: IDatabaseColumn[];
    validationErrors: string[];
    missingColumnErrors: string[]
    errorsOccurred: boolean;
}

const getFilesForm = document.getElementById('getfiles') as HTMLFormElement;
const getFilesFolderInput = document.getElementById('folder') as HTMLInputElement;

const selectFilesForm = document.getElementById('selectfiles') as HTMLFormElement;
const selectFilesFilesInput = document.getElementById('files') as HTMLSelectElement;

const dataLoader = document.getElementById('dataloading') as HTMLDivElement;

const columns = document.getElementById('columns') as HTMLDivElement;

const queryForm = document.getElementById('runquery') as HTMLFormElement;
const queryFormQueryInput = document.getElementById('query') as HTMLInputElement;
const queryFormPageInput = document.getElementById('page') as HTMLInputElement;
const queryFormSubmitButton = document.getElementById('querysubmit') as HTMLButtonElement;

const queryLoader = document.getElementById('queryloading') as HTMLDivElement;

const pagination = document.getElementById('pagination') as HTMLUListElement;

const resultsFrame = document.getElementById('results') as HTMLIFrameElement;

function getFormAction(e: SubmitEvent) {
    return (e.target as HTMLFormElement).action;
}

function paginationLink(page: number, active: boolean) {
    return `<li class="page-item${(active ? ' active' : '')}"><a class="page-link page-link-direct" href="#" data-page="${page}">${page}</a></li>`;
}

function buildPagination(pages: number, active: number) {
    const links = [
        '<li class="page-item"><a class="page-link page-link-prev" href="#" aria-label="Previous"><span aria-hidden="true">&laquo;</span></a></li>'
    ];

    for (let i = 1; i <= pages; i++) {
        links.push(paginationLink(i, i === active));
    }

    links.push('<li class="page-item"><a class="page-link page-link-next" href="#" aria-label="Next"><span aria-hidden="true">&raquo;</span></a></li>');

    return links.join('');
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

function displayColumns(columns: IDatabaseColumn[]) {
    return 'Columns: ' + columns.map(c => `${c.name} [${c.dataTypeString}]`).join(', ');
}

function loadFiles(folder: string, onFilesLoaded?: () => void) {
    post(getFilesForm.action, { folder: folder }, response => {
        localStorage.setItem('folder', getFilesFolderInput.value);
        selectFilesFilesInput.innerHTML = response.map((f: string) => '<option>' + f + '</option>').join('');

        if (typeof onFilesLoaded === 'function') {
            onFilesLoaded();
        }
    });
}

getFilesForm.addEventListener('submit', function (e) {
    e.preventDefault();

    loadFiles(getFilesFolderInput.value);
});

selectFilesForm.addEventListener('submit', function (e) {
    e.preventDefault();

    const selectedFiles = Array
        .from(selectFilesFilesInput.selectedOptions)
        .map(o => o.value);

    const request = {
        files: selectedFiles
    };

    dataLoader.innerText = 'Loading...';

    post(getFormAction(e), request, (response: ISelectFilesResponse) => {
        localStorage.setItem('files', response.files.join('|'));
        dataLoader.innerText = '';

        localStorage.setItem('columns', JSON.stringify(response.databaseColumns));
        columns.innerText = displayColumns(response.databaseColumns);
    });
});

queryFormSubmitButton.addEventListener('click', function (e) {
    queryLoader.innerText = 'Loading...';

    queryFormPageInput.value = '1';

    const request = {
        query: queryFormQueryInput.value
    };

    post('/api/resultcount', request, response => {
        localStorage.setItem('query', queryFormQueryInput.value);
        pagination.innerHTML = buildPagination(response.totalPages, 1);
        dataLoader.innerText = '';
    });
});

resultsFrame.addEventListener('load', function (e) {
    queryLoader.innerText = '';
});

document.addEventListener('click', function (e) {
    const a = (e.target as HTMLAnchorElement);

    if (a.classList.contains('page-link')) {
        e.preventDefault();

        const currentPage = parseInt(queryFormPageInput.value, 10);
        let newPage = -1;
        if (a.classList.contains('page-link-prev')) {
            newPage = currentPage - 1;
        } else if (a.classList.contains('page-link-next')) {
            newPage = currentPage + 1;
        } else {
            newPage = parseInt(a.getAttribute('data-page'), 10);
        }

        Array.from(pagination.getElementsByClassName('active')).forEach(el => el.classList.remove('active'));
        pagination.querySelector('[data-page="' + newPage + '"]').parentElement.classList.add('active');

        queryFormPageInput.value = newPage.toString();

        queryForm.requestSubmit();
    }
});

const previousFolder = localStorage.getItem('folder');
const previousFiles = localStorage.getItem('files');
const previousColumns = localStorage.getItem('columns');
const previousQuery = localStorage.getItem('query');

if (previousFolder) {
    getFilesFolderInput.value = previousFolder;

    loadFiles(getFilesFolderInput.value, () => {
        if (previousFiles) {
            const filesToSelect = previousFiles.split('|');

            Array.from(selectFilesFilesInput.options)
                .filter(o => filesToSelect.includes(o.text))
                .forEach(o => { o.selected = true; });
        }
        if (previousColumns) {
            columns.innerText = displayColumns(JSON.parse(previousColumns));
        }
    });
}





queryFormQueryInput.value = previousQuery ?? "SELECT * FROM entries LIMIT 100";
