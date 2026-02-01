"use strict";
/**
 * Dev 3 Notification UI Helper
 * Handles visibility of notification bell and other UI elements specific to Dev 3 features.
 * This file is created to avoid modifying shared 'auth.js' or 'site.css'.
 */

document.addEventListener('DOMContentLoaded', function () {
    // Check periodically for auth state or if element is hidden
    // We assume auth.js sets a session or cookie, or we can check the API
    checkAuthAndToggleBell();

    // Also listen for any potential custom events if we add them later
    // Or just interval check for simplicity as auth.js updates DOM classes
});

async function checkAuthAndToggleBell() {
    const bell = document.getElementById('notificationBell');
    if (!bell) return;

    // 1. Check if user is already logged in via checking "userDropdown" visibility
    // This is a heuristic: if userDropdown is visible, user is logged in.
    const userDropdown = document.getElementById('userDropdown');

    // Observer to watch for class changes on userDropdown done by auth.js
    if (userDropdown) {
        const observer = new MutationObserver((mutations) => {
            mutations.forEach((mutation) => {
                if (mutation.type === 'attributes' && mutation.attributeName === 'class') {
                    if (!userDropdown.classList.contains('d-none')) {
                        bell.classList.remove('d-none');
                    } else {
                        bell.classList.add('d-none');
                    }
                }
            });
        });

        observer.observe(userDropdown, { attributes: true });

        // Initial check
        if (!userDropdown.classList.contains('d-none')) {
            bell.classList.remove('d-none');
        }
    }
}
