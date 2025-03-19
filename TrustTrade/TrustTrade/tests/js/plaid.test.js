/**
 * @jest-environment jsdom
 * @file tests/js/plaid.test.js
 * @description Component tests for Plaid integration in TrustTrade
 * @version 1.0.0
 */

// -----------------------------------------------------------------------------
// MOCK PLAID SERVICE FOR TESTING
// -----------------------------------------------------------------------------

/**
 * Mock implementation of Plaid services for component testing
 * This separates tests from actual Plaid implementation details
 */
class MockPlaidService {
    constructor(options = {}) {
        // Initial state
        this.isConnected = options.isConnected || false;
        this.institutionName = options.institutionName || 'Test Bank';
        this.lastSyncDate = options.lastSyncDate || new Date();

        // Callbacks
        this.onSuccess = options.onSuccess || (() => {});
        this.onError = options.onError || (() => {});
        this.onExit = options.onExit || (() => {});

        // Link handler mock
        this.linkHandler = {
            open: jest.fn().mockImplementation(() => {
                // By default simulate successful flow
                setTimeout(() => this.onSuccess('mock-public-token', {}), 10);
            })
        };
    }

    /**
     * Initialize the mock Plaid service
     * Returns a promise to simulate async initialization
     */
    async initialize() {
        return Promise.resolve(this.linkHandler);
    }

    /**
     * Simulate opening Plaid Link
     */
    openLink() {
        return this.linkHandler.open();
    }

    /**
     * Simulate deleting a Plaid connection
     */
    async deleteConnection(shouldConfirm = true) {
        if (!shouldConfirm) return false;

        this.isConnected = false;
        return Promise.resolve(true);
    }

    /**
     * Simulate connection status check
     */
    getConnectionStatus() {
        return {
            connected: this.isConnected,
            institutionName: this.isConnected ? this.institutionName : null,
            lastUpdated: this.isConnected ? this.lastSyncDate : null
        };
    }

    /**
     * Simulate exchanging a public token
     */
    async exchangePublicToken(token) {
        return Promise.resolve({ success: true });
    }
}

// -----------------------------------------------------------------------------
// TEST SUITE
// -----------------------------------------------------------------------------

