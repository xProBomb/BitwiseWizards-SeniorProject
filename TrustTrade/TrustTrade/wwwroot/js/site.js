
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

    // ✅ Only open sidebar by default on Home/Index
    const path = window.location.pathname.toLowerCase();
    const isHomePage = path === "/" || path === "/home" || path === "/home/index";

    if (isHomePage) {
        sidebar.classList.add('expanded');
        body.classList.add('sidebar-expanded');
    }

    // Toggle sidebar on button click
    sidebarToggle.addEventListener('click', function () {
        sidebar.classList.toggle('expanded');
        body.classList.toggle('sidebar-expanded');
    });

    // Close sidebar when X is clicked
    sidebarClose.addEventListener('click', function () {
        sidebar.classList.remove('expanded');
        body.classList.remove('sidebar-expanded');
    });
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