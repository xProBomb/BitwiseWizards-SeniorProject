// Chat functionality with signalR
document.addEventListener('DOMContentLoaded', function() {
    // Only initialize if we're on a chat page
    if (document.getElementById('chatMessages') || document.getElementById('chatInbox')) {
        initChatNotifications();
    }
});

function initChatNotifications() {
    // Connect to SignalR hub
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
        .withAutomaticReconnect()
        .build();

    // Handle notifications for new messages
    connection.on('ReceiveNotification', function(conversationId) {
        // Update the unread count
        updateUnreadCount();

        // If we're in the inbox, update the conversation list
        if (document.getElementById('chatInbox')) {
            updateConversationList();
        }
    });

    // Start connection
    connection.start()
        .then(function() {
            console.log('Connected to chat notifications');
        })
        .catch(function(err) {
            console.error('Error connecting to chat notifications:', err);
        });

    // Update unread message count
    function updateUnreadCount() {
        fetch('/Chat/GetUnreadCount')
            .then(response => response.json())
            .then(data => {
                const badge = document.getElementById('chatBadge');
                if (badge) {
                    if (data.count > 0) {
                        badge.textContent = data.count > 99 ? '99+' : data.count;
                        badge.classList.remove('d-none');
                    } else {
                        badge.classList.add('d-none');
                    }
                }
            })
            .catch(error => console.error('Error fetching unread count:', error));
    }

    // Update conversation list in inbox
    function updateConversationList() {
        const chatInbox = document.getElementById('chatInbox');
        if (chatInbox) {
            fetch('/Chat/Index')
                .then(response => response.text())
                .then(html => {
                    const parser = new DOMParser();
                    const doc = parser.parseFromString(html, 'text/html');
                    const newInbox = doc.getElementById('chatInbox');
                    if (newInbox) {
                        chatInbox.innerHTML = newInbox.innerHTML;
                    }
                })
                .catch(error => console.error('Error updating conversation list:', error));
        }
    }

    // Initial update
    updateUnreadCount();

    // Set interval to update unread count
    setInterval(updateUnreadCount, 30000);
}