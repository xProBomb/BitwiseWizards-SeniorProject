// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener('DOMContentLoaded', function () {
    var followersModal = document.getElementById('followersModal');

    if (followersModal) {
        followersModal.style.display = 'none';
    }

    var followingModal = document.getElementById('followingModal');

    if (followingModal) {
        followingModal.style.display = 'none';
    }

    document.querySelectorAll('form.follow-unfollow-form').forEach(function (form) {
        let isSubmitting = false; // Flag to prevent multiple submissions

        form.addEventListener('submit', function (e) {
            e.preventDefault();

            if (isSubmitting) {
                return; // Prevent multiple submissions
            }

            isSubmitting = true; // Set the flag to true

            var actionUrl = form.action;
            var formData = new FormData(form);

            fetch(actionUrl, {
                method: 'POST',
                body: formData
            })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                return response.json();
            })
            .then(data => {
                // Refresh the page after successful follow/unfollow
                window.location.reload();
            })
            .catch(error => {
                console.error('There was a problem with the fetch operation:', error);
                isSubmitting = false; // Reset the flag in case of error
            });
        });
    });
});
