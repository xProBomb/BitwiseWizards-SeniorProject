// Initialize the Plaid Link handler
let linkHandler = null;

async function initializePlaidLink() {
    try {
        const response = await fetch('/api/plaid/create-link-token', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            const errorData = await response.json();
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
                    const exchangeResponse = await fetch('/api/plaid/exchange-public-token', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify({ publicToken: public_token })
                    });

                    if (!exchangeResponse.ok) {
                        const errorData = await exchangeResponse.json();
                        console.error('Server error:', errorData);
                        throw new Error(errorData.error || 'Failed to exchange public token');
                    }

                    const exchangeData = await exchangeResponse.json();
                    if (exchangeData.success) {
                        // Show success message
                        alert('Successfully connected your brokerage account!');

                        // Optionally reload or update UI
                        window.location.reload();
                    } else {
                        throw new Error('Failed to process brokerage connection');
                    }
                } catch (error) {
                    console.error('Error exchanging public token:', error);
                    alert('There was an error connecting your account. Please try again.');
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

// Initialize when the page loads
document.addEventListener('DOMContentLoaded', () => {
    const plaidButton = document.getElementById('connect-plaid');
    if (plaidButton) {
        initializePlaidLink();
        plaidButton.addEventListener('click', openPlaidLink);
    }
});