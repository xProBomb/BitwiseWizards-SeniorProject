// UPDATED VERSION - May, 2025
// Version 2.2 - with retry logic, improved error handling, and robust token exchange

// Add a version log to verify we're running the latest code
console.log("Running plaid.js version 2.2 - with enhanced error handling and retry logic");

// Initialize the Plaid Link handler
let linkHandler = null;

/**
 * Exchanges a public token with retry logic
 * @param {string} publicToken - The public token from Plaid Link
 * @param {number} maxRetries - Maximum number of retry attempts
 * @returns {Promise<Object>} - The response from the token exchange
 */
async function exchangeTokenWithRetry(publicToken, maxRetries = 3) {
    let retries = 0;

    while (retries < maxRetries) {
        try {
            console.log(`Attempt ${retries + 1} to exchange token`);

            const exchangeResponse = await fetch('/api/plaid/exchange-public-token', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({publicToken: publicToken})
            });

            // Get full error details if the response isn't successful
            if (!exchangeResponse.ok) {
                let errorData;
                try {
                    errorData = await exchangeResponse.json();
                } catch (e) {
                    const errorText = await exchangeResponse.text();
                    errorData = {
                        error: "Failed to parse error response",
                        rawText: errorText,
                        status: exchangeResponse.status
                    };
                }

                console.error('Server error:', errorData);
                throw new Error(errorData.error || 'Failed to exchange public token');
            }

            return await exchangeResponse.json();
        } catch (error) {
            retries++;
            console.error(`Token exchange failed, attempt ${retries}/${maxRetries}`, error);

            if (retries >= maxRetries) {
                throw error;
            }

            // Wait before retrying (exponential backoff)
            await new Promise(resolve => setTimeout(resolve, 1000 * retries));
        }
    }
}

/**
 * Safely refreshes holdings after successful token exchange
 * @returns {Promise<boolean>} - Success status of the refresh operation
 */
async function safelyRefreshHoldings() {
    try {
        console.log('Submitting refresh request to /Profile/RefreshHoldings');

        // Get any CSRF token that might be on the page
        const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
        const formData = new FormData();

        if (tokenElement) {
            formData.append('__RequestVerificationToken', tokenElement.value);
        }

        // Use fetch for the refresh request
        const response = await fetch('/Profile/RefreshHoldings', {
            method: 'POST',
            body: formData,
            credentials: 'same-origin',
            redirect: 'follow'
        });

        console.log('Refresh request completed with status:', response.status);

        if (response.redirected) {
            console.log('Redirecting to:', response.url);
            window.location.href = response.url;
            return true;
        } else {
            // If not redirected automatically, redirect to Profile
            console.log('No redirect detected, manually navigating to /Profile');
            window.location.href = '/Profile';
            return true;
        }
    } catch (error) {
        console.error('Error during holdings refresh:', error);
        // Still redirect to profile
        window.location.href = '/Profile';
        return false;
    }
}

async function initializePlaidLink() {
    try {
        const response = await fetch('/api/plaid/create-link-token', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            // Add cache-busting query parameter
            cache: 'no-store'
        });

        if (!response.ok) {
            let errorData;
            try {
                errorData = await response.json();
            } catch (e) {
                errorData = { error: 'Failed to parse error response' };
            }
            console.error('Error from server:', errorData);
            throw new Error(errorData.error || 'Failed to get link token');
        }

        const data = await response.json();

        if (!data.linkToken) {
            console.error('No link token in response:', data);
            throw new Error('Invalid server response - no link token');
        }

        // Initialize Plaid Link
        linkHandler = Plaid.create({
            token: data.linkToken,
            onSuccess: async (public_token, metadata) => {
                console.log('Link success - exchanging public token');
                try {
                    // Use the retry mechanism for token exchange
                    const exchangeData = await exchangeTokenWithRetry(public_token);

                    if (exchangeData.success) {
                        // Notification message to verify this code is running
                        alert('Connection successful! Your account is now linked. Refreshing your holdings...');

                        // Safely refresh holdings and handle navigation
                        await safelyRefreshHoldings();
                    } else {
                        throw new Error('Failed to process brokerage connection');
                    }
                } catch (error) {
                    console.error('Error exchanging public token:', error);

                    // More detailed error message
                    alert('There was an error connecting your account. Please try again later. ' +
                        (error.message || 'Unknown error occurred.'));
                }
            },
            onExit: (err, metadata) => {
                console.log('Link exit:', err, metadata);
                if (err != null) {
                    console.error('Link error:', err);
                    alert('Error connecting to Plaid. Please try again.');
                }
            },
            onEvent: (eventName, metadata) => {
                console.log('Link Event:', eventName, metadata);
            },
        });
    } catch (error) {
        console.error('Error initializing Plaid Link:', error);
        alert('Error initializing Plaid connection. Please try again later.');
    }
}

// Function to launch Plaid Link
function openPlaidLink() {
    if (linkHandler) {
        linkHandler.open();
    } else {
        console.error('Plaid Link not initialized');
        // Try to reinitialize
        initializePlaidLink().then(() => {
            if (linkHandler) {
                linkHandler.open();
            } else {
                alert('Unable to initialize Plaid connection. Please refresh the page and try again.');
            }
        });
    }
}

// Function to delete Plaid connection
async function deletePlaidConnection() {
    try {
        const confirmed = confirm("Are you sure you want to delete your brokerage connection? This will remove all your portfolio data.");

        if (!confirmed) {
            return false;
        }

        // Show loading state
        document.body.style.cursor = 'wait';

        const response = await fetch('/Brokerage/DeleteConnection', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            },
            credentials: 'same-origin'
        });

        if (!response.ok) {
            let errorData;
            try {
                errorData = await response.json();
            } catch (e) {
                const errorText = await response.text();
                errorData = {
                    error: "Failed to parse delete response",
                    rawText: errorText
                };
            }
            console.error('Error deleting connection:', errorData);
            throw new Error(errorData.error || 'Failed to delete connection');
        }

        const data = await response.json();
        return data.success;
    } catch (error) {
        console.error('Error deleting Plaid connection:', error);
        alert('Error deleting connection. Please try again. ' + (error.message || ''));
        return false;
    } finally {
        document.body.style.cursor = 'default';
    }
}

// Initialize when the page loads
document.addEventListener('DOMContentLoaded', () => {
    const connectPlaidButton = document.getElementById('connect-plaid');
    const updatePlaidButton = document.getElementById('update-plaid');
    const deletePlaidButton = document.getElementById('delete-plaid');

    // Initialize Plaid Link for any button that might use it
    if (connectPlaidButton || updatePlaidButton) {
        initializePlaidLink();
    }

    // Regular connection button (new users)
    if (connectPlaidButton) {
        connectPlaidButton.addEventListener('click', openPlaidLink);
    }

    // Update connection button (existing users)
    if (updatePlaidButton) {
        updatePlaidButton.addEventListener('click', async (event) => {
            event.preventDefault();

            // First delete existing connection
            const deleteSuccess = await deletePlaidConnection();

            if (deleteSuccess) {
                // Then open Plaid Link to create a new connection
                openPlaidLink();
            }
        });
    }

    // Delete connection button
    if (deletePlaidButton) {
        deletePlaidButton.addEventListener('click', async (event) => {
            event.preventDefault();

            // Delete connection
            const deleteSuccess = await deletePlaidConnection();

            // Refresh the page to show updated UI
            if (deleteSuccess) {
                window.location.reload();
            }
        });
    }
});