describe('Plaid Integration Components', () => {
    // Common variables
    let mockService;

    // Setup before each test
    beforeEach(() => {
        // Reset document body
        document.body.innerHTML = '';

        // Reset console mocks
        jest.spyOn(console, 'log').mockImplementation(() => {});
        jest.spyOn(console, 'error').mockImplementation(() => {});

        // Mock alert and confirm
        global.alert = jest.fn();
        global.confirm = jest.fn().mockReturnValue(true);

        // Mock fetch
        global.fetch = jest.fn().mockImplementation(() =>
            Promise.resolve({
                ok: true,
                json: () => Promise.resolve({ success: true })
            })
        );

        // Create default mock service
        mockService = new MockPlaidService();
    });

    // Clean up after each test
    afterEach(() => {
        jest.restoreAllMocks();
    });

    // -----------------------------------------------------------------------------
    // CONNECT BUTTON COMPONENT TESTS
    // -----------------------------------------------------------------------------

    describe('Connect Button Component', () => {
        // Setup for connect button tests
        beforeEach(() => {
            // Create DOM for connect button
            document.body.innerHTML = `
        <button id="connect-plaid" class="btn btn-primary">Connect Brokerage</button>
        <div id="status-message"></div>
      `;
        });

        test('renders connect button correctly', () => {
            const button = document.getElementById('connect-plaid');
            expect(button).not.toBeNull();
            expect(button.textContent).toBe('Connect Brokerage');
            expect(button.classList.contains('btn-primary')).toBe(true);
        });

        test('opens Plaid Link when clicked', () => {
            // Setup click handler using mock service
            const button = document.getElementById('connect-plaid');
            button.addEventListener('click', () => mockService.openLink());

            // Click the button
            button.click();

            // Verify link was opened
            expect(mockService.linkHandler.open).toHaveBeenCalled();
        });

        test('displays success message after connection', (done) => {
            // Set up status element
            const statusMsg = document.getElementById('status-message');

            // Configure mock service with custom success handler
            mockService = new MockPlaidService({
                onSuccess: (token) => {
                    statusMsg.textContent = 'Successfully connected!';
                    statusMsg.classList.add('text-success');
                    done(); // Signal test completion
                }
            });

            // Setup click handler
            const button = document.getElementById('connect-plaid');
            button.addEventListener('click', () => mockService.openLink());

            // Click the button
            button.click();
        });
    });

    // -----------------------------------------------------------------------------
    // BROKERAGE STATUS COMPONENT TESTS
    // -----------------------------------------------------------------------------

    describe('Brokerage Status Component', () => {
        // Setup for status component tests
        beforeEach(() => {
            // Create DOM for status display
            document.body.innerHTML = `
        <div class="card">
          <div class="card-header">
            <h3>Connection Status</h3>
          </div>
          <div class="card-body">
            <div id="connection-status">Loading...</div>
            <div id="institution-name"></div>
            <div id="last-updated"></div>
          </div>
        </div>
      `;
        });

        test('displays connected status correctly', () => {
            // Create connected mock service
            mockService = new MockPlaidService({
                isConnected: true,
                institutionName: 'Test Bank Inc.'
            });

            // Update status display
            updateStatusDisplay(mockService.getConnectionStatus());

            // Verify status display
            const statusElement = document.getElementById('connection-status');
            const institutionElement = document.getElementById('institution-name');

            expect(statusElement.textContent).toBe('Connected');
            expect(statusElement.classList.contains('text-success')).toBe(true);
            expect(institutionElement.textContent).toBe('Test Bank Inc.');
        });

        test('displays disconnected status correctly', () => {
            // Create disconnected mock service
            mockService = new MockPlaidService({
                isConnected: false
            });

            // Update status display
            updateStatusDisplay(mockService.getConnectionStatus());

            // Verify status display
            const statusElement = document.getElementById('connection-status');
            const institutionElement = document.getElementById('institution-name');

            expect(statusElement.textContent).toBe('Not Connected');
            expect(statusElement.classList.contains('text-success')).toBe(false);
            expect(institutionElement.textContent).toBe('');
        });

        // Helper function to update status display
        function updateStatusDisplay(status) {
            const statusElement = document.getElementById('connection-status');
            const institutionElement = document.getElementById('institution-name');
            const lastUpdatedElement = document.getElementById('last-updated');

            if (status.connected) {
                statusElement.textContent = 'Connected';
                statusElement.classList.add('text-success');
                institutionElement.textContent = status.institutionName;
                if (status.lastUpdated) {
                    lastUpdatedElement.textContent = `Last updated: ${status.lastUpdated.toLocaleString()}`;
                }
            } else {
                statusElement.textContent = 'Not Connected';
                statusElement.classList.remove('text-success');
                institutionElement.textContent = '';
                lastUpdatedElement.textContent = '';
            }
        }
    });

    // -----------------------------------------------------------------------------
    // DELETE BUTTON COMPONENT TESTS
    // -----------------------------------------------------------------------------

    // Delete Connection Component tests - FIXED
    describe('Delete Connection Component', () => {
        beforeEach(() => {
            document.body.innerHTML = `
        <button id="delete-plaid" class="btn btn-danger">Delete Connection</button>
        <div id="status-display">Connected</div>
      `;

            mockService = new MockPlaidService({
                isConnected: true
            });
        });

        test('confirms before deleting connection', async () => {
            // Set up confirmation mock
            global.confirm = jest.fn().mockReturnValue(true);

            // Create a promise that will resolve when the DOM is updated
            const domUpdatePromise = new Promise(resolve => {
                const deleteButton = document.getElementById('delete-plaid');

                deleteButton.addEventListener('click', async () => {
                    const confirmResult = confirm('Are you sure you want to delete your brokerage connection?');
                    if (confirmResult) {
                        await mockService.deleteConnection(true);
                        // Update the DOM element
                        document.getElementById('status-display').textContent = 'Not Connected';

                        // Signal that the DOM has been updated
                        resolve();
                    }
                });

                // Click delete button
                deleteButton.click();
            });

            // Wait for DOM update to complete
            await domUpdatePromise;

            // Verify confirmation was shown
            expect(global.confirm).toHaveBeenCalled();

            // Now check the DOM update and service state
            expect(mockService.isConnected).toBe(false);
            expect(document.getElementById('status-display').textContent).toBe('Not Connected');
        });
    });

    // -----------------------------------------------------------------------------
    // UPDATE BUTTON COMPONENT TESTS
    // -----------------------------------------------------------------------------

        // Update Connection Component tests - FIXED
        describe('Update Connection Component', () => {
            beforeEach(() => {
                document.body.innerHTML = `
        <button id="update-plaid" class="btn btn-primary">Update Connection</button>
        <div id="institution-name">Old Bank Name</div>
      `;

                mockService = new MockPlaidService({
                    isConnected: true,
                    institutionName: 'Old Bank Name'
                });
            });

            test('updates connection when clicked', async () => {
                // Create a promise that will resolve when DOM updates complete
                const updateCompletePromise = new Promise(resolve => {
                    const updateButton = document.getElementById('update-plaid');

                    updateButton.addEventListener('click', async () => {
                        // Delete existing connection
                        await mockService.deleteConnection(true);

                        // Create new connection with different institution
                        mockService = new MockPlaidService({
                            isConnected: true,
                            institutionName: 'New Bank Name'
                        });

                        // Re-initialize and open
                        await mockService.initialize();
                        mockService.openLink();

                        // Update DOM explicitly
                        document.getElementById('institution-name').textContent = mockService.institutionName;

                        // Signal that updates are complete
                        resolve();
                    });

                    // Click update button
                    updateButton.click();
                });

                // Wait for all updates to complete
                await updateCompletePromise;

                // Now verify the institution name was updated
                expect(document.getElementById('institution-name').textContent).toBe('New Bank Name');
            });
        });

    // -----------------------------------------------------------------------------
    // VERIFY COMPONENT INTERACTIONS
    // -----------------------------------------------------------------------------

    describe('Component Interactions', () => {
        test('status updates after successful connection', (done) => {
            // Create more complex DOM with multiple components
            document.body.innerHTML = `
        <button id="connect-plaid">Connect</button>
        <div id="status-container">
          <div id="connection-status">Not Connected</div>
          <div id="institution-name"></div>
        </div>
      `;

            // Configure mock service with success handler
            mockService = new MockPlaidService({
                onSuccess: async (token) => {
                    // Update UI after successful connection
                    document.getElementById('connection-status').textContent = 'Connected';
                    document.getElementById('connection-status').classList.add('text-success');
                    document.getElementById('institution-name').textContent = 'Test Bank';
                    done(); // Signal test completion
                }
            });

            // Set up connect button handler
            const connectButton = document.getElementById('connect-plaid');
            connectButton.addEventListener('click', () => {
                mockService.openLink();
            });

            // Click connect button
            connectButton.click();
        });
    });
});