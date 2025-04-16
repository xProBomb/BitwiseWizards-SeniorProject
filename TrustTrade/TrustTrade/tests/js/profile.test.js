/**
 * @jest-environment jsdom
 * @file tests/js/profile.test.js
 * @description Component tests for profile functionality in TrustTrade
 */

// Import the profile.js file directly
require('../../wwwroot/js/profile.js');

describe('Profile Components', () => {
    // Common variables
    let mockFetch;
    let originalLocation;

    // Setup before each test
    beforeEach(() => {
        // Reset document body
        document.body.innerHTML = '';

        // Mock console.error
        jest.spyOn(console, 'error').mockImplementation(() => { });

        // Mock fetch
        mockFetch = jest.fn().mockImplementation(() =>
            Promise.resolve({
                ok: true,
                json: () => Promise.resolve({ success: true })
            })
        );
        global.fetch = mockFetch;

        // Create test DOM
        document.body.innerHTML = `
      <div id="followersModal" style="display: block;">Followers Modal Content</div>
      <div id="followingModal" style="display: block;">Following Modal Content</div>
      <form class="follow-unfollow-form" action="/api/follow">
        <input type="hidden" name="userId" value="123">
        <button type="submit">Follow</button>
      </form>
      <form class="follow-unfollow-form" action="/api/unfollow">
        <input type="hidden" name="userId" value="456">
        <button type="submit">Unfollow</button>
      </form>
    `;

        // Mock window.location
        originalLocation = window.location;
        delete window.location;
        window.location = { reload: jest.fn() };

        // Load the profile.js script
        // Simulate DOMContentLoaded immediately rather than waiting
        document.dispatchEvent(new Event('DOMContentLoaded'));
    });

    // Clean up after each test
    afterEach(() => {
        jest.restoreAllMocks();
        window.location = originalLocation;
    });

    // -----------------------------------------------------------------------------
    // MODAL VISIBILITY TESTS
    // -----------------------------------------------------------------------------

    describe('Modal Visibility', () => {
        test('hides followers modal on page load', () => {
            const followersModal = document.getElementById('followersModal');
            expect(followersModal.style.display).toBe('none');
        });

        test('hides following modal on page load', () => {
            const followingModal = document.getElementById('followingModal');
            expect(followingModal.style.display).toBe('none');
        });
    });

    // -----------------------------------------------------------------------------
    // FOLLOW/UNFOLLOW FORM TESTS
    // -----------------------------------------------------------------------------

    describe('Follow/Unfollow Forms', () => {
        test('prevents default form submission', () => {
            const form = document.querySelector('form.follow-unfollow-form');
            const preventDefaultSpy = jest.spyOn(Event.prototype, 'preventDefault');

            // Simulate form submission
            form.dispatchEvent(new Event('submit'));

            expect(preventDefaultSpy).toHaveBeenCalled();
        });

        test('submits form data via fetch', () => {
            const form = document.querySelector('form.follow-unfollow-form');

            // Simulate form submission
            form.dispatchEvent(new Event('submit'));

            expect(mockFetch).toHaveBeenCalled();
            expect(mockFetch.mock.calls[0][0]).toBe(form.action);
        });

        test('prevents multiple submissions', () => {
            const form = document.querySelector('form.follow-unfollow-form');

            // Simulate form submission twice
            form.dispatchEvent(new Event('submit'));
            form.dispatchEvent(new Event('submit'));

            // Should only call fetch once
            expect(mockFetch).toHaveBeenCalledTimes(1);
        });

        test('reloads page on successful submission', async () => {
            const form = document.querySelector('form.follow-unfollow-form');

            // Simulate form submission
            form.dispatchEvent(new Event('submit'));

            // Allow fetch promise to resolve
            await new Promise(resolve => setTimeout(resolve, 0));

            expect(window.location.reload).toHaveBeenCalled();
        });

        test('handles fetch errors', async () => {
            // Mock fetch to reject
            global.fetch = jest.fn().mockImplementation(() =>
                Promise.reject(new Error('Network error'))
            );

            const form = document.querySelector('form.follow-unfollow-form');

            // Simulate form submission
            form.dispatchEvent(new Event('submit'));

            // Allow fetch promise to reject
            await new Promise(resolve => setTimeout(resolve, 0));

            expect(console.error).toHaveBeenCalled();
        });

        test('resets isSubmitting flag on fetch error', async () => {
            // Mock fetch to reject
            global.fetch = jest.fn().mockImplementation(() =>
                Promise.reject(new Error('Network error'))
            );

            const form = document.querySelector('form.follow-unfollow-form');

            // Simulate form submission
            form.dispatchEvent(new Event('submit'));

            // Allow fetch promise to reject
            await new Promise(resolve => setTimeout(resolve, 10));

            // After error, should be able to submit again
            global.fetch = mockFetch; // Reset fetch to succeed
            form.dispatchEvent(new Event('submit'));

            expect(mockFetch).toHaveBeenCalled();
        });
    });

    // -----------------------------------------------------------------------------
    // INTEGRATION TESTS
    // -----------------------------------------------------------------------------

    describe('Integration Tests', () => {
        test('follow-unfollow forms work independently', () => {
            const forms = document.querySelectorAll('form.follow-unfollow-form');

            // Submit first form
            forms[0].dispatchEvent(new Event('submit'));

            // First form should use first action URL
            expect(mockFetch.mock.calls[0][0]).toBe(forms[0].action);

            // Submit second form
            forms[1].dispatchEvent(new Event('submit'));

            // Second form should use second action URL
            expect(mockFetch.mock.calls[1][0]).toBe(forms[1].action);
        });
    });
});