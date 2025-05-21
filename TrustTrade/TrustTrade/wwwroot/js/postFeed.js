document.addEventListener("DOMContentLoaded", () => {
    // Make posts clickable to redirect to their respective pages
    document.getElementById("postFeedContainer").addEventListener("click", function (e) {
        const post = e.target.closest(".clickable-post");
        if (post) {
            window.location.href = post.dataset.url;
        }
    });


    let currentPage = 2; // Start from page 2 since page 1 is already loaded
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

        isLoading = true;
        document.getElementById('loading').style.display = 'block';
        category = document.getElementById('categoryFilter').value;

        const response = await fetch(`/posts/loadmore?pageNumber=${currentPage}&categoryFilter=${category}`)

        if (!response.ok) {
            console.error('Failed to load more posts');
            return;
        }

        const postsFeedHtml = await response.text();
        
        if (postsFeedHtml.trim() === "") {
            document.getElementById('endMessage').style.display = 'block';
            document.getElementById('loading').style.display = 'none';
            observer.disconnect();
            return;
        }

        document.getElementById('postFeedContainer').insertAdjacentHTML('beforeend', postsFeedHtml);

        currentPage++;
        isLoading = false;
        console.log('Loaded more posts:', currentPage);
        document.getElementById('loading').style.display = 'none';
    }
});
