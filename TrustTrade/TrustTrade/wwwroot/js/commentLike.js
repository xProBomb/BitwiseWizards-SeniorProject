document.addEventListener("DOMContentLoaded", () => {
    // Attach click event listener to all comment like buttons
    const commentsContainer = document.getElementById("comments-section");
    if (commentsContainer) {
        commentsContainer.addEventListener("click", async function (e) {
            const button = e.target.closest(".comment-like-button");
            if (button) {
                e.preventDefault();
                await toggleCommentLike(button);
            }
        });
    }
});

async function toggleCommentLike(button) {
    // Disable the like button to prevent multiple clicks
    button.disabled = true;

    const commentId = button.getAttribute("data-comment-id");
    const iconElement = button.querySelector(".bi");
    const countElement = button.querySelector(".like-count");

    try {
        const response = await fetch(`/api/comments/${commentId}/toggleLike`, {
            method: 'POST',
            headers: {
                'Accept': 'application/problem+json; charset=utf-8',
                'Content-Type': 'application/json; charset=utf-8'
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
        
        // Update like button appearance
        if (data.isLiked) {
            iconElement.className = "bi bi-hand-thumbs-up-fill";
            button.classList.add("liked");
        } else {
            iconElement.className = "bi bi-hand-thumbs-up";
            button.classList.remove("liked");
        }

        // Update like count
        if (countElement) {
            countElement.textContent = data.likeCount;
        }

        console.log("Updated like count")
    } catch (error) {
        console.error('Error toggling like:', error);
        // Restore original icon on error
        iconElement.className = "bi bi-hand-thumbs-up";
    }
    finally {
        // Re-enable the button after processing
        button.disabled = false;
    }
}
