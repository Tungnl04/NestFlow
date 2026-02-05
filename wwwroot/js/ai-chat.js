document.addEventListener('DOMContentLoaded', function () {
    const chatWidget = document.getElementById('aiChatWidget');
    const chatButton = document.getElementById('aiChatButton');
    const closeButton = document.getElementById('closeChatBtn');
    const sendButton = document.getElementById('sendMessageBtn');
    const chatInput = document.getElementById('chatInput');
    const messagesContainer = document.getElementById('chatMessages');

    // Toggle Chat Window
    function toggleChat() {
        // Check if user is authenticated (by checking if auth buttons are hidden)
        const authButtons = document.getElementById('authButtons');
        const isAuthenticated = authButtons && authButtons.classList.contains('d-none');

        if (!isAuthenticated) {
            // User not logged in, show login modal instead
            const loginModalEl = document.getElementById('loginModal');
            if (loginModalEl) {
                const modal = new bootstrap.Modal(loginModalEl);
                modal.show();
            } else {
                // Fallback: redirect to login
                window.location.href = '/Auth/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
            }
            return;
        }

        // User is authenticated, proceed with toggling chat
        if (chatWidget.classList.contains('d-none')) {
            chatWidget.classList.remove('d-none');
            // Slight delay to allow display:block to apply before adding class for animation
            setTimeout(() => chatWidget.classList.add('active'), 10);
            chatInput.focus();
        } else {
            chatWidget.classList.remove('active');
            setTimeout(() => chatWidget.classList.add('d-none'), 300);
        }
    }

    chatButton?.addEventListener('click', toggleChat);
    closeButton?.addEventListener('click', toggleChat);

    // Send Message Logic
    async function sendMessage() {
        const message = chatInput.value.trim();
        if (!message) return;

        // 1. Add User Message
        appendMessage('user', message);
        chatInput.value = '';
        chatInput.disabled = true; // Disable input while waiting

        // Show typing indicator
        const typingId = showTypingIndicator();

        // 2. Prepare History
        const history = [];
        const msgElements = document.querySelectorAll('.nf-chat-message');

        // Take last 10 messages to keep context (skip typing indicators and welcome msg if strict)
        let count = 0;
        // Iterate backwards
        for (let i = msgElements.length - 1; i >= 0; i--) {
            if (count >= 10) break;
            const el = msgElements[i];
            if (el.classList.contains('typing-indicator')) continue;

            const role = el.classList.contains('user') ? 'user' : 'ai';
            // Extract text only (remove time, buttons etc)
            const clone = el.cloneNode(true);
            const buttons = clone.querySelectorAll('button, a');
            buttons.forEach(b => b.remove());
            const smalls = clone.querySelectorAll('small');
            smalls.forEach(s => s.remove());

            let text = clone.innerText.trim();
            if (!text) continue; // skip empty

            history.unshift({ role: role, message: text });
            count++;
        }

        // Remove the very last one we just added (current user message) from history
        history.pop();

        try {
            // 3. Call API
            const response = await fetch('/api/AIChat/send', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ message: message, history: history })
            });

            removeTypingIndicator(typingId);

            if (response.ok) {
                const data = await response.json();
                // 3. Add AI Response
                appendMessage('ai', data.reply);
            } else {
                appendMessage('ai', 'Xin lỗi, hệ thống đang bận. Vui lòng thử lại sau.');
            }
        } catch (error) {
            console.error('Chat Error:', error);
            removeTypingIndicator(typingId);
            appendMessage('ai', 'Có lỗi xảy ra kết nối.');
        } finally {
            chatInput.disabled = false;
            chatInput.focus();
        }
    }

    sendButton?.addEventListener('click', sendMessage);

    chatInput?.addEventListener('keypress', function (e) {
        if (e.key === 'Enter') {
            sendMessage();
        }
    });

    // Helper: Append Message
    function appendMessage(sender, text) {
        const msgDiv = document.createElement('div');
        msgDiv.className = `nf-chat-message ${sender}`;

        const contentDiv = document.createElement('div');
        contentDiv.className = 'message-content';
        if (sender === 'ai') {
            contentDiv.innerHTML = parseMarkdown(text);
        } else {
            contentDiv.textContent = text;
        }

        msgDiv.appendChild(contentDiv);
        messagesContainer.appendChild(msgDiv);

        // Scroll to bottom
        messagesContainer.scrollTop = messagesContainer.scrollHeight;
    }

    // Helper: Simple Markdown Parser
    function parseMarkdown(text) {
        if (!text) return '';

        // Escape HTML first to prevent XSS from raw text (except what we enable below)
        let html = text
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;");

        // 1. Bold: **text**
        html = html.replace(/\*\*(.*?)\*\*/g, '<b>$1</b>');

        // 2. Lists: - item or * item
        html = html.replace(/^\s*[-*]\s+(.*)$/gm, '<li>$1</li>');

        // 3. Bullet points • - ensure proper spacing and line breaks
        html = html.replace(/•\s*/g, '<br>• ');

        // 4. Line breaks - convert \n to <br>
        html = html.replace(/\n/g, '<br>');

        // 5. Clean up excessive line breaks (more than 2 consecutive)
        html = html.replace(/(<br>\s*){3,}/g, '<br><br>');

        // 6. Auto-detect room codes: (Mã: X) -> Add button after
        html = html.replace(/\(Mã:\s*(\d+)\)/g, function (match, id) {
            return match + ` <a href="/Room/Detail?id=${id}" class="btn btn-sm btn-primary ms-2" target="_blank"><i class="fas fa-info-circle"></i> Xem chi tiết</a>`;
        });

        // 7. Detail Page Link Token: [[DETAIL:123]] (legacy)
        html = html.replace(/\[\[\s*DETAIL\s*:\s*(\d+)\s*\]\]/g, function (match, id) {
            return `<a href="/Room/Detail?id=${id}" class="btn btn-sm btn-primary mt-2" target="_blank">
                        <i class="fas fa-info-circle"></i> Xem chi tiết phòng
                    </a>`;
        });

        // 8. Contact Button Token: [[CONTACT:123]] (legacy)
        html = html.replace(/\[\[\s*CONTACT\s*:\s*(\d+)\s*\]\]/g, function (match, id) {
            return `<button class="btn btn-sm btn-success mt-2" onclick="revealContact(${id}, this)">
                        <i class="fas fa-eye"></i> Xem SĐT Chủ nhà
                    </button>`;
        });

        return html;
    }

    // Expose this function globally so the button onclick can find it
    window.revealContact = async function (propertyId, btnElement) {
        try {
            btnElement.disabled = true;
            btnElement.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang lấy...';

            const response = await fetch(`/api/AIChat/contact/${propertyId}`, {
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                },
                redirect: 'manual'
            });

            // Check if user is not logged in
            if (response.status === 401 || response.status === 0 || response.type === 'opaqueredirect') {
                btnElement.disabled = false;
                btnElement.innerHTML = '<i class="fas fa-eye"></i> Xem SĐT';

                // Show login modal
                const loginModalEl = document.getElementById('loginModal');
                if (loginModalEl) {
                    const modal = new bootstrap.Modal(loginModalEl);
                    modal.show();
                } else {
                    showLoginPopup();
                }
                return;
            }

            if (response.ok) {
                const data = await response.json();
                // Replace button with the phone number
                const parent = btnElement.parentElement;
                const infoDiv = document.createElement('div');
                infoDiv.className = 'alert alert-success mt-2 mb-0 p-2';
                infoDiv.innerHTML = `<strong>${data.name}:</strong> <a href="tel:${data.phone}" class="fw-bold">${data.phone}</a>`;

                btnElement.remove();
                parent.appendChild(infoDiv);
            } else {
                // Try to parse error message from API
                let errorMsg = 'Lỗi ' + response.status;
                try {
                    const errorData = await response.json();
                    if (errorData.error) {
                        errorMsg = errorData.error;
                    }
                } catch (e) {
                    // If can't parse JSON, use status code
                }

                btnElement.innerHTML = '<i class="fas fa-exclamation-circle"></i> ' + errorMsg;
                btnElement.disabled = false;
            }
        } catch (error) {
            console.error(error);
            btnElement.innerHTML = 'Lỗi kết nối';
            btnElement.disabled = false;
        }
    };

    function showLoginPopup() {
        if (!document.getElementById('dynamicLoginModal')) {
            const modalHtml = `
            <div class="modal fade" id="dynamicLoginModal" tabindex="-1" aria-hidden="true">
                <div class="modal-dialog modal-dialog-centered">
                    <div class="modal-content text-center p-4">
                        <div class="mb-3">
                            <i class="fas fa-lock fa-3x text-warning"></i>
                        </div>
                        <h4 class="mb-2">Yêu cầu đăng nhập</h4>
                        <p class="text-muted mb-4">Bạn cần đăng nhập để xem thông tin liên hệ của chủ nhà.</p>
                        <div class="d-grid gap-2">
                            <a href="/Auth/Login?returnUrl=${encodeURIComponent(window.location.pathname)}" class="btn btn-primary">Đăng nhập ngay</a>
                            <button type="button" class="btn btn-outline-secondary" data-bs-dismiss="modal">Đóng</button>
                        </div>
                    </div>
                </div>
            </div>`;
            document.body.insertAdjacentHTML('beforeend', modalHtml);
        }
        const dynModal = new bootstrap.Modal(document.getElementById('dynamicLoginModal'));
        dynModal.show();
    }

    // Helper: Typing Indicator
    function showTypingIndicator() {
        const id = 'typing-' + Date.now();
        const msgDiv = document.createElement('div');
        msgDiv.className = 'nf-chat-message ai typing-indicator';
        msgDiv.id = id;
        msgDiv.innerHTML = `<div class="message-content">
                                <span class="dot"></span>
                                <span class="dot"></span>
                                <span class="dot"></span>
                            </div>`;
        messagesContainer.appendChild(msgDiv);
        messagesContainer.scrollTop = messagesContainer.scrollHeight;
        return id;
    }

    function removeTypingIndicator(id) {
        const el = document.getElementById(id);
        if (el) el.remove();
    }
});
