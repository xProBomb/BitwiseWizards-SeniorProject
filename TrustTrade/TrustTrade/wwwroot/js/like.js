document.addEventListener('DOMContentLoaded', function() {
    // Setup event delegation for like buttons
    document.body.addEventListener('click', async function(event) {
        // Find closest like button if clicked on button or its child elements
        const likeButton = event.target.closest('.like-btn');
        if (!likeButton) return;
        
        event.preventDefault();
        event.stopPropagation(); // Prevent click from bubbling to post link
        
        const postId = likeButton.getAttribute('data-post-id');
        if (!postId) return;
        
        try {
            // Show loading state
            likeButton.disabled = true;
            const iconElement = likeButton.querySelector('.bi');
            const countElement = likeButton.querySelector('.like-count');
            const originalIcon = iconElement.className;
            iconElement.className = 'bi bi-hourglass-split';
            
            // Make API request to toggle like
            const response = await fetch(`/api/Like/toggle/${postId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                credentials: 'same-origin'
            });
            
            if (!response.ok) {
                // Handle unauthorized or error
                if (response.status === 401) {
                    // Redirect to login page
                    window.location.href = '/Identity/Account/Login?ReturnUrl=' + encodeURIComponent(window.location.pathname);
                    return;
                }
                throw new Error('Failed to toggle like');
            }
            
            const data = await response.json();
            
            // Update button appearance
            if (data.isLiked) {
                iconElement.className = 'bi bi-hand-thumbs-up-fill';
                likeButton.classList.add('liked');
            } else {
                iconElement.className = 'bi bi-hand-thumbs-up';
                likeButton.classList.remove('liked');
            }
            
            // Update like count
            if (countElement) {
                countElement.textContent = data.likeCount;
            }
            
        } catch (error) {
            console.error('Error toggling like:', error);
            // Restore original icon on error
            const iconElement = likeButton.querySelector('.bi');
            iconElement.className = 'bi bi-hand-thumbs-up';
        } finally {
            likeButton.disabled = false;
        }
    });
    
    // Initialize like status for posts when page loads
    async function fetchLikeStatus() {
        const likeButtons = document.querySelectorAll('.like-btn[data-post-id]');
        
        for (const button of likeButtons) {
            const postId = button.getAttribute('data-post-id');
            try {
                const response = await fetch(`/api/Like/status/${postId}`, {
                    method: 'GET',
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'
                    },
                    credentials: 'same-origin'
                });
                
                if (!response.ok) continue;
                
                const data = await response.json();
                const iconElement = button.querySelector('.bi');
                const countElement = button.querySelector('.like-count');
                
                // Update button appearance based on like status
                if (data.isLiked) {
                    iconElement.className = 'bi bi-hand-thumbs-up-fill';
                    button.classList.add('liked');
                } else {
                    iconElement.className = 'bi bi-hand-thumbs-up';
                    button.classList.remove('liked');
                }
                
                // Update like count
                if (countElement) {
                    countElement.textContent = data.likeCount;
                }
                
            } catch (error) {
                console.error(`Error fetching like status for post ${postId}:`, error);
            }
        }
    }
    
    // Only fetch like status if there are like buttons on the page
    if (document.querySelectorAll('.like-btn[data-post-id]').length > 0) {
        fetchLikeStatus();
    }
});