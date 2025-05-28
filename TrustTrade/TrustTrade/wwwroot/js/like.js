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
                    window.location.href = '/Identity/Account/Login';
                    return;
                }
                throw new Error('Failed to toggle like');
            }

            // Check if response is JSON before parsing
            const contentType = response.headers.get('content-type');
            if (!contentType || !contentType.includes('application/json')) {
                // If not JSON, likely a redirect or error page, redirect to login
                window.location.href = '/Identity/Account/Login';
                return;
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
});