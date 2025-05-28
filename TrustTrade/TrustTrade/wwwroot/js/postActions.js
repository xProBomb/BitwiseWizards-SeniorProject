document.addEventListener("DOMContentLoaded", () => {
    // Confirmation for deleting a post
    document.querySelectorAll(".delete-post-form").forEach(form => {
        form.addEventListener("submit", (event) => {
            if (!confirm("Are you sure you want to delete this post?")) {
                event.preventDefault(); // Stop form submission if user cancels
            }
        });
    });

    // Confirmation for canceling an edit
    document.querySelectorAll(".cancel-edit").forEach(form => {
        form.addEventListener("click", (event) => {
            if (!confirm("Are you sure you want to cancel? Unsaved changes will be lost.")) {
                event.preventDefault(); // Stop navigation if user cancels
            }
        });
    });

    // Attach click event listener to all save post buttons in the feed
    const postFeedContainer = document.getElementById("postFeedContainer");
    if (postFeedContainer) {
        postFeedContainer.addEventListener("click", async function (e) {
            const button = e.target.closest(".save-post-button");
            if (button) {
                e.preventDefault();
                await togglePostSave(button);
            }
        });
    }

    // Attach click event listener for save button on post details page
    const detailsSaveButton = document.querySelector(".save-post-button");
    if (detailsSaveButton && !postFeedContainer) {
        detailsSaveButton.addEventListener("click", async function (e) {
            e.preventDefault();
            await togglePostSave(detailsSaveButton);
        });
    }
});

async function togglePostSave(button) {
    // Disable the save button to prevent multiple clicks
    button.disabled = true;

    const postId = button.getAttribute("data-post-id");
    const isSaved = button.dataset.isSaved?.toLowerCase() === "true"; // Use data attribute to check if saved
    const buttonStatusText = button.querySelector(".save-status-text");
    const iconElement = button.querySelector(".bi");

    let response;
    try {
        if (isSaved) {
            response = await fetch(`/api/saves/posts/${postId}`, {
                method: 'DELETE',
                headers: {
                    'Accept': 'application/problem+json; charset=utf-8',
                    'Content-Type': 'application/json; charset=utf-8'
                },
                credentials: 'same-origin'
            });
        }
        else {
            response = await fetch(`/api/saves/posts/${postId}`, {
                method: 'POST',
                headers: {
                    'Accept': 'application/problem+json; charset=utf-8',
                    'Content-Type': 'application/json; charset=utf-8'
                },
                credentials: 'same-origin'
            });
        }

        if (!response.ok) {
            // Handle unauthorized or error
            if (response.status === 401) {
                // Redirect to login page
                window.location.href = '/Identity/Account/Login?ReturnUrl=' + encodeURIComponent(window.location.pathname);
                return;
            }
            throw new Error('Failed to toggle save');
        }

        const data = await response.json();

        // Update save button appearance and status text
        if (isSaved) {
            iconElement.className = "bi bi-bookmark";
            button.classList.remove("saved");
            button.dataset.isSaved = "false";
            buttonStatusText.textContent = "Save";
        } else {
            iconElement.className = "bi bi-bookmark-fill";
            button.classList.add("saved");
            button.dataset.isSaved = "true";
            buttonStatusText.textContent = "Unsave";
        }
    }
    catch (error) {
        console.error('Error toggling save:', error);
        // Restore original icon on error
        const iconElement = button.querySelector(".bi");
        iconElement.className = isSaved ? "bi bi-bookmark-fill" : "bi bi-bookmark";
    } finally {
        // Re-enable the button after processing
        button.disabled = false;
    }
}
