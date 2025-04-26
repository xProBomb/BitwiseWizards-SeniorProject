document.addEventListener('DOMContentLoaded', function () {
    // Initialize notification system if logged in (notification menu displays)
    const notificationDropdown = document.getElementById('notificationDropdown');
    if (notificationDropdown) {
        initNotifications();

        // Handle mark all as read button on main page
        const markAllAsReadBtn = document.getElementById('markAllAsReadBtn');
        if (markAllAsReadBtn) {
            markAllAsReadBtn.addEventListener('click', markAllAsRead);
        }
    }

    // Handle archive button clicks on history page
    const archiveButtons = document.querySelectorAll('.archive-btn');
    if (archiveButtons.length > 0) {
        archiveButtons.forEach(button => {
            button.addEventListener('click', function(e) {
                e.preventDefault();
                e.stopPropagation();
                const notificationId = this.getAttribute('data-id');
                archiveNotification(notificationId);
            });
        });
    }

    // Handle "Archive All" button on history page
    const archiveAllBtn = document.getElementById('archiveAllBtn');
    if (archiveAllBtn) {
        archiveAllBtn.addEventListener('click', function(e) {
            e.preventDefault();
            archiveAllNotifications();
        });
    }
});

// Function to initialize notifications
function initNotifications() {
    // Load initial unread count
    updateNotificationCount();

    // Load notifications dropdown content when clicked
    const notificationDropdown = document.getElementById('notificationDropdown');
    if (notificationDropdown) {
        notificationDropdown.addEventListener('show.bs.dropdown', function () {
            fetchNotificationsDropdown();
        });
    }

    // Set up periodic refresh (every 30 seconds)
    setInterval(updateNotificationCount, 30000);
}

// Function to update the notification count badge
function updateNotificationCount() {
    fetch('/Notifications/GetUnreadCount')
        .then(response => response.json())
        .then(data => {
            const badge = document.getElementById('notificationBadge');
            const countElement = document.getElementById('notificationCount');

            if (badge && countElement) {
                if (data.count > 0) {
                    countElement.textContent = data.count > 99 ? '99+' : data.count;
                    badge.classList.remove('d-none');
                } else {
                    badge.classList.add('d-none');
                }
            }
        })
        .catch(error => console.error('Error fetching notification count:', error));
}

// Function to fetch notifications for the dropdown
function fetchNotificationsDropdown() {
    const notificationsContent = document.getElementById('notificationsContent');

    if (!notificationsContent) return;

    fetch('/Notifications/GetNotificationsDropdown')
        .then(response => response.text())
        .then(html => {
            notificationsContent.innerHTML = html;

            // Add event listeners to the newly loaded content
            const markAllReadBtn = document.getElementById('markAllReadBtn');
            if (markAllReadBtn) {
                markAllReadBtn.addEventListener('click', function (event) {
                    event.preventDefault();
                    markAllAsRead();
                });
            }

            // Add click handlers to notification items in dropdown
            document.querySelectorAll('#notificationsContent .notification-item').forEach(item => {
                item.addEventListener('click', function(e) {
                    // Don't prevent default here to allow navigation to the target
                    const notificationId = this.dataset.id;
                    markAsRead(notificationId);
                    // Let the native navigation happen via the href
                });
            });
        })
        .catch(error => {
            console.error('Error fetching notifications:', error);
            notificationsContent.innerHTML = '<div class="p-3 text-center text-danger">Failed to load notifications</div>';
        });
}

// Function to mark a single notification as read
function markAsRead(notificationId) {
    // No need to send a request if we're just clicking to navigate
    // The RedirectToContent action will already mark it as read on the server side

    // We just need to update the UI to reflect that this notification is now read
    const notification = document.querySelector(`.notification-item[data-id="${notificationId}"]`);
    if (notification) {
        notification.classList.remove('unread');
        notification.classList.add('read');

        // Remove the badge if present
        const badge = notification.querySelector('.badge');
        if (badge) {
            badge.remove();
        }
    }

    // Update notification count
    updateNotificationCount();
}

