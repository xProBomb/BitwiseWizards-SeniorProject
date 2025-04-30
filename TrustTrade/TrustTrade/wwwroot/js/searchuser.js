const searchInput = document.getElementById('searchTerm');
const searchType = document.getElementById('searchType');
const resultsContainer = document.getElementById('searchResults');

let lastSearch = "";
let currentPage = 1;

function performSearch(term) {
    const type = searchType.value;
    
    // Show loading indicator
    resultsContainer.innerHTML = '<div class="text-center p-3"><i class="bi bi-hourglass-split text-accent"></i> Searching...</div>';

    fetch(`/Search/${type === "posts" ? "Posts" : "SearchUsers"}?search=${encodeURIComponent(term)}`, {
        headers: { 'X-Requested-With': 'XMLHttpRequest' }
    })
    .then(res => res.text())
    .then(html => {
        resultsContainer.innerHTML = html;
        
        // Initialize message buttons after loading search results
        initializeMessageButtons();
    })
    .catch(() => {
        resultsContainer.innerHTML = '<div class="text-center p-3 text-danger"><i class="bi bi-exclamation-triangle"></i> Error loading results.</div>';
    });
}

// Function to initialize message buttons
function initializeMessageButtons() {
    document.querySelectorAll('.start-chat-btn, [data-action="start-conversation"]').forEach(button => {
        // Remove any existing event listeners by cloning and replacing the element
        const newButton = button.cloneNode(true);
        button.parentNode.replaceChild(newButton, button);
        
        // Add event listener to the new button
        newButton.addEventListener('click', function(e) {
            e.preventDefault();
            
            const userId = this.getAttribute('data-user-id');
            if (!userId) {
                console.error('No user ID found on button:', this);
                return;
            }
            
            console.log('Clicked message button for user ID:', userId);
            
            // Check if global function exists and use it
            if (typeof window.startConversation === 'function') {
                window.startConversation(userId);
            } else {
                // Fallback: Create and submit a form directly
                createAndSubmitStartConversationForm(userId);
            }
        });
    });
}

// Function to create and submit a form to start a conversation
function createAndSubmitStartConversationForm(userId) {
    console.log('Creating form to start conversation with user ID:', userId);
    
    // Create a form to submit
    const form = document.createElement('form');
    form.method = 'post';
    form.action = '/Chat/StartConversation';
    
    // Add CSRF token if the site uses it
    const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
    if (tokenElement) {
        const tokenInput = document.createElement('input');
        tokenInput.type = 'hidden';
        tokenInput.name = '__RequestVerificationToken';
        tokenInput.value = tokenElement.value;
        form.appendChild(tokenInput);
    }
    
    // Add the user ID
    const userIdInput = document.createElement('input');
    userIdInput.type = 'hidden';
    userIdInput.name = 'userId';
    userIdInput.value = userId;
    
    // Add to form and submit
    form.appendChild(userIdInput);
    document.body.appendChild(form);
    
    console.log('Submitting form to start conversation...');
    form.submit();
}

function debounce(fn, delay) {
    let timer;
    return function (...args) {
        clearTimeout(timer);
        timer = setTimeout(() => fn(...args), delay);
    };
}

// Initialize event listeners when document is ready
document.addEventListener('DOMContentLoaded', function() {
    if (searchInput) {
        searchInput.addEventListener('input', debounce(() => {
            currentPage = 1;
            lastSearch = searchInput.value;
            performSearch(lastSearch);
        }, 300));
    }
    
    if (searchType) {
        searchType.addEventListener('change', () => {
            currentPage = 1;
            lastSearch = searchInput.value;
            performSearch(lastSearch);
        });
    }
    
    // Initialize any message buttons already on the page
    initializeMessageButtons();
    
    // If there's already a search term in the input, perform search
    if (searchInput && searchInput.value.trim() !== '') {
        lastSearch = searchInput.value.trim();
        performSearch(lastSearch);
    }
});

// Export functions for external use
window.SearchUtils = {
    performSearch: performSearch,
    initializeMessageButtons: initializeMessageButtons
};