
        // Sidebar functionality
        document.addEventListener('DOMContentLoaded', function() {
            const sidebar = document.getElementById('sidebar');
            const sidebarToggle = document.getElementById('sidebarToggle');
            const sidebarClose = document.getElementById('sidebarClose');
            const body = document.body;
            
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
            
            // Handle window resize
            function handleResize() {
                if (window.innerWidth > 991.98) {
                    // For desktop: if sidebar is supposed to be visible
                    sidebar.classList.add('expanded');
                    body.classList.add('sidebar-expanded');
                } else {
                    // For mobile: hide sidebar by default
                    sidebar.classList.remove('expanded');
                    body.classList.remove('sidebar-expanded');
                }
            }
            
            // Initialize sidebar state based on screen size
            handleResize();
            
            // Update on resize
            window.addEventListener('resize', handleResize);
        });
