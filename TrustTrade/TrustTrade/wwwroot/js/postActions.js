document.addEventListener("DOMContentLoaded", () => {
    // Confirmation for deleting a post
    document.querySelectorAll(".delete-post-form").forEach(form => {
        form.addEventListener("submit", (event) => {
            if (!confirm("Are you sure you want to delete this post?")) {
                event.preventDefault(); // Stop form submission if user cancels
            }
        });
    });
});