// Function to mark all notifications as read
function markAllAsRead(event) {
    if (event) {
        event.preventDefault();
    }

    fetch('/Notifications/MarkAllAsRead', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Smoothly fade out all notification items
                const notifications = document.querySelectorAll('.notification-item');
                if (notifications.length > 0) {
                    notifications.forEach(notification => {
                        notification.style.transition = 'opacity 0.5s ease, height 0.5s ease, margin 0.5s ease, padding 0.5s ease';
                        notification.style.opacity = '0';
                        notification.style.height = '0';
                        notification.style.margin = '0';
                        notification.style.padding = '0';
                        notification.style.overflow = 'hidden';
                    });

                    // After animation, replace with empty state message
                    setTimeout(() => {
                        const notificationList = document.querySelector('.notification-list');
                        if (notificationList) {
                            notificationList.innerHTML = `
                            <div class="alert alert-info">
                                <i class="bi bi-bell"></i> You don't have any notifications yet.
                            </div>
                            `;
                        }
                    }, 500);
                }

                // Update notification count (should be 0)
                updateNotificationCount();

                // Show success message
                const container = document.querySelector('.container');
                if (container) {
                    const successAlert = document.createElement('div');
                    successAlert.className = 'alert alert-success alert-dismissible fade show';
                    successAlert.innerHTML = `
                        <i class="bi bi-check-circle-fill"></i> All notifications have been marked as read.
                        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
                    `;

                    // Insert alert before the first child of container
                    container.insertBefore(successAlert, container.firstChild);

                    // Auto-dismiss after 5 seconds
                    setTimeout(() => {
                        successAlert.classList.remove('show');
                        setTimeout(() => successAlert.remove(), 150);
                    }, 5000);
                    
                }
            }
        })
        .catch(error => console.error('Error marking all notifications as read:', error));
}

// Function to archive a notification
function archiveNotification(notificationId) {
    if (!confirm('Are you sure you want to remove this notification?')) {
        return;
    }

    // Create form data
    const formData = new FormData();
    formData.append('id', notificationId);

    fetch('/Notifications/ArchiveNotification', {
        method: 'POST',
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: formData
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(data => {
            if (data.success) {
                // Find the notification card element
                const notificationCard = document.querySelector(`.notification-item[data-id="${notificationId}"]`);
                if (notificationCard) {
                    // Use animation for smooth removal
                    notificationCard.style.transition = 'opacity 0.3s ease, height 0.3s ease, margin 0.3s ease, padding 0.3s ease';
                    notificationCard.style.opacity = '0';
                    notificationCard.style.height = '0';
                    notificationCard.style.margin = '0';
                    notificationCard.style.padding = '0';
                    notificationCard.style.overflow = 'hidden';

                    // Remove element after animation completes
                    setTimeout(() => {
                        notificationCard.remove();

                        // Check if any notifications remain
                        const remainingNotifications = document.querySelectorAll('.notification-item');
                        if (remainingNotifications.length === 0) {
                            // Show "no notifications" message
                            const notificationList = document.querySelector('.notification-list');
                            if (notificationList) {
                                notificationList.innerHTML = `
                            <div class="alert alert-info">
                                <i class="bi bi-info-circle"></i> You don't have any notifications in your history.
                            </div>
                        `;
                            }
                        }
                    }, 300);
                }
            } else {
                alert('Failed to remove notification. Please try again.');
            }
        })
        .catch(error => {
            console.error('Error archiving notification:', error);
            alert('An error occurred while trying to archive the notification.');
        });
}

// Function to archive all notifications
function archiveAllNotifications() {
    if (!confirm('Are you sure you want to clear all notifications? This cannot be undone.')) {
        return;
    }

    fetch('/Notifications/ArchiveAllNotifications', {
        method: 'POST',
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(data => {
            if (data.success) {
                // Smoothly fade out all notification items
                const notifications = document.querySelectorAll('.notification-item');
                if (notifications.length > 0) {
                    notifications.forEach(notification => {
                        notification.style.transition = 'opacity 0.5s ease, height 0.5s ease, margin 0.5s ease, padding 0.5s ease';
                        notification.style.opacity = '0';
                        notification.style.height = '0';
                        notification.style.margin = '0';
                        notification.style.padding = '0';
                        notification.style.overflow = 'hidden';
                    });

                    // After animation, replace with empty state message
                    setTimeout(() => {
                        const notificationList = document.querySelector('.notification-list');
                        if (notificationList) {
                            notificationList.innerHTML = `
                        <div class="alert alert-info">
                            <i class="bi bi-info-circle"></i> You don't have any notifications in your history.
                        </div>
                    `;
                        }
                    }, 500);
                }

                // Show success message
                const container = document.querySelector('.container');
                if (container) {
                    const successAlert = document.createElement('div');
                    successAlert.className = 'alert alert-success alert-dismissible fade show';
                    successAlert.innerHTML = `
                    <i class="bi bi-check-circle-fill"></i> All notifications have been cleared.
                    <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
                `;

                    // Insert alert before the first child of container
                    container.insertBefore(successAlert, container.firstChild);

                    // Auto-dismiss after 5 seconds
                    setTimeout(() => {
                        successAlert.classList.remove('show');
                        setTimeout(() => successAlert.remove(), 150);
                    }, 5000);
                }
            } else {
                alert('Failed to clear notifications. Please try again.');
            }
        })
        .catch(error => {
            console.error('Error clearing all notifications:', error);
            alert('An error occurred while trying to clear all notifications.');
        });
}