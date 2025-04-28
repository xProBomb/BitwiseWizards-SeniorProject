document.addEventListener("DOMContentLoaded", () => {
    const contentInput = document.getElementById('content');
    const postId = document.getElementById('postId').value;
    const submitButtonWrapper = document.getElementById('submitButtonWrapper');
    const submitCommentButton = document.getElementById('submitCommentButton');
    const commentsList = document.querySelector('.list-group');

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

    submitCommentButton.addEventListener('click', async () => {
        const content = contentInput.value.trim();

        if (!content) {
            alert("Comment cannot be empty!");
            return;
        }

        const comment = {
            Content: content
        };

        try {
            const response = await fetch(`/api/posts/${postId}/comments`, {
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

                console.log("Comment ID:", commentId);

                // Get the new comment HTML from the server
                const htmlResponse = await fetch(`/comments/rendercommentpartial/${commentId}`, {
                    method: 'GET'
                });

                if (!htmlResponse.ok) {
                    // TODO: Display error message to the user
                    return;
                }

                const commentHtml = await htmlResponse.text();

                // Insert the new comment HTML into the comments list
                commentsList.insertAdjacentHTML('beforeend', commentHtml);

            } else {
                alert("Failed to post comment.");
            }
        } catch (error) {
            console.error(error);
            alert("Error posting comment.");
        }
    });
});
