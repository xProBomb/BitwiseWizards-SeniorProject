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
});
