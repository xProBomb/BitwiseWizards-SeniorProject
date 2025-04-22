const searchInput = document.getElementById('searchTerm');
const searchType = document.getElementById('searchType');
const resultsContainer = document.getElementById('searchResults');

let currentPage = 1;
let loading = false;
let lastSearch = "";

function performSearch(term, page = 1, append = false) {
    const type = searchType.value;

    if (!term.trim() || term.length > 50) {
        resultsContainer.innerHTML = "<p>Invalid search term.</p>";
        return;
    }

    loading = true;

    fetch(`/Search/${type === "posts" ? "SearchPosts" : "SearchUsers"}?search=${encodeURIComponent(term)}&pageNumber=${page}`, {
        headers: { 'X-Requested-With': 'XMLHttpRequest' }
    })
        .then(res => res.text())
        .then(html => {
            if (append) {
                resultsContainer.insertAdjacentHTML("beforeend", html);
            } else {
                resultsContainer.innerHTML = html;
            }
            loading = false;
        })
        .catch(() => {
            resultsContainer.innerHTML = "<p>Error loading results.</p>";
            loading = false;
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

window.addEventListener('scroll', () => {
    if (loading) return;

    const scrollPos = window.innerHeight + window.scrollY;
    if (scrollPos >= document.body.offsetHeight - 300) {
        currentPage++;
        performSearch(lastSearch, currentPage, true);
    }
});
