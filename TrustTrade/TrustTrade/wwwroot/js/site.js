
document.addEventListener('DOMContentLoaded', function() {
    // Initialize sidebar
    initSidebar();
    
    // Initialize tooltips
    initTooltips();
    
    // Add fade-in animation to main content
    addPageTransitions();
});

/**
 * Initialize sidebar functionality
 */
function initSidebar() {
    const sidebar = document.getElementById('sidebar');
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebarClose = document.getElementById('sidebarClose');
    const body = document.body;
    
    if (!sidebar || !sidebarToggle || !sidebarClose) return;
    
    // Toggle sidebar on button click
    sidebarToggle.addEventListener('click', function() {
        sidebar.classList.toggle('expanded');
        body.classList.toggle('sidebar-expanded');
    });
    
    // Close sidebar when X is clicked
    sidebarClose.addEventListener('click', function() {
        sidebar.classList.remove('expanded');
        body.classList.remove('sidebar-expanded');
    });
    
    // Handle window resize - MODIFIED to keep sidebar closed by default
    function handleResize() {
        // Always keep sidebar closed by default regardless of screen size
        sidebar.classList.remove('expanded');
        body.classList.remove('sidebar-expanded');
        
        // Just handle any additional responsive adjustments if needed
        if (window.innerWidth <= 991.98) {
            // Mobile-specific adjustments can go here
        }
    }
    
    // Initialize sidebar state based on screen size
    handleResize();
    
    // Update on resize
    window.addEventListener('resize', handleResize);
}

/**
 * Initialize Bootstrap tooltips
 */
function initTooltips() {
    // Check if Bootstrap's tooltip is available
    if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }
}

/**
 * Add page transitions for smoother UX
 */
function addPageTransitions() {
    const mainContent = document.querySelector('.main-container');
    if (mainContent) {
        mainContent.classList.add('fadeIn');
    }
}

/**
 * Handle profile dropdown menu positioning and interactions
 */
function setupProfileDropdown() {
    const profileDropdown = document.getElementById('profileDropdown');
    const dropdownMenu = document.querySelector('.dropdown-menu');
    
    if (!profileDropdown || !dropdownMenu) return;
    
    // Ensure dropdown menu stays within viewport
    profileDropdown.addEventListener('shown.bs.dropdown', function() {
        const rect = dropdownMenu.getBoundingClientRect();
        if (rect.bottom > window.innerHeight) {
            const moveUp = rect.bottom - window.innerHeight + 10;
            dropdownMenu.style.transform = `translateY(-${moveUp}px)`;
        }
    });
    
    profileDropdown.addEventListener('hidden.bs.dropdown', function() {
        dropdownMenu.style.transform = '';
    });
}