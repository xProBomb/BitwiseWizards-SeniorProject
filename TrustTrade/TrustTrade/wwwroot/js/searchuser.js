const searchInput = document.getElementById('searchTerm');
const resultsContainer = document.getElementById('searchResults');
let timer;

// Attach keyup event
if (searchInput) {
    searchInput.addEventListener('keyup', function () {
        clearTimeout(timer);

        // Use a small delay to avoid sending too many requests
        timer = setTimeout(() => {
            performSearch(searchInput.value);
        }, 300);
    });
}

function performSearch(term) {
    // will need to add more validation
    if (term.length > 50) {
        resultsContainer.innerHTML = "<p>Search term is too long.</p>";
        return;
    }

    // If term is empty or whitespace, clear the results
    if (!term.trim()) {
        resultsContainer.innerHTML = "";
        return;
    }

    // Use fetch to call our controller action
    fetch(`/Search/SearchUsers?searchTerm=${encodeURIComponent(term)}`, {
        method: 'GET'
    })
    .then(response => {
        if (!response.ok) {
            // handle error (e.g., 400 Bad Request)
            throw new Error('Network response was not ok');
        }
        return response.text(); // we’re returning a partial view (HTML)
    })
    .then(html => {
        resultsContainer.innerHTML = html;
    })
    .catch(error => {
        console.error('Fetch error:', error);
        resultsContainer.innerHTML = "<p>Error fetching search results.</p>";
    });
}
