document.addEventListener("DOMContentLoaded", () => {
    const contentInput = document.getElementById('content');
    const postId = document.getElementById('postId').value;
    const submitButtonWrapper = document.getElementById('submitButtonWrapper');
    const submitCommentButton = document.getElementById('submitCommentButton');
    const commentsList = document.querySelector('.list-group');
    const commentCount = document.getElementById('commentCount');

    contentInput.addEventListener('focus', () => {
        submitButtonWrapper.style.display = 'block';
    });

    contentInput.addEventListener('blur', () => {
        if (!contentInput.value.trim()) {
            setTimeout(() => {
                submitButtonWrapper.style.display = 'none';
            }, 100);
        }
    });

    // Attach click event listener to all delete buttons
    document.querySelectorAll(".delete-comment-button").forEach(button => {
        button.addEventListener("click", async () => {
            setupDeleteCommentButton(button);
        });
    });

    submitCommentButton.addEventListener('click', async () => {
        const content = contentInput.value.trim();

        if (!content) {
            alert("Comment cannot be empty!");
            return;
        }

        const comment = {
            PostId: postId,
            Content: content
        };

        try {
            const response = await fetch(`/api/comments`, {
                method: 'POST',
                headers: {
                    'Accept': 'application/problem+json; charset=utf-8',
                    'Content-Type': 'application/json; charset=utf-8'
                },
                credentials: 'same-origin',
                body: JSON.stringify(comment)
            });

            if (response.status === 401) {
                // TODO: Send unregistered user to login page
                // User is not authenticated, redirect to login page
                //window.location.href = '/Account/Login';
                return;
            }

            if (!response.ok) {
                // TODO: Display error message to the user
                return
            }

            if (response.ok) {
                // Clear the input
                contentInput.value = '';
                submitButtonWrapper.style.display = 'none';

                const { commentId } = await response.json();

                // Get the new comment HTML from the server
                const htmlResponse = await fetch(`/comments/rendercommentpartial/${commentId}`);

                if (!htmlResponse.ok) {
                    // TODO: Display error message to the user
                    return;
                }

                // Remove the paragraph element if the list is empty
                console.log("Hello1");
                const emptyParagraph = commentsList.querySelector("p");
                if (emptyParagraph) {
                    emptyParagraph.remove();
                }

                const commentHtml = await htmlResponse.text();

                // Insert the new comment HTML into the comments list
                commentsList.insertAdjacentHTML('beforeend', commentHtml);

                // Update the comment count
                const currentCount = parseInt(commentCount.textContent, 10);
                commentCount.textContent = currentCount + 1;

                // Attach the delete event listener to the new comment's delete button
                const newComment = commentsList.lastElementChild; // Get the newly added comment
                const deleteButton = newComment.querySelector(".delete-comment-button");
                if (deleteButton) {
                    deleteButton.addEventListener("click", async () => {
                        setupDeleteCommentButton(deleteButton);
                    });
                }
            } else {
                alert("Failed to post comment.");
            }
        } catch (error) {
            console.error(error);
            alert("Error posting comment.");
        }
    });
});

async function setupDeleteCommentButton(button) {
    const commentId = button.getAttribute("data-comment-id");

    if (confirm("Are you sure you want to delete this comment?")) {
        fetch(`/api/comments/${commentId}`, {
            method: "DELETE",
            headers: {
                "Content-Type": "application/json",
            },
            credentials: "same-origin"
        })
        .then(response => {
            if (response.ok) {
                // Remove the comment from the DOM
                const commentItem = button.closest(".list-group-item");
                commentItem.remove();

                // Update the comment count
                const currentCount = parseInt(commentCount.textContent, 10);
                commentCount.textContent = currentCount - 1;

            } else {
                alert("Failed to delete the comment. Please try again.");
            }
        })
        .catch(error => {
            console.error("Error deleting comment:", error);
            alert("An error occurred. Please try again.");
        });
    }
}
