// Authentication API utilities

const AUTH_API = {
    LOGIN: '/api/auth/login',
    REGISTER: '/api/auth/register',
    LOGOUT: '/api/auth/logout',
    CURRENT_USER: '/api/auth/current-user',
    CHECK_SESSION: '/api/auth/check-session',
    FORGOT_PASSWORD: '/api/auth/forgot-password',
    RESET_PASSWORD: '/api/auth/reset-password'
};

// Global user state
let currentUser = null;
let forgotPasswordEmail = '';

// ==================== TOAST NOTIFICATION ====================
function showToast(message, type = 'success', title = '') {
    const toast = document.getElementById('notificationToast');
    const toastHeader = document.getElementById('toastHeader');
    const toastIcon = document.getElementById('toastIcon');
    const toastTitle = document.getElementById('toastTitle');
    const toastMessage = document.getElementById('toastMessage');

    // Set message
    toastMessage.textContent = message;

    // Set style based on type
    toastHeader.className = 'toast-header';
    toastIcon.className = 'bi me-2';

    switch (type) {
        case 'success':
            toastHeader.classList.add('bg-success', 'text-white');
            toastIcon.classList.add('bi-check-circle-fill');
            toastTitle.textContent = title || 'Thành công';
            break;
        case 'error':
            toastHeader.classList.add('bg-danger', 'text-white');
            toastIcon.classList.add('bi-x-circle-fill');
            toastTitle.textContent = title || 'Lỗi';
            break;
        case 'warning':
            toastHeader.classList.add('bg-warning', 'text-dark');
            toastIcon.classList.add('bi-exclamation-triangle-fill');
            toastTitle.textContent = title || 'Cảnh báo';
            break;
        case 'info':
            toastHeader.classList.add('bg-info', 'text-white');
            toastIcon.classList.add('bi-info-circle-fill');
            toastTitle.textContent = title || 'Thông tin';
            break;
    }

    // Show toast
    const bsToast = new bootstrap.Toast(toast, {
        autohide: true,
        delay: 3000
    });
    bsToast.show();
}

// ==================== AUTHENTICATION FUNCTIONS ====================

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
            showToast('Đăng xuất thành công', 'success');
            setTimeout(() => {
                window.location.href = '/';
            }, 1000);
            return { success: true, message: data.message };
        } else {
            return { success: false, message: data.message };
        }
    } catch (error) {
        console.error('Logout error:', error);
        return { success: false, message: 'Đã xảy ra lỗi khi đăng xuất' };
    }
}

// Forgot password - Send verification code
async function forgotPassword(email) {
    try {
        const response = await fetch(AUTH_API.FORGOT_PASSWORD, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ email })
        });

        const data = await response.json();
        return data;
    } catch (error) {
        console.error('Forgot password error:', error);
        return { success: false, message: 'Đã xảy ra lỗi khi gửi mã xác thực' };
    }
}

// Reset password
async function resetPassword(email, verificationCode, newPassword, confirmPassword) {
    try {
        const response = await fetch(AUTH_API.RESET_PASSWORD, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                email,
                verificationCode,
                newPassword,
                confirmPassword
            })
        });

        const data = await response.json();
        return data;
    } catch (error) {
        console.error('Reset password error:', error);
        return { success: false, message: 'Đã xảy ra lỗi khi đặt lại mật khẩu' };
    }
}

// ==================== UTILITY FUNCTIONS ====================

