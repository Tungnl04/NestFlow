// Xử lý modal đăng nhập/đăng ký
document.addEventListener('DOMContentLoaded', function () {
    const loginModal = document.getElementById('loginModal');
    const loginForm = document.getElementById('loginForm');
    const showSignup = document.getElementById('showSignup');

    if (showSignup) {
        showSignup.addEventListener('click', function (e) {
            e.preventDefault();
            showSignupForm();
        });
    }

    if (loginForm) {
        loginForm.addEventListener('submit', function (e) {
            e.preventDefault();
            handleLogin();
        });
    }

    // Xử lý đăng nhập bằng social
    document.querySelectorAll('.btn-outline-primary').forEach(btn => {
        if (btn.textContent.includes('Google') || btn.textContent.includes('Facebook') || btn.textContent.includes('Email')) {
            btn.addEventListener('click', function () {
                const provider = this.textContent.includes('Google') ? 'google' :
                    this.textContent.includes('Facebook') ? 'facebook' : 'email';
                handleSocialLogin(provider);
            });
        }
    });
});

function showSignupForm() {
    const modalBody = document.getElementById('loginModalBody');
    const modalTitle = document.getElementById('loginModalLabel');

    modalTitle.textContent = 'Hoàn tất đăng ký';
    modalBody.innerHTML = `
        <form id="signupForm">
            <div class="mb-3">
                <label for="signupName" class="form-label">Họ và tên</label>
                <input type="text" class="form-control" id="signupName" placeholder="Nguyễn Văn A" required>
            </div>
            <div class="mb-3">
                <label for="signupBirthday" class="form-label">Ngày sinh</label>
                <div class="position-relative">
                    <input type="date" class="form-control" id="signupBirthday" required>
                    <i class="fas fa-calendar-alt position-absolute" style="right: 15px; top: 50%; transform: translateY(-50%); color: #999; pointer-events: none;"></i>
                </div>
            </div>
            <div class="mb-3">
                <label for="signupPhone" class="form-label">Số điện thoại</label>
                <input type="tel" class="form-control" id="signupPhone" placeholder="0123456789" required>
            </div>
            <div class="mb-3 form-check">
                <input type="checkbox" class="form-check-input" id="marketingConsent">
                <label class="form-check-label" for="marketingConsent" style="font-size: 0.9rem; line-height: 1.5;">
                    Nestflow sẽ gửi cho bạn các ưu đãi chỉ dành cho thành viên, bài viết truyền cảm hứng, email tiếp thị và thông báo đẩy. Bạn có thể chọn không nhận các thông tin này bất kỳ lúc nào trong cài đặt tài khoản của mình hoặc trực tiếp từ thông báo tiếp thị.
                </label>
            </div>
            <div class="mb-3">
                <small class="text-muted" style="font-size: 0.85rem; line-height: 1.6;">
                    Bằng việc chọn Đồng ý và tiếp tục, tôi đồng ý với <a href="#" class="text-primary text-decoration-underline">Điều khoản dịch vụ</a>, <a href="#" class="text-primary text-decoration-underline">Điều khoản dịch vụ thanh toán</a> và <a href="#" class="text-primary text-decoration-underline">Chính sách không phân biệt</a> của Nestflow, đồng thời chấp thuận <a href="#" class="text-primary text-decoration-underline">Chính sách về quyền riêng tư</a>.
                </small>
            </div>
            <button type="submit" class="btn nf-btn-primary w-100">Đồng ý và tiếp tục</button>
        </form>
    `;

    document.getElementById('signupForm').addEventListener('submit', function (e) {
        e.preventDefault();
        handleSignup();
    });

    document.getElementById('showLogin').addEventListener('click', function (e) {
        e.preventDefault();
        showLoginForm();
    });
}

