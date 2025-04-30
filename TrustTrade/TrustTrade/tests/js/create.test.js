/**
 * @jest-environment jsdom
 */

describe('Form Submission Behavior', () => {
    beforeEach(() => {
        // Set up the DOM structure
        document.body.innerHTML = `
            <form>
                <button type="submit" class="btn btn-primary">Submit</button>
            </form>
        `;

        // Simulate the script behavior
        document.addEventListener('DOMContentLoaded', function () {
            const form = document.querySelector('form');
            const submitButton = form.querySelector('button[type="submit"]');

            if (form && submitButton) {
                form.addEventListener('submit', function (event) {
                    // Prevent default form submission for testing
                    event.preventDefault();

                    // Disable the submit button to prevent multiple submissions
                    submitButton.disabled = true;

                    // Optionally, you can add a loading spinner or change button text
                    submitButton.innerHTML = '<i class="bi bi-hourglass-split"></i> Submitting...';
                });
            }
        });

        // Trigger DOMContentLoaded
        document.dispatchEvent(new Event('DOMContentLoaded'));
    });

    test('disables the submit button and updates its text on form submission', () => {
        const form = document.querySelector('form');
        const submitButton = form.querySelector('button[type="submit"]');

        // Simulate form submission
        form.dispatchEvent(new Event('submit'));

        // Check if the button is disabled
        expect(submitButton.disabled).toBe(true);

        // Check if the button text is updated
        expect(submitButton.innerHTML).toBe('<i class="bi bi-hourglass-split"></i> Submitting...');
    });
});