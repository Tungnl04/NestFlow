// Authentication and Authorization utilities

// Kiểm tra role và redirect nếu không đủ quyền
function checkRoleAccess(requiredRole) {
    const userRole = localStorage.getItem('userRole');
    const isLoggedIn = localStorage.getItem('isLoggedIn');

    if (!isLoggedIn || isLoggedIn !== 'true') {
        // Chưa đăng nhập
        if (requiredRole) {
            alert('Vui lòng đăng nhập để truy cập trang này.');
            window.location.href = '/';
            return false;
        }
        return true;
    }

    if (!userRole) {
        alert('Không xác định được quyền truy cập. Vui lòng đăng nhập lại.');
        window.location.href = '/';
        return false;
    }

    // Kiểm tra quyền truy cập
    if (requiredRole === 'tenant' && userRole !== 'tenant') {
        alert('Chỉ người thuê mới có thể truy cập trang này.');
        window.location.href = '/';
        return false;
    }

    if (requiredRole === 'landlord' && userRole !== 'landlord' && userRole !== 'admin') {
        alert('Chỉ chủ trọ và quản trị viên mới có thể truy cập trang này.');
        window.location.href = '/';
        return false;
    }

    if (requiredRole === 'admin' && userRole !== 'admin') {
        alert('Chỉ quản trị viên mới có thể truy cập trang này.');
        window.location.href = '/';
        return false;
    }

    return true;
}

// Kiểm tra khi trang load
document.addEventListener('DOMContentLoaded', function () {
    // Kiểm tra trang Saved - chỉ tenant
    if (window.location.pathname.includes('/Home/Saved')) {
        if (!checkRoleAccess('tenant')) {
            return;
        }
    }

    // Kiểm tra các trang Landlord - chỉ landlord và admin
    if (window.location.pathname.includes('/Landlord/')) {
        if (!checkRoleAccess('landlord')) {
            return;
        }
    }
});