function showLoginForm() {
    const modalBody = document.getElementById('loginModalBody');
    const modalTitle = document.getElementById('loginModalLabel');

    modalTitle.textContent = 'Đăng nhập hoặc đăng ký';
    modalBody.innerHTML = `
        <h3 class="nf-modal-welcome">Chào mừng đến với Nestflow</h3>
        <form id="loginForm">
            <div class="mb-3">
                <label for="loginEmail" class="form-label">Email</label>
                <input type="email" class="form-control" id="loginEmail" placeholder="email@example.com" required>
            </div>
            <div class="mb-3">
                <label for="loginPassword" class="form-label">Mật khẩu</label>
                <input type="password" class="form-control" id="loginPassword" required>
            </div>
            <button type="submit" class="btn nf-btn-primary w-100 mb-3">Tiếp tục</button>
            <div class="nf-modal-separator">
                <span>Hoặc</span>
            </div>
            <div class="d-grid gap-2 mt-3">
                <button type="button" class="btn nf-btn-social">
                    <i class="fab fa-google"></i> Tiếp tục với Google
                </button>
                <button type="button" class="btn nf-btn-social">
                    <i class="fab fa-facebook-f"></i> Tiếp tục với Facebook
                </button>
                <button type="button" class="btn nf-btn-social">
                    <i class="fas fa-envelope"></i> Tiếp tục với Email
                </button>
            </div>
        </form>
    `;

    document.getElementById('loginForm').addEventListener('submit', function (e) {
        e.preventDefault();
        handleLogin();
    });

    document.getElementById('showSignup').addEventListener('click', function (e) {
        e.preventDefault();
        showSignupForm();
    });
}

// Mock users database
const mockUsers = {
    'tenant@nestflow.com': {
        password: 'password123',
        role: 'tenant',
        name: 'Người Thuê',
        email: 'tenant@nestflow.com'
    },
    'landlord@nestflow.com': {
        password: 'password123',
        role: 'landlord',
        name: 'Chủ Trọ',
        email: 'landlord@nestflow.com'
    },
    'admin@nestflow.com': {
        password: 'password123',
        role: 'admin',
        name: 'Quản Trị Viên',
        email: 'admin@nestflow.com'
    }
};

function handleLogin() {
    const email = document.getElementById('loginEmail').value;
    const password = document.getElementById('loginPassword').value;

    // Kiểm tra thông tin đăng nhập
    const user = mockUsers[email.toLowerCase()];

    if (!user) {
        alert('Email không tồn tại!');
        return;
    }

    if (user.password !== password) {
        alert('Mật khẩu không đúng!');
        return;
    }

    // Lưu thông tin user vào localStorage
    localStorage.setItem('userRole', user.role);
    localStorage.setItem('userName', user.name);
    localStorage.setItem('userEmail', user.email);
    localStorage.setItem('isLoggedIn', 'true');

    // Đóng modal
    const modal = bootstrap.Modal.getInstance(document.getElementById('loginModal'));
    if (modal) {
        modal.hide();
    }

    // Cập nhật UI ngay lập tức
    if (typeof updateAuthUI === 'function') {
        updateAuthUI(user.role, user.name);
    }

    // Redirect theo role sau một chút delay để UI kịp cập nhật
    setTimeout(function () {
        if (user.role === 'landlord' || user.role === 'admin') {
            window.location.href = '/Landlord/Dashboard';
        } else {
            // Tenant - reload trang để cập nhật menu
            window.location.reload();
        }
    }, 300);
}

function handleSignup() {
    const name = document.getElementById('signupName').value;
    const birthday = document.getElementById('signupBirthday').value;
    const phone = document.getElementById('signupPhone').value;
    const email = document.getElementById('signupEmail').value;
    const marketingConsent = document.getElementById('marketingConsent').checked;

    // TODO: Gửi request đến server
    console.log('Đăng ký:', { name, birthday, phone, email, marketingConsent });

    // Tạm thời đóng modal
    const modal = bootstrap.Modal.getInstance(document.getElementById('loginModal'));
    if (modal) {
        modal.hide();
    }

    alert('Đăng ký thành công!');
}

function handleSocialLogin(provider) {
    // Giả lập đăng nhập bằng social - tự động đăng nhập với role tenant
    const user = {
        role: 'tenant',
        name: 'Người dùng mới',
        email: `user_${Date.now()}@nestflow.com`
    };

    localStorage.setItem('userRole', user.role);
    localStorage.setItem('userName', user.name);
    localStorage.setItem('userEmail', user.email);
    localStorage.setItem('isLoggedIn', 'true');

    // Đóng modal
    const modal = bootstrap.Modal.getInstance(document.getElementById('loginModal'));
    if (modal) {
        modal.hide();
    }

    // Cập nhật UI ngay lập tức
    updateAuthUI(user.role, user.name);

    // Hiển thị form hoàn tất đăng ký
    showSignupForm();
}