// Switch between modals smoothly
function switchModal(fromModalId, toModalId) {
    const fromModal = document.getElementById(fromModalId);
    const toModal = document.getElementById(toModalId);

    if (fromModal && toModal) {
        const bsFromModal = bootstrap.Modal.getInstance(fromModal);
        if (bsFromModal) {
            bsFromModal.hide();

            // Wait for the modal to be fully hidden
            fromModal.addEventListener('hidden.bs.modal', function () {
                const bsToModal = new bootstrap.Modal(toModal);
                bsToModal.show();
            }, { once: true });
        } else {
            // If no instance exists, just hide and show
            const newBsFromModal = new bootstrap.Modal(fromModal);
            newBsFromModal.hide();

            fromModal.addEventListener('hidden.bs.modal', function () {
                const bsToModal = new bootstrap.Modal(toModal);
                bsToModal.show();
            }, { once: true });
        }
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
    if (role === 'renter') return currentUser.userType === 'renter';
    if (role === 'tenant') return currentUser.userType === 'renter'; // Alias
    return false;
}

// Role access control with redirect
function requireRole(requiredRole, redirectUrl = '/') {
    if (!isLoggedIn()) {
        showToast('Vui lòng đăng nhập để truy cập trang này.', 'warning');
        setTimeout(() => {
            window.location.href = redirectUrl;
        }, 1500);
        return false;
    }

    if (requiredRole === 'renter' && !hasRole('renter')) {
        showToast('Chỉ người thuê mới có thể truy cập trang này.', 'error');
        setTimeout(() => {
            window.location.href = redirectUrl;
        }, 1500);
        return false;
    }

    if (requiredRole === 'landlord' && !hasRole('landlord')) {
        showToast('Chỉ chủ trọ và quản trị viên mới có thể truy cập trang này.', 'error');
        setTimeout(() => {
            window.location.href = redirectUrl;
        }, 1500);
        return false;
    }

    if (requiredRole === 'admin' && !hasRole('admin')) {
        showToast('Chỉ quản trị viên mới có thể truy cập trang này.', 'error');
        setTimeout(() => {
            window.location.href = redirectUrl;
        }, 1500);
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

// ==================== MODAL EVENT HANDLERS ====================

// Initialize modal handlers when DOM is loaded
function initModalHandlers() {
    // Login Form Handler
    const loginForm = document.getElementById('loginForm');
    if (loginForm) {
        loginForm.addEventListener('submit', async function (e) {
            e.preventDefault();

            const email = document.getElementById('loginEmail').value;
            const password = document.getElementById('loginPassword').value;
            const errorDiv = document.getElementById('loginError');

            errorDiv.classList.add('d-none');

            const result = await login(email, password);

            if (result.success) {
                // Close modal
                const modal = bootstrap.Modal.getInstance(document.getElementById('loginModal'));
                modal.hide();

                // Show success toast
                showToast('Đăng nhập thành công! Chào mừng ' + result.user.fullName, 'success');

                // Reload page after a short delay
                setTimeout(() => {
                    window.location.reload();
                }, 1000);
            } else {
                errorDiv.textContent = result.message;
                errorDiv.classList.remove('d-none');
            }
        });
    }

    // Register Form Handler
    const registerForm = document.getElementById('registerForm');
    if (registerForm) {
        registerForm.addEventListener('submit', async function (e) {
            e.preventDefault();

            const password = document.getElementById('registerPassword').value;
            const confirmPassword = document.getElementById('registerConfirmPassword').value;
            const errorDiv = document.getElementById('registerError');

            errorDiv.classList.add('d-none');

            if (password !== confirmPassword) {
                errorDiv.textContent = 'Mật khẩu không khớp';
                errorDiv.classList.remove('d-none');
                return;
            }

            const registerData = {
                email: document.getElementById('registerEmail').value,
                password: password,
                confirmPassword: confirmPassword,
                fullName: document.getElementById('registerFullName').value,
                phone: document.getElementById('registerPhone').value,
                userType: document.querySelector('input[name="userType"]:checked').value
            };

            const result = await register(registerData);

            if (result.success) {
                // Close modal
                const modal = bootstrap.Modal.getInstance(document.getElementById('registerModal'));
                modal.hide();

                // Show success toast
                showToast('Đăng ký thành công! Chào mừng bạn đến với NestFlow', 'success');

                // Reload page after a short delay
                setTimeout(() => {
                    window.location.reload();
                }, 1000);
            } else {
                errorDiv.textContent = result.message;
                errorDiv.classList.remove('d-none');
            }
        });
    }

    // Forgot Password Form Handler - Step 1
    const forgotPasswordForm = document.getElementById('forgotPasswordForm');
    if (forgotPasswordForm) {
        forgotPasswordForm.addEventListener('submit', async function (e) {
            e.preventDefault();

            const email = document.getElementById('forgotEmail').value;
            const errorDiv = document.getElementById('forgotPasswordError');

            errorDiv.classList.add('d-none');

            const data = await forgotPassword(email);

            if (data.success) {
                forgotPasswordEmail = email;

                // Show step 2
                document.getElementById('forgotPasswordStep1').classList.add('d-none');
                document.getElementById('forgotPasswordStep2').classList.remove('d-none');

                showToast('Mã xác thực đã được gửi đến email của bạn', 'success');
            } else {
                errorDiv.textContent = data.message;
                errorDiv.classList.remove('d-none');
            }
        });
    }

    // Reset Password Form Handler - Step 2
    const resetPasswordForm = document.getElementById('resetPasswordForm');
    if (resetPasswordForm) {
        resetPasswordForm.addEventListener('submit', async function (e) {
            e.preventDefault();

            const verificationCode = document.getElementById('verificationCode').value;
            const newPassword = document.getElementById('resetNewPassword').value;
            const confirmPassword = document.getElementById('resetConfirmPassword').value;
            const errorDiv = document.getElementById('resetPasswordError');

            errorDiv.classList.add('d-none');

            if (newPassword !== confirmPassword) {
                errorDiv.textContent = 'Mật khẩu không khớp';
                errorDiv.classList.remove('d-none');
                return;
            }

            const data = await resetPassword(forgotPasswordEmail, verificationCode, newPassword, confirmPassword);

            if (data.success) {
                // Close modal
                const modal = bootstrap.Modal.getInstance(document.getElementById('forgotPasswordModal'));
                modal.hide();

                showToast('Đặt lại mật khẩu thành công! Vui lòng đăng nhập lại', 'success');

                // Reset form
                resetForgotPasswordModal();

                // Show login modal after a short delay
                setTimeout(() => {
                    const loginModal = new bootstrap.Modal(document.getElementById('loginModal'));
                    loginModal.show();
                }, 1500);
            } else {
                errorDiv.textContent = data.message;
                errorDiv.classList.remove('d-none');
            }
        });
    }

    // Modal reset handlers
    const loginModal = document.getElementById('loginModal');
    if (loginModal) {
        loginModal.addEventListener('hidden.bs.modal', function () {
            const errorDiv = document.getElementById('loginError');
            if (errorDiv) errorDiv.classList.add('d-none');
            const form = document.getElementById('loginForm');
            if (form) form.reset();
        });
    }

    const registerModal = document.getElementById('registerModal');
    if (registerModal) {
        registerModal.addEventListener('hidden.bs.modal', function () {
            const errorDiv = document.getElementById('registerError');
            if (errorDiv) errorDiv.classList.add('d-none');
            const form = document.getElementById('registerForm');
            if (form) form.reset();
        });
    }

    const forgotPasswordModal = document.getElementById('forgotPasswordModal');
    if (forgotPasswordModal) {
        forgotPasswordModal.addEventListener('hidden.bs.modal', function () {
            resetForgotPasswordModal();
        });
    }

    // Initialize modal switching handlers
    initModalSwitching();
}

// Initialize modal switching to prevent overlapping
function initModalSwitching() {
    // Handle all links that switch between modals
    document.querySelectorAll('a[data-bs-toggle="modal"][data-bs-dismiss="modal"]').forEach(link => {
        link.addEventListener('click', function (e) {
            e.preventDefault();

            const targetModalId = this.getAttribute('data-bs-target');
            const currentModal = this.closest('.modal');

            if (currentModal && targetModalId) {
                switchModal(currentModal.id, targetModalId.substring(1)); // Remove # from target
            }
        });
    });

    // Handle the forgot password link specifically
    const forgotPasswordLink = document.querySelector('a[href="#"][data-bs-target="#forgotPasswordModal"]');
    if (forgotPasswordLink) {
        forgotPasswordLink.addEventListener('click', function (e) {
            e.preventDefault();
            switchModal('loginModal', 'forgotPasswordModal');
        });
    }

    // Handle back to login links
    document.querySelectorAll('a[data-bs-target="#loginModal"]').forEach(link => {
        link.addEventListener('click', function (e) {
            const currentModal = this.closest('.modal');
            if (currentModal) {
                e.preventDefault();
                switchModal(currentModal.id, 'loginModal');
            }
        });
    });
}

// Resend verification code
async function resendVerificationCode() {
    const data = await forgotPassword(forgotPasswordEmail);

    if (data.success) {
        showToast('Mã xác thực mới đã được gửi', 'success');
    } else {
        showToast(data.message, 'error');
    }
}

// Reset forgot password modal
function resetForgotPasswordModal() {
    const step1 = document.getElementById('forgotPasswordStep1');
    const step2 = document.getElementById('forgotPasswordStep2');

    if (step1) step1.classList.remove('d-none');
    if (step2) step2.classList.add('d-none');

    const forgotForm = document.getElementById('forgotPasswordForm');
    const resetForm = document.getElementById('resetPasswordForm');
    const forgotError = document.getElementById('forgotPasswordError');
    const resetError = document.getElementById('resetPasswordError');

    if (forgotForm) forgotForm.reset();
    if (resetForm) resetForm.reset();
    if (forgotError) forgotError.classList.add('d-none');
    if (resetError) resetError.classList.add('d-none');

    forgotPasswordEmail = '';
}

// ==================== PAGE PROTECTION ====================

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

// NOTE: Landlord pages are protected by _LandlordLayout.cshtml
// No need to protect here to avoid double-checking
// if (window.location.pathname.includes('/Landlord/')) {
//     protectPage('landlord');
// }

// ==================== INITIALIZATION ====================

// Initialize auth on all pages
document.addEventListener('DOMContentLoaded', async function () {
    await initAuth();
    initModalHandlers();
});

// ==================== EXPORT ====================

// Export functions for use in other scripts
window.NestFlowAuth = {
    login,
    register,
    logout,
    isLoggedIn,
    getCurrentUser,
    hasRole,
    requireRole,
    initAuth,
    forgotPassword,
    resetPassword,
    showToast,
    resendVerificationCode,
    switchModal
};