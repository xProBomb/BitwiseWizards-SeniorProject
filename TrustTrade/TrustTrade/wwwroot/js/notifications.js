document.addEventListener('DOMContentLoaded', function () {
    // Initialize notification system if logged in(notification menu displays)
    const notificationDropdown = document.getElementById('notificationDropdown');
    if (notificationDropdown) {
        initNotifications();

        // Handle mark all as read button
        const markAllReadBtn = document.getElementById('markAllReadBtn');
        if (markAllReadBtn) {
            markAllReadBtn.addEventListener('click', markAllAsRead);
        }

        // Handle notification item clicks
        document.addEventListener('click', function (event) {
            const notificationItem = event.target.closest('.notification-item');
            if (notificationItem) {
                event.preventDefault();
                const notificationId = notificationItem.dataset.id;
                markAsRead(notificationId);

                // Handle navigation based on notification type
                // For now, just redirect to notifications page
                window.location.href = '/Notifications';
            }
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
        })
        .catch(error => {
            console.error('Error fetching notifications:', error);
            notificationsContent.innerHTML = '<div class="p-3 text-center text-danger">Failed to load notifications</div>';
        });
}

// Function to mark a single notification as read
function markAsRead(notificationId) {
    fetch('/Notifications/MarkAsRead', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: JSON.stringify({id: notificationId})
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Update UI to show notification as read
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
        })
        .catch(error => console.error('Error marking notification as read:', error));
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
                // Update UI to show all notifications as read
                document.querySelectorAll('.notification-item.unread').forEach(notification => {
                    notification.classList.remove('unread');
                    notification.classList.add('read');

                    // Remove badges
                    const badge = notification.querySelector('.badge');
                    if (badge) {
                        badge.remove();
                    }
                });

                // Update notification count (should be 0)
                const badge = document.getElementById('notificationBadge');
                if (badge) {
                    badge.classList.add('d-none');
                }
            }
        })
        .catch(error => console.error('Error marking all notifications as read:', error));
}

// Handle archive button clicks
document.addEventListener('click', function (event) {
    if (event.target.closest('.archive-btn')) {
        const button = event.target.closest('.archive-btn');
        const notificationId = button.dataset.id;
        archiveNotification(notificationId);
    }
});

// Function to archive a notification
function archiveNotification(notificationId) {
    if (!confirm('Are you sure you want to remove this notification?')) {
        return;
    }

    fetch('/Notifications/Archive', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: JSON.stringify({id: notificationId})
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Remove the notification from the UI
                const notification = document.querySelector(`.notification-item[data-id="${notificationId}"]`);
                if (notification) {
                    notification.remove();

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
                }

                // Update notification count
                updateNotificationCount();
            } else {
                alert('Failed to remove notification. Please try again.');
            }
        })
        .catch(error => console.error('Error archiving notification:', error));
}

// Handle "Archive All" button click
document.getElementById('archiveAllBtn')?.addEventListener('click', function () {
    if (!confirm('Are you sure you want to clear all notifications? This cannot be undone.')) {
        return;
    }

    fetch('/Notifications/ArchiveAll', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Reload the page to show the empty state
                window.location.reload();
            } else {
                alert('Failed to clear notifications. Please try again.');
            }
        })
        .catch(error => console.error('Error clearing all notifications:', error));
});