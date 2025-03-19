// tests/js/ThemeSwitcher.test.js

/**
 * @jest-environment jsdom
 */

describe('ThemeSwitcher', () => {
    // Setup DOM elements before each test
    beforeEach(() => {
        // Create the HTML structure needed
        document.body.innerHTML = `
      <input type="checkbox" id="checkbox">
    `;

        // Mock localStorage
        Object.defineProperty(window, 'localStorage', {
            value: {
                getItem: jest.fn().mockImplementation((key) => {
                    return key === 'theme' ? 'dark' : null;
                }),
                setItem: jest.fn(),
            },
            writable: true
        });

        // Manually trigger the setup instead of relying on DOMContentLoaded
        // We'll need to save the original addEventListener
        const originalAddEventListener = document.addEventListener;
        let domContentLoadedHandler;

        // Override addEventListener to capture the DOMContentLoaded handler
        document.addEventListener = jest.fn((event, handler) => {
            if (event === 'DOMContentLoaded') {
                domContentLoadedHandler = handler;
            }
        });

        // Load the ThemeSwitcher script
        require('../../wwwroot/js/ThemeSwitcher.js');

        // Restore original addEventListener
        document.addEventListener = originalAddEventListener;

        // Manually call the captured handler if it exists
        if (domContentLoadedHandler) {
            domContentLoadedHandler();
        }
    });

    test('toggles theme when checkbox changes', () => {
        const checkbox = document.querySelector('#checkbox');

        // Verify initial state (should be dark based on localStorage mock)
        expect(checkbox.checked).toBe(false);

        // Simulate checking the checkbox
        checkbox.checked = true;
        checkbox.dispatchEvent(new Event('change'));

        // Verify theme was changed to light
        expect(document.documentElement.getAttribute('data-bs-theme')).toBe('light');
        expect(document.body.getAttribute('data-bs-theme')).toBe('light');
        expect(localStorage.setItem).toHaveBeenCalledWith('theme', 'light');

        // Simulate unchecking the checkbox
        checkbox.checked = false;
        checkbox.dispatchEvent(new Event('change'));

        // Verify theme was changed back to dark
        expect(document.documentElement.getAttribute('data-bs-theme')).toBe('dark');
        expect(document.body.getAttribute('data-bs-theme')).toBe('dark');
        expect(localStorage.setItem).toHaveBeenCalledWith('theme', 'dark');
    });
});