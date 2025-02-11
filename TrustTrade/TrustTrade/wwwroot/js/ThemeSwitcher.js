//Js file for theme changer
document.addEventListener('DOMContentLoaded', () => {
    const toggleSwitch = document.querySelector('#checkbox');

    // Default to dark theme if no preference is saved
    const currentTheme = localStorage.getItem('theme') ?? 'dark';
    document.documentElement.setAttribute('data-bs-theme', currentTheme);
    document.body.setAttribute('data-bs-theme', currentTheme);
    
    toggleSwitch.checked = currentTheme === 'light';

    // Handle theme toggle
    toggleSwitch.addEventListener('change', function () {
        if (this.checked) {
            document.documentElement.setAttribute('data-bs-theme', 'light');
            document.body.setAttribute('data-bs-theme', 'light');
            localStorage.setItem('theme', 'light');
        } else {
            document.documentElement.setAttribute('data-bs-theme', 'dark');
            document.body.setAttribute('data-bs-theme', 'dark');
            localStorage.setItem('theme', 'dark');
        }
    });
});