// Hàm cập nhật UI sau khi đăng nhập
function updateAuthUI(userRole, userName) {
    const authButtons = document.getElementById('authButtons');
    const userInfo = document.getElementById('userInfo');
    const userNameDisplay = document.getElementById('userNameDisplay');
    const userRoleDisplay = document.getElementById('userRoleDisplay');
    const landlordBtn = document.getElementById('landlordBtn');

    // Ẩn nút đăng nhập/đăng ký
    if (authButtons) {
        authButtons.style.setProperty('display', 'none', 'important');
        authButtons.classList.add('d-none');
        authButtons.setAttribute('hidden', 'true');
    }

    // Hiển thị thông tin user
    if (userInfo) {
        userInfo.style.setProperty('display', 'flex', 'important');
        userInfo.classList.remove('d-none');
        userInfo.removeAttribute('hidden');
    }

    if (userNameDisplay && userName) {
        userNameDisplay.textContent = userName;
    }

    if (userRoleDisplay && userRole) {
        let roleText = '';
        let roleClass = '';
        switch (userRole) {
            case 'tenant':
                roleText = 'Người thuê';
                roleClass = 'nf-role-tenant-badge';
                break;
            case 'landlord':
                roleText = 'Chủ trọ';
                roleClass = 'nf-role-landlord-badge';
                break;
            case 'admin':
                roleText = 'Quản trị viên';
                roleClass = 'nf-role-admin-badge';
                break;
            default:
                roleText = 'Người dùng';
                roleClass = 'nf-role-default-badge';
        }
        userRoleDisplay.textContent = roleText;
        userRoleDisplay.className = 'nf-role-badge ' + roleClass;
    }

    if ((userRole === 'landlord' || userRole === 'admin') && landlordBtn) {
        landlordBtn.classList.remove('d-none');
    } else if (landlordBtn) {
        landlordBtn.classList.add('d-none');
    }

    // Cập nhật menu theo role
    if (typeof updateMenuByRole === 'function') {
        updateMenuByRole(userRole);
    }
}

// Xử lý lưu phòng trọ
document.addEventListener('DOMContentLoaded', function () {
    function updateSavedCount() {
        const savedRooms = JSON.parse(localStorage.getItem('savedRooms') || '[]');
        const savedCountBadge = document.getElementById('savedCountBadge');
        if (savedCountBadge) {
            if (savedRooms.length > 0) {
                savedCountBadge.textContent = savedRooms.length;
                savedCountBadge.classList.remove('d-none');
            } else {
                savedCountBadge.classList.add('d-none');
            }
        }
    }

    updateSavedCount();

    document.querySelectorAll('.nf-save-btn').forEach(btn => {
        btn.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();

            const roomId = this.dataset.roomId;
            const icon = this.querySelector('i');
            const roomCard = this.closest('.nf-room-card');
            const roomName = roomCard.querySelector('.nf-room-name').textContent;
            const roomPrice = roomCard.querySelector('.nf-room-price').textContent;
            const roomImage = roomCard.querySelector('img').src;
            const roomRating = roomCard.querySelector('.nf-rating').textContent.trim();
            const postTime = roomCard.querySelector('.nf-post-time').textContent;

            if (icon.classList.contains('far')) {
                // Lưu
                icon.classList.remove('far');
                icon.classList.add('fas');
                this.classList.add('saved');

                // Lưu vào localStorage
                let savedRooms = JSON.parse(localStorage.getItem('savedRooms') || '[]');
                savedRooms.push({
                    id: roomId,
                    name: roomName,
                    price: roomPrice,
                    image: roomImage,
                    rating: roomRating,
                    postTime: postTime
                });
                localStorage.setItem('savedRooms', JSON.stringify(savedRooms));
            } else {
                // Bỏ lưu
                icon.classList.remove('fas');
                icon.classList.add('far');
                this.classList.remove('saved');

                // Xóa khỏi localStorage
                let savedRooms = JSON.parse(localStorage.getItem('savedRooms') || '[]');
                savedRooms = savedRooms.filter(room => room.id !== roomId);
                localStorage.setItem('savedRooms', JSON.stringify(savedRooms));
            }

            updateSavedCount();
        });
    });

    // Xử lý click vào card phòng trọ để chuyển đến trang chi tiết
    document.querySelectorAll('.nf-room-card').forEach(card => {
        if (!card.onclick) {
            card.addEventListener('click', function (e) {
                if (!e.target.closest('.nf-save-btn')) {
                    const roomId = this.querySelector('.nf-save-btn')?.dataset.roomId || '1';
                    window.location.href = `/Room/Detail/${roomId}`;
                }
            });
        }
    });
});