document.addEventListener("DOMContentLoaded", () => {
    // Make posts clickable to redirect to their respective pages
    document.getElementById("postFeedContainer").addEventListener("click", function (e) {
        const post = e.target.closest(".clickable-post");
        if (post) {
            window.location.href = post.dataset.url;
        }
    });

    let currentPage = 2; // Start from page 2 since page 1 is already loaded
    let category = document.getElementById('categoryFilter').value;
    let sortOrder = document.getElementById('sortOrder').value;
    let isLoading = false;

    // Set up infinite scroll for loading more posts
    const observer = new IntersectionObserver((entries) => {
        if (entries[0].isIntersecting) {
            loadMorePosts();
        }
    }, {
        rootMargin: '200px'
    });

    observer.observe(document.getElementById('loadTrigger'));

    async function loadMorePosts() {
        if (isLoading) return;

        const loadingElement = document.getElementById('loading');
        const endMessageElement = document.getElementById('endMessage');
        const postFeedContainer = document.getElementById('postFeedContainer');
        const username = postFeedContainer.dataset.username;
        const url = postFeedContainer.dataset.url;

        isLoading = true;
        loadingElement.style.display = 'block';

        const params = new URLSearchParams(
            {
                pageNumber: currentPage,
                categoryFilter: category,
                sortOrder: sortOrder
            }
        );

        let fetchUrl = `${url}?${params.toString()}`;
        if (username) {
            fetchUrl += `&username=${username}`;
        }

        const response = await fetch(fetchUrl);

        if (!response.ok) {
            console.error('Failed to load more posts');
            return;
        }

        const postsFeedHtml = await response.text();
        
        // Check if the response contains any posts
        if (postsFeedHtml.trim() === "") {
            endMessageElement.style.display = 'block';
            loadingElement.style.display = 'none';
            observer.disconnect();
            return;
        }

        postFeedContainer.insertAdjacentHTML('beforeend', postsFeedHtml);
        currentPage++;
        isLoading = false;
        loadingElement.style.display = 'none';
    }
});
