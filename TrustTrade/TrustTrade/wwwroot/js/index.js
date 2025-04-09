document.addEventListener("DOMContentLoaded", () => {
    // Make posts clickable to redirect to their respective pages
    document.querySelectorAll(".clickable-post").forEach(post => {
        post.addEventListener("click", () => {
            window.location.href = post.dataset.url;
        });
    });
});
