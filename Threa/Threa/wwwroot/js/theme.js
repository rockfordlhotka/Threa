// Threa Theme Management
// Handles applying and transitioning between fantasy and sci-fi themes

window.threaTheme = {
    // Apply theme to document
    apply: function(theme) {
        const validThemes = ['fantasy', 'scifi'];
        const selectedTheme = validThemes.includes(theme) ? theme : 'fantasy';

        document.documentElement.setAttribute('data-theme', selectedTheme);

        // Store in sessionStorage for persistence during session
        sessionStorage.setItem('threa-theme', selectedTheme);

        console.log(`[Threa] Theme applied: ${selectedTheme}`);
    },

    // Get current theme
    get: function() {
        return document.documentElement.getAttribute('data-theme') || 'fantasy';
    },

    // Initialize theme from storage or default
    init: function() {
        const storedTheme = sessionStorage.getItem('threa-theme');
        if (storedTheme) {
            this.apply(storedTheme);
        }
    }
};

// Auto-initialize on page load
document.addEventListener('DOMContentLoaded', function() {
    window.threaTheme.init();
});
