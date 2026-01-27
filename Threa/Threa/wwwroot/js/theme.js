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
// Handle both initial load and cases where DOM is already loaded
// (e.g., when script loads after Blazor hydration or enhanced navigation)
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', function() {
        window.threaTheme.init();
    });
} else {
    // DOM already loaded, init immediately
    window.threaTheme.init();
}

// Bootstrap Tooltip Management
// Handles initializing tooltips on dynamically rendered Blazor content

// Initialize Bootstrap tooltips on dynamically rendered elements
window.initializeTooltips = function() {
    // Dispose existing tooltips to prevent duplicates
    var existingTooltips = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    existingTooltips.forEach(function(el) {
        var existingInstance = bootstrap.Tooltip.getInstance(el);
        if (existingInstance) {
            existingInstance.dispose();
        }
    });

    // Initialize new tooltips
    var tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    tooltipTriggerList.forEach(function(el) {
        new bootstrap.Tooltip(el, {
            container: 'body',
            trigger: 'hover'
        });
    });
};

// Reinitialize tooltips after a delay (useful for Blazor re-renders)
window.reinitializeTooltipsAfterDelay = function(delayMs) {
    setTimeout(function() {
        window.initializeTooltips();
    }, delayMs || 100);
};
