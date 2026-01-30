// Authentication API utilities

const AUTH_API = {
    LOGIN: '/api/auth/login',
    REGISTER: '/api/auth/register',
    LOGOUT: '/api/auth/logout',
    CURRENT_USER: '/api/auth/current-user',
    CHECK_SESSION: '/api/auth/check-session'
};

// Global user state
let currentUser = null;

// Initialize authentication on page load
async function initAuth() {
    try {
        const response = await fetch(AUTH_API.CURRENT_USER);
        if (response.ok) {
            const data = await response.json();
            if (data.success && data.user) {
                currentUser = data.user;
                updateUIForLoggedInUser();
                return true;
            }
        }
    } catch (error) {
        console.error('Error initializing auth:', error);
    }
    currentUser = null;
    updateUIForLoggedOutUser();
    return false;
}

// Login function
async function login(email, password) {
    try {
        const response = await fetch(AUTH_API.LOGIN, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ email, password })
        });

        const data = await response.json();

        if (data.success) {
            currentUser = data.user;
            updateUIForLoggedInUser();
            return { success: true, message: data.message, user: data.user };
        } else {
            return { success: false, message: data.message };
        }
    } catch (error) {
        console.error('Login error:', error);
        return { success: false, message: 'Đã xảy ra lỗi khi đăng nhập' };
    }
}

// Register function
async function register(registerData) {
    try {
        const response = await fetch(AUTH_API.REGISTER, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(registerData)
        });

        const data = await response.json();

        if (data.success) {
            currentUser = data.user;
            updateUIForLoggedInUser();
            return { success: true, message: data.message, user: data.user };
        } else {
            return { success: false, message: data.message };
        }
    } catch (error) {
        console.error('Register error:', error);
        return { success: false, message: 'Đã xảy ra lỗi khi đăng ký' };
    }
}

// Logout function
async function logout() {
    try {
        const response = await fetch(AUTH_API.LOGOUT, {
            method: 'POST'
        });

        const data = await response.json();

        if (data.success) {
            currentUser = null;
            updateUIForLoggedOutUser();
            window.location.href = '/';
            return { success: true, message: data.message };
        } else {
            return { success: false, message: data.message };
        }
    } catch (error) {
        console.error('Logout error:', error);
        return { success: false, message: 'Đã xảy ra lỗi khi đăng xuất' };
    }
}

// Check if user is logged in
function isLoggedIn() {
    return currentUser !== null;
}

// Get current user
function getCurrentUser() {
    return currentUser;
}

// Check user role
function hasRole(role) {
    if (!currentUser) return false;
    if (role === 'admin') return currentUser.userType === 'admin';
    if (role === 'landlord') return ['landlord', 'admin'].includes(currentUser.userType);
    if (role === 'tenant') return currentUser.userType === 'tenant';
    return false;
}

// Role access control with redirect
function requireRole(requiredRole, redirectUrl = '/') {
    if (!isLoggedIn()) {
        alert('Vui lòng đăng nhập để truy cập trang này.');
        window.location.href = redirectUrl;
        return false;
    }

    if (requiredRole === 'renter' && !hasRole('renter')) {
        alert('Chỉ người thuê mới có thể truy cập trang này.');
        window.location.href = redirectUrl;
        return false;
    }

    if (requiredRole === 'landlord' && !hasRole('landlord')) {
        alert('Chỉ chủ trọ và quản trị viên mới có thể truy cập trang này.');
        window.location.href = redirectUrl;
        return false;
    }

    if (requiredRole === 'admin' && !hasRole('admin')) {
        alert('Chỉ quản trị viên mới có thể truy cập trang này.');
        window.location.href = redirectUrl;
        return false;
    }

    return true;
}

// Update UI for logged in user
function updateUIForLoggedInUser() {
    if (!currentUser) return;

    // Ẩn role selector
    const roleSelector = document.getElementById('roleSelector');
    if (roleSelector) {
        roleSelector.classList.add('d-none');
    }

    // Hiện/ẩn nút landlord dựa trên role
    const landlordBtn = document.getElementById('landlordBtn');
    if (landlordBtn) {
        if (hasRole('landlord')) {
            landlordBtn.classList.remove('d-none');
        } else {
            landlordBtn.classList.add('d-none');
        }
    }

    // Cập nhật user info trong navbar
    const userNameElement = document.getElementById('userName');
    if (userNameElement) {
        userNameElement.textContent = currentUser.fullName || currentUser.email;
    }

    const userAvatarElement = document.getElementById('userAvatar');
    if (userAvatarElement && currentUser.avatarUrl) {
        userAvatarElement.src = currentUser.avatarUrl;
    }

    // Hiện dropdown user, ẩn login/register buttons
    const userDropdown = document.getElementById('userDropdown');
    const authButtons = document.getElementById('authButtons');
    if (userDropdown) userDropdown.classList.remove('d-none');
    if (authButtons) authButtons.classList.add('d-none');

    // Add landlord view mode class if needed
    if (hasRole('landlord')) {
        document.body.classList.add('landlord-view-mode');
    }
}

// Update UI for logged out user
function updateUIForLoggedOutUser() {
    // Hiện role selector nếu chưa chọn
    const roleSelector = document.getElementById('roleSelector');
    if (roleSelector) {
        roleSelector.classList.remove('d-none');
    }

    // Ẩn nút landlord
    const landlordBtn = document.getElementById('landlordBtn');
    if (landlordBtn) {
        landlordBtn.classList.add('d-none');
    }

    // Hiện login/register buttons, ẩn user dropdown
    const userDropdown = document.getElementById('userDropdown');
    const authButtons = document.getElementById('authButtons');
    if (userDropdown) userDropdown.classList.add('d-none');
    if (authButtons) authButtons.classList.remove('d-none');

    // Remove landlord view mode class
    document.body.classList.remove('landlord-view-mode');
}

// Page protection
function protectPage(requiredRole) {
    document.addEventListener('DOMContentLoaded', async function () {
        await initAuth();

        if (requiredRole) {
            requireRole(requiredRole);
        }
    });
}

// Example: Protect Saved page (tenant only)
if (window.location.pathname.includes('/Home/Saved')) {
    protectPage('renter');
}

// Example: Protect Landlord pages
if (window.location.pathname.includes('/Landlord/')) {
    protectPage('landlord');
}

// Initialize auth on all pages
document.addEventListener('DOMContentLoaded', async function () {
    await initAuth();
});

// Export functions for use in other scripts
window.NestFlowAuth = {
    login,
    register,
    logout,
    isLoggedIn,
    getCurrentUser,
    hasRole,
    requireRole,
    initAuth
};