const searchInput = document.getElementById('searchTerm');
const searchType = document.getElementById('searchType');
const resultsContainer = document.getElementById('searchResults');

let lastSearch = "";

function performSearch(term) {
    const type = searchType.value;

    loading = true;

    fetch(`/Search/${type === "posts" ? "Posts" : "SearchUsers"}?search=${encodeURIComponent(term)}`, {
        headers: { 'X-Requested-With': 'XMLHttpRequest' }
    })
    .then(res => res.text())
    .then(html => {
        resultsContainer.innerHTML = html;
    })
    .catch(() => {
        resultsContainer.innerHTML = "<p>Error loading results.</p>";
    });
}

function debounce(fn, delay) {
    let timer;
    return function (...args) {
        clearTimeout(timer);
        timer = setTimeout(() => fn(...args), delay);
    };
}

searchInput.addEventListener('input', debounce(() => {
    currentPage = 1;
    lastSearch = searchInput.value;
    performSearch(lastSearch);
}, 300));

searchType.addEventListener('change', () => {
    currentPage = 1;
    lastSearch = searchInput.value;
    performSearch(lastSearch);
});


