// Chat user search functionality
document.addEventListener('DOMContentLoaded', function() {
    // Only initialize if we're on the chat index page with the search modal
    if (document.getElementById('newConversationModal')) {
        initChatUserSearch();
    }
});

function initChatUserSearch() {
    const userSearchInput = document.getElementById('userSearchInput');
    const searchButton = document.getElementById('searchButton');
    const userSearchResults = document.getElementById('userSearchResults');
    
    // Debounce function to limit search requests
    function debounce(func, wait) {
        let timeout;
        return function(...args) {
            clearTimeout(timeout);
            timeout = setTimeout(() => func.apply(this, args), wait);
        };
    }
    
    // Function to search for users
    const searchUsers = debounce(function() {
        const term = userSearchInput.value.trim();
        
        // Check if search term is long enough
        if (term.length < 2) {
            userSearchResults.innerHTML = '<p class="text-center text-muted">Enter at least 2 characters to search</p>';
            return;
        }
        
        // Show loading indicator
        userSearchResults.innerHTML = '<div class="text-center py-3"><i class="bi bi-hourglass-split fs-4 text-accent"></i><p class="mt-2">Searching...</p></div>';
        
        // Perform the search
        fetch(`/Search/SearchUsers?search=${encodeURIComponent(term)}`)
            .then(response => response.text())
            .then(html => {
                // Parse the HTML response
                const parser = new DOMParser();
                const doc = parser.parseFromString(html, 'text/html');
                
                // Check if we have the no users found message
                if (doc.querySelector('.text-center.py-5')) {
                    userSearchResults.innerHTML = '<div class="text-center py-3"><i class="bi bi-person-x fs-3 text-secondary"></i><p class="mt-2">No users found</p></div>';
                    return;
                }
                
                // Clear previous results
                userSearchResults.innerHTML = '';
                
                // Find all user cards in the search results
                const userCards = doc.querySelectorAll('.user-card');
                if (userCards.length === 0) {
                    // Try alternative selectors based on your HTML structure
                    const alternativeCards = doc.querySelectorAll('.col-md-6.col-lg-4');
                    if (alternativeCards.length === 0) {
                        userSearchResults.innerHTML = '<div class="text-center py-3"><i class="bi bi-question-circle fs-3 text-secondary"></i><p class="mt-2">Could not process search results</p></div>';
                        return;
                    }
                    
                    // Process the alternative cards
                    alternativeCards.forEach(card => {
                        // Extract profile link which contains the username
                        const profileLink = card.querySelector('a[href*="UserProfile"]');
                        if (!profileLink) return;
                        
                        // Extract username from the profile link or card content
                        const usernameEl = card.querySelector('h5') || card.querySelector('.fw-semibold');
                        const username = usernameEl ? usernameEl.textContent.trim() : 'User';
                        
                        // Extract image source
                        const imgEl = card.querySelector('img');
                        const imgSrc = imgEl ? imgEl.getAttribute('src') : '/icons/defpfp.png';
                        
                        // Extract user ID from the URL or data attribute
                        let userId;
                        // First try to find a data-user-id attribute
                        const userCard = card.querySelector('[data-user-id]');
                        if (userCard) {
                            userId = userCard.getAttribute('data-user-id');
                        } else {
                            // Otherwise, try to extract from the profile URL
                            const href = profileLink.getAttribute('href');
                            // The URL might be in the format /Profile/UserProfile/123 or /Profile/UserProfile?username=xyz
                            // We need to extract the user ID from the appropriate part
                            const urlParts = href.split('/');
                            if (urlParts.length > 0 && !isNaN(urlParts[urlParts.length - 1])) {
                                // If the last part is a number, use it as the user ID
                                userId = urlParts[urlParts.length - 1];
                            } else {
                                // Try to find a data attribute with the user ID
                                const viewProfileBtn = card.querySelector('.view-profile-btn');
                                if (viewProfileBtn && viewProfileBtn.hasAttribute('data-user-id')) {
                                    userId = viewProfileBtn.getAttribute('data-user-id');
                                } else {
                                    console.error('Could not extract user ID from:', card);
                                    return; // Skip this card if we can't get the user ID
                                }
                            }
                        }
                        
                        // Create the user search item
                        const userItem = document.createElement('div');
                        userItem.className = 'user-search-item';
                        userItem.innerHTML = `
    <img src="${imgSrc}" alt="${username}">
    <div class="d-flex justify-content-between align-items-center w-100">
    <div>
    <h6 class="mb-0">${username}</h6>
</div>
<button type="button" class="btn btn-sm btn-accent start-chat-btn" data-user-id="${userId}">
    <i class="bi bi-chat-text me-1"></i> Message
</button>
</div>
`;
                        
                        userSearchResults.appendChild(userItem);
                    });
                    
                    // Add event listeners to the new buttons
                    userSearchResults.querySelectorAll('.start-chat-btn').forEach(btn => {
                        btn.addEventListener('click', function() {
                            const userId = this.getAttribute('data-user-id');
                            startConversation(userId);
                        });
                    });
                    
                    return;
                }
                
                // Process regular user cards
                userCards.forEach(card => {
                    const username = card.querySelector('h5').textContent;
                    const userId = card.getAttribute('data-user-id');
                    const imgSrc = card.querySelector('img').getAttribute('src');
                    
                    const userItem = document.createElement('div');
                    userItem.className = 'user-search-item';
                    userItem.innerHTML = `
<img src="${imgSrc}" alt="${username}">
    <div class="d-flex justify-content-between align-items-center w-100">
    <div>
    <h6 class="mb-0">${username}</h6>
</div>
<button type="button" class="btn btn-sm btn-accent start-chat-btn" data-user-id="${userId}">
    <i class="bi bi-chat-text me-1"></i> Message
</button>
</div>
`;
                    
                    userSearchResults.appendChild(userItem);
                });
                
                // Add event listeners to the new buttons
                userSearchResults.querySelectorAll('.start-chat-btn').forEach(btn => {
                    btn.addEventListener('click', function() {
                        const userId = this.getAttribute('data-user-id');
                        startConversation(userId);
                    });
                });
            })
            .catch(error => {
                console.error('Error searching users:', error);
                userSearchResults.innerHTML = '<div class="text-center py-3"><i class="bi bi-exclamation-triangle fs-3 text-danger"></i><p class="mt-2">Error searching for users</p></div>';
            });
    }, 300);
    
    // Function to start a conversation with a user
    function startConversation(userId) {
        if (!userId) {
            console.error('No user ID provided for starting conversation');
            return;
        }
        
        console.log('Starting conversation with user ID:', userId);
        
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
        
        // Log before submission for debugging
        console.log('Submitting form to start conversation with user ID:', userId);
        
        form.submit();
    }
    
    // Event listeners
    if (userSearchInput) {
        userSearchInput.addEventListener('input', searchUsers);
        
        // Search on enter key
        userSearchInput.addEventListener('keydown', function(e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                searchUsers();
            }
        });
    }
    
    if (searchButton) {
        searchButton.addEventListener('click', searchUsers);
    }
    
    // Clear search input when modal is closed/opened
    const searchModal = document.getElementById('newConversationModal');
    if (searchModal) {
        searchModal.addEventListener('show.bs.modal', function() {
            // Clear previous search results and input
            userSearchInput.value = '';
            userSearchResults.innerHTML = '<p class="text-center text-muted">Search for users to start a conversation</p>';
            
            // Focus the search input
            setTimeout(() => {
                userSearchInput.focus();
            }, 500);
        });
    }
}