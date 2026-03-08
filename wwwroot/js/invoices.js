let currentUserId = null;

// Format số tiền khi nhập
function formatMoney(input) {
    // Lấy giá trị và loại bỏ tất cả ký tự không phải số
    let value = input.value.replace(/[^\d]/g, '');
    
    // Nếu rỗng, set về 0
    if (value === '') {
        value = '0';
    }
    
    // Format với dấu phẩy ngăn cách hàng nghìn
    let formatted = parseInt(value).toLocaleString('vi-VN');
    
    // Set lại giá trị đã format
    input.value = formatted;
}

// Lấy giá trị số từ input đã format
function getMoneyValue(elementId) {
    const value = document.getElementById(elementId).value.replace(/[^\d]/g, '');
    return parseFloat(value) || 0;
}

// Hiển thị thông báo bằng modal
function showMessage(message, type = 'info') {
    const modal = document.getElementById('messageModal');
    const header = document.getElementById('messageModalHeader');
    const title = document.getElementById('messageModalTitle');
    const body = document.getElementById('messageModalBody');
    
    // Set màu header theo loại thông báo
    header.className = 'modal-header';
    if (type === 'success') {
        header.classList.add('bg-success', 'text-white');
        title.innerHTML = '<i class="fas fa-check-circle"></i> Thành công';
    } else if (type === 'error') {
        header.classList.add('bg-danger', 'text-white');
        title.innerHTML = '<i class="fas fa-times-circle"></i> Lỗi';
    } else if (type === 'warning') {
        header.classList.add('bg-warning', 'text-dark');
        title.innerHTML = '<i class="fas fa-exclamation-triangle"></i> Cảnh báo';
    } else {
        header.classList.add('bg-info', 'text-white');
        title.innerHTML = '<i class="fas fa-info-circle"></i> Thông báo';
    }
    
    body.innerHTML = `<p class="mb-0">${message}</p>`;
    
    const bsModal = new bootstrap.Modal(modal);
    bsModal.show();
}

// Hiển thị modal xác nhận xóa
function showConfirmDelete(invoiceId) {
    document.getElementById('deleteInvoiceId').value = invoiceId;
    const modal = new bootstrap.Modal(document.getElementById('confirmDeleteModal'));
    modal.show();
}

document.addEventListener('DOMContentLoaded', function () {
    // Get user ID from session
    fetch('/api/Auth/current-user')
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                currentUserId = data.user.userId;
                loadInvoices();
                loadRentals();
                loadStats();
            } else {
                console.error('User not logged in');
            }
        })
        .catch(error => {
            console.error('Error getting current user:', error);
        });

    // Set default dates
    const dueDate = new Date();
    dueDate.setDate(dueDate.getDate() + 7);
    document.getElementById('dueDate').valueAsDate = dueDate;
    document.getElementById('paidDate').valueAsDate = new Date();
    
    // Set default invoice month
    const now = new Date();
    document.getElementById('invoiceMonth').value = `${String(now.getMonth() + 1).padStart(2, '0')}/${now.getFullYear()}`;
});

async function loadInvoices() {
    if (!currentUserId) return;

    const status = document.getElementById('filterStatus').value;
    const tbody = document.getElementById('invoicesTableBody');
    tbody.innerHTML = '<tr><td colspan="8" class="text-center py-4"><div class="spinner-border text-primary"></div></td></tr>';

    try {
        const url = `/api/Invoice/landlord/${currentUserId}${status ? '?status=' + status : ''}`;
        const response = await fetch(url);
        const result = await response.json();

        if (result.success) {
            displayInvoices(result.data);
        } else {
            tbody.innerHTML = '<tr><td colspan="8" class="text-center text-danger">Lỗi tải dữ liệu</td></tr>';
        }
    } catch (error) {
        console.error('Error loading invoices:', error);
        tbody.innerHTML = '<tr><td colspan="8" class="text-center text-danger">Lỗi tải dữ liệu</td></tr>';
    }
}

function displayInvoices(invoices) {
    const tbody = document.getElementById('invoicesTableBody');

    if (invoices.length === 0) {
        tbody.innerHTML = '<tr><td colspan="8" class="text-center text-muted py-4">Chưa có hóa đơn nào</td></tr>';
        return;
    }

    tbody.innerHTML = invoices.map(invoice => `
        <tr>
            <td>#${invoice.invoiceId}</td>
            <td>${invoice.propertyTitle}</td>
            <td>${invoice.renterName}</td>
            <td>${invoice.invoiceMonth || 'N/A'}</td>
            <td>${invoice.dueDate ? formatDate(invoice.dueDate) : 'N/A'}</td>
            <td><strong>${formatCurrency(invoice.totalAmount || 0)}</strong></td>
            <td>${getStatusBadge(invoice.status, invoice.isOverdue)}</td>
            <td>
                <div class="btn-group btn-group-sm">
                    <button class="btn btn-outline-primary" onclick="viewInvoice(${invoice.invoiceId})" title="Xem">
                        <i class="fas fa-eye"></i>
                    </button>
                    ${invoice.status !== 'paid' ? `
                        <button class="btn btn-outline-success" onclick="openMarkPaidModal(${invoice.invoiceId})" title="Đánh dấu đã thanh toán">
                            <i class="fas fa-check"></i>
                        </button>
                        <button class="btn btn-outline-danger" onclick="deleteInvoice(${invoice.invoiceId})" title="Xóa">
                            <i class="fas fa-trash"></i>
                        </button>
                    ` : ''}
                </div>
            </td>
        </tr>
    `).join('');
}

function getStatusBadge(status, isOverdue) {
    if (isOverdue && status !== 'paid') {
        return '<span class="badge bg-danger">Quá hạn</span>';
    }

    switch (status) {
        case 'pending':
            return '<span class="badge bg-warning">Chờ thanh toán</span>';
        case 'paid':
            return '<span class="badge bg-success">Đã thanh toán</span>';
        case 'overdue':
            return '<span class="badge bg-danger">Quá hạn</span>';
        case 'cancelled':
            return '<span class="badge bg-secondary">Đã hủy</span>';
        default:
            return '<span class="badge bg-secondary">' + status + '</span>';
    }
}

async function loadRentals() {
    if (!currentUserId) return;

    try {
        const response = await fetch(`/api/Rental/landlord/${currentUserId}`);
        const result = await response.json();

        if (result.success) {
            const select = document.getElementById('rentalId');
            select.innerHTML = '<option value="">Chọn hợp đồng...</option>' +
                result.data.map(rental => `
                    <option value="${rental.rentalId}">
                        ${rental.propertyTitle} - ${rental.renterName}
                    </option>
                `).join('');
        }
    } catch (error) {
        console.error('Error loading rentals:', error);
    }
}

async function loadStats() {
    if (!currentUserId) return;

    try {
        const response = await fetch(`/api/Invoice/revenue-stats/${currentUserId}`);
        const result = await response.json();

        if (result.success) {
            const stats = result.data;
            document.getElementById('totalInvoices').textContent = stats.totalInvoices;
            document.getElementById('paidInvoices').textContent = stats.paidInvoices;
            document.getElementById('overdueInvoices').textContent = stats.overdueInvoices;
            document.getElementById('totalRevenue').textContent = formatCurrency(stats.totalRevenue);
        }
    } catch (error) {
        console.error('Error loading stats:', error);
    }
}

function openCreateInvoiceModal() {
    document.getElementById('invoiceModalTitle').textContent = 'Tạo hóa đơn mới';
    document.getElementById('invoiceId').value = '';
    document.getElementById('rentalId').value = '';
    
    const now = new Date();
    document.getElementById('invoiceMonth').value = `${String(now.getMonth() + 1).padStart(2, '0')}/${now.getFullYear()}`;
    
    document.getElementById('roomRent').value = '0';
    document.getElementById('electricAmount').value = '0';
    document.getElementById('waterAmount').value = '0';
    document.getElementById('internetFee').value = '0';
    document.getElementById('electricUsage').value = '0';
    document.getElementById('waterUsage').value = '0';
    document.getElementById('electricOldReading').value = '0';
    document.getElementById('electricNewReading').value = '0';
    document.getElementById('waterOldReading').value = '0';
    document.getElementById('waterNewReading').value = '0';
    document.getElementById('otherFees').value = '';
    document.getElementById('notes').value = '';
    
    calculateTotal();
    
    const modal = new bootstrap.Modal(document.getElementById('invoiceModal'));
    modal.show();
}

function calculateElectric() {
    const oldReading = parseFloat(document.getElementById('electricOldReading').value) || 0;
    const newReading = parseFloat(document.getElementById('electricNewReading').value) || 0;
    const usage = Math.max(0, newReading - oldReading);
    document.getElementById('electricUsage').value = usage;
}

function calculateWater() {
    const oldReading = parseFloat(document.getElementById('waterOldReading').value) || 0;
    const newReading = parseFloat(document.getElementById('waterNewReading').value) || 0;
    const usage = Math.max(0, newReading - oldReading);
    document.getElementById('waterUsage').value = usage;
}

function calculateTotal() {
    const rent = getMoneyValue('roomRent');
    const electricity = getMoneyValue('electricAmount');
    const water = getMoneyValue('waterAmount');
    const internet = getMoneyValue('internetFee');
    
    const total = rent + electricity + water + internet;
    document.getElementById('totalAmountDisplay').textContent = formatCurrency(total);
}

async function saveInvoice() {
    const invoiceId = document.getElementById('invoiceId').value;
    const rentalId = document.getElementById('rentalId').value;
    const invoiceMonth = document.getElementById('invoiceMonth').value;
    const dueDate = document.getElementById('dueDate').value;

    if (!rentalId || !dueDate) {
        showMessage('Vui lòng điền đầy đủ thông tin bắt buộc', 'warning');
        return;
    }

    const data = {
        rentalId: parseInt(rentalId),
        invoiceMonth: invoiceMonth,
        dueDate: dueDate,
        roomRent: getMoneyValue('roomRent'),
        electricAmount: getMoneyValue('electricAmount'),
        waterAmount: getMoneyValue('waterAmount'),
        internetFee: getMoneyValue('internetFee'),
        electricUsage: parseInt(document.getElementById('electricUsage').value) || 0,
        waterUsage: parseInt(document.getElementById('waterUsage').value) || 0,
        electricOldReading: parseInt(document.getElementById('electricOldReading').value) || 0,
        electricNewReading: parseInt(document.getElementById('electricNewReading').value) || 0,
        waterOldReading: parseInt(document.getElementById('waterOldReading').value) || 0,
        waterNewReading: parseInt(document.getElementById('waterNewReading').value) || 0,
        otherFees: document.getElementById('otherFees').value,
        notes: document.getElementById('notes').value
    };

    try {
        const url = invoiceId ? `/api/Invoice/update/${invoiceId}` : '/api/Invoice/create';
        const method = invoiceId ? 'PUT' : 'POST';

        const response = await fetch(url, {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });

        const result = await response.json();

        if (result.success) {
            showMessage(result.message, 'success');
            bootstrap.Modal.getInstance(document.getElementById('invoiceModal')).hide();
            loadInvoices();
            loadStats();
        } else {
            showMessage(result.message || 'Có lỗi xảy ra', 'error');
        }
    } catch (error) {
        console.error('Error saving invoice:', error);
        showMessage('Có lỗi xảy ra khi lưu hóa đơn', 'error');
    }
}

function openMarkPaidModal(invoiceId) {
    document.getElementById('paidInvoiceId').value = invoiceId;
    document.getElementById('paidDate').valueAsDate = new Date();
    
    const modal = new bootstrap.Modal(document.getElementById('markPaidModal'));
    modal.show();
}

async function markAsPaid() {
    const invoiceId = document.getElementById('paidInvoiceId').value;
    const paidDate = document.getElementById('paidDate').value;
    const paymentMethod = document.getElementById('paymentMethod').value;

    if (!paidDate) {
        showMessage('Vui lòng chọn ngày thanh toán', 'warning');
        return;
    }

    try {
        const response = await fetch(`/api/Invoice/mark-paid/${invoiceId}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ paidDate, paymentMethod })
        });

        const result = await response.json();

        if (result.success) {
            showMessage(result.message, 'success');
            bootstrap.Modal.getInstance(document.getElementById('markPaidModal')).hide();
            loadInvoices();
            loadStats();
        } else {
            showMessage(result.message || 'Có lỗi xảy ra', 'error');
        }
    } catch (error) {
        console.error('Error marking as paid:', error);
        showMessage('Có lỗi xảy ra', 'error');
    }
}

async function deleteInvoice(invoiceId) {
    showConfirmDelete(invoiceId);
}

async function confirmDelete() {
    const invoiceId = document.getElementById('deleteInvoiceId').value;
    
    try {
        const response = await fetch(`/api/Invoice/delete/${invoiceId}`, {
            method: 'DELETE'
        });

        const result = await response.json();

        if (result.success) {
            showMessage(result.message, 'success');
            bootstrap.Modal.getInstance(document.getElementById('confirmDeleteModal')).hide();
            loadInvoices();
            loadStats();
        } else {
            showMessage(result.message || 'Có lỗi xảy ra', 'error');
        }
    } catch (error) {
        console.error('Error deleting invoice:', error);
        showMessage('Có lỗi xảy ra khi xóa hóa đơn', 'error');
    }
}

async function exportToExcel() {
    if (!currentUserId) return;

    const fromDate = document.getElementById('filterFromDate').value;
    const toDate = document.getElementById('filterToDate').value;

    let url = `/api/Invoice/export-csv/${currentUserId}`;
    const params = [];
    if (fromDate) params.push(`fromDate=${fromDate}`);
    if (toDate) params.push(`toDate=${toDate}`);
    if (params.length > 0) url += '?' + params.join('&');

    window.location.href = url;
}

function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('vi-VN');
}

function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(amount);
}

function viewInvoice(invoiceId) {
    fetch(`/api/Invoice/detail/${invoiceId}`)
        .then(response => response.json())
        .then(result => {
            if (result.success) {
                displayInvoiceDetail(result.data);
            } else {
                showMessage('Không thể tải chi tiết hóa đơn', 'error');
            }
        })
        .catch(error => {
            console.error('Error loading invoice detail:', error);
            showMessage('Có lỗi xảy ra khi tải chi tiết hóa đơn', 'error');
        });
}

function displayInvoiceDetail(invoice) {
    // Thông tin chung
    document.getElementById('viewInvoiceId').textContent = '#' + invoice.invoiceId;
    document.getElementById('viewInvoiceMonth').textContent = invoice.invoiceMonth || 'N/A';
    document.getElementById('viewCreatedAt').textContent = invoice.createdAt ? formatDateTime(invoice.createdAt) : 'N/A';
    document.getElementById('viewDueDate').textContent = invoice.dueDate ? formatDate(invoice.dueDate) : 'N/A';
    document.getElementById('viewStatus').innerHTML = getStatusBadge(invoice.status, invoice.isOverdue);
    
    // Thông tin thuê
    document.getElementById('viewPropertyTitle').textContent = invoice.propertyTitle;
    document.getElementById('viewRenterName').textContent = invoice.renterName;
    document.getElementById('viewPaymentDate').textContent = invoice.paymentDate ? formatDate(invoice.paymentDate) : 'Chưa thanh toán';
    document.getElementById('viewPaymentMethod').textContent = getPaymentMethodText(invoice.paymentMethod);
    
    // Chi tiết chi phí
    document.getElementById('viewRoomRent').textContent = formatCurrency(invoice.roomRent || 0);
    
    // Điện
    document.getElementById('viewElectricUsage').textContent = (invoice.electricUsage || 0) + ' kWh';
    document.getElementById('viewElectricReading').textContent = `${invoice.electricOldReading || 0} → ${invoice.electricNewReading || 0}`;
    document.getElementById('viewElectricAmount').textContent = formatCurrency(invoice.electricAmount || 0);
    
    // Nước
    document.getElementById('viewWaterUsage').textContent = (invoice.waterUsage || 0) + ' m³';
    document.getElementById('viewWaterReading').textContent = `${invoice.waterOldReading || 0} → ${invoice.waterNewReading || 0}`;
    document.getElementById('viewWaterAmount').textContent = formatCurrency(invoice.waterAmount || 0);
    
    // Internet
    document.getElementById('viewInternetFee').textContent = formatCurrency(invoice.internetFee || 0);
    
    // Tổng
    document.getElementById('viewTotalAmount').textContent = formatCurrency(invoice.totalAmount || 0);
    
    // Phí khác
    const otherFeesSection = document.getElementById('viewOtherFeesSection');
    if (invoice.otherFees) {
        document.getElementById('viewOtherFees').textContent = invoice.otherFees;
        otherFeesSection.style.display = 'block';
    } else {
        otherFeesSection.style.display = 'none';
    }
    
    // Ghi chú
    const notesSection = document.getElementById('viewNotesSection');
    if (invoice.notes) {
        document.getElementById('viewNotes').textContent = invoice.notes;
        notesSection.style.display = 'block';
    } else {
        notesSection.style.display = 'none';
    }
    
    // Hiển thị modal
    const modal = new bootstrap.Modal(document.getElementById('viewInvoiceModal'));
    modal.show();
}

function getPaymentMethodText(method) {
    if (!method) return 'N/A';
    
    const methods = {
        'cash': 'Tiền mặt',
        'bank_transfer': 'Chuyển khoản',
        'momo': 'MoMo',
        'zalopay': 'ZaloPay'
    };
    
    return methods[method] || method;
}

function formatDateTime(dateString) {
    const date = new Date(dateString);
    return date.toLocaleString('vi-VN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit'
    });
}

function printInvoice() {
    const modal = document.getElementById('viewInvoiceModal');
    const modalContent = modal.querySelector('.modal-content');
    
    // Tạo cửa sổ in mới
    const printWindow = window.open('', '_blank');
    
    // Lấy dữ liệu từ modal
    const invoiceId = document.getElementById('viewInvoiceId').textContent;
    const invoiceMonth = document.getElementById('viewInvoiceMonth').textContent;
    const createdAt = document.getElementById('viewCreatedAt').textContent;
    const dueDate = document.getElementById('viewDueDate').textContent;
    const status = document.getElementById('viewStatus').innerHTML;
    const propertyTitle = document.getElementById('viewPropertyTitle').textContent;
    const renterName = document.getElementById('viewRenterName').textContent;
    const paymentDate = document.getElementById('viewPaymentDate').textContent;
    const paymentMethod = document.getElementById('viewPaymentMethod').textContent;
    
    const roomRent = document.getElementById('viewRoomRent').textContent;
    const electricUsage = document.getElementById('viewElectricUsage').textContent;
    const electricReading = document.getElementById('viewElectricReading').textContent;
    const electricAmount = document.getElementById('viewElectricAmount').textContent;
    const waterUsage = document.getElementById('viewWaterUsage').textContent;
    const waterReading = document.getElementById('viewWaterReading').textContent;
    const waterAmount = document.getElementById('viewWaterAmount').textContent;
    const internetFee = document.getElementById('viewInternetFee').textContent;
    const totalAmount = document.getElementById('viewTotalAmount').textContent;
    
    const otherFees = document.getElementById('viewOtherFees').textContent;
    const notes = document.getElementById('viewNotes').textContent;
    const hasOtherFees = document.getElementById('viewOtherFeesSection').style.display !== 'none';
    const hasNotes = document.getElementById('viewNotesSection').style.display !== 'none';
    
    // HTML cho trang in
    const printHTML = `
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset="UTF-8">
            <title>Hóa đơn ${invoiceId}</title>
            <style>
                * {
                    margin: 0;
                    padding: 0;
                    box-sizing: border-box;
                }
                
                body {
                    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                    padding: 20px;
                    color: #333;
                }
                
                .invoice-container {
                    max-width: 800px;
                    margin: 0 auto;
                    background: white;
                }
                
                .invoice-header {
                    text-align: center;
                    border-bottom: 3px solid #0d6efd;
                    padding-bottom: 20px;
                    margin-bottom: 30px;
                }
                
                .invoice-header h1 {
                    color: #0d6efd;
                    font-size: 32px;
                    margin-bottom: 10px;
                }
                
                .invoice-header p {
                    color: #666;
                    font-size: 14px;
                }
                
                .invoice-info {
                    display: flex;
                    justify-content: space-between;
                    margin-bottom: 30px;
                }
                
                .info-section {
                    flex: 1;
                }
                
                .info-section h3 {
                    color: #0d6efd;
                    font-size: 16px;
                    margin-bottom: 10px;
                    border-bottom: 2px solid #e9ecef;
                    padding-bottom: 5px;
                }
                
                .info-row {
                    display: flex;
                    padding: 5px 0;
                    font-size: 14px;
                }
                
                .info-label {
                    color: #666;
                    width: 140px;
                    font-weight: 500;
                }
                
                .info-value {
                    color: #333;
                    font-weight: 600;
                }
                
                .invoice-table {
                    width: 100%;
                    border-collapse: collapse;
                    margin-bottom: 20px;
                }
                
                .invoice-table thead {
                    background: #f8f9fa;
                }
                
                .invoice-table th {
                    padding: 12px;
                    text-align: left;
                    font-weight: 600;
                    color: #495057;
                    border: 1px solid #dee2e6;
                    font-size: 14px;
                }
                
                .invoice-table td {
                    padding: 10px 12px;
                    border: 1px solid #dee2e6;
                    font-size: 14px;
                }
                
                .invoice-table .text-end {
                    text-align: right;
                }
                
                .invoice-table tfoot {
                    background: #f8f9fa;
                }
                
                .invoice-table tfoot td {
                    font-weight: 700;
                    font-size: 16px;
                    color: #0d6efd;
                }
                
                .additional-info {
                    margin-top: 20px;
                    padding: 15px;
                    background: #f8f9fa;
                    border-radius: 5px;
                }
                
                .additional-info h4 {
                    color: #495057;
                    font-size: 14px;
                    margin-bottom: 8px;
                }
                
                .additional-info p {
                    color: #666;
                    font-size: 13px;
                    line-height: 1.6;
                }
                
                .invoice-footer {
                    margin-top: 40px;
                    padding-top: 20px;
                    border-top: 2px solid #e9ecef;
                    text-align: center;
                    color: #666;
                    font-size: 12px;
                }
                
                .status-badge {
                    display: inline-block;
                    padding: 4px 12px;
                    border-radius: 4px;
                    font-size: 12px;
                    font-weight: 600;
                }
                
                .badge.bg-warning {
                    background-color: #ffc107 !important;
                    color: #000;
                }
                
                .badge.bg-success {
                    background-color: #198754 !important;
                    color: white;
                }
                
                .badge.bg-danger {
                    background-color: #dc3545 !important;
                    color: white;
                }
                
                @media print {
                    body {
                        padding: 0;
                    }
                    
                    .invoice-container {
                        max-width: 100%;
                    }
                }
            </style>
        </head>
        <body>
            <div class="invoice-container">
                <div class="invoice-header">
                    <h1>HÓA ĐƠN THANH TOÁN</h1>
                    <p>NestFlow - Hệ thống quản lý cho thuê phòng trọ</p>
                </div>
                
                <div class="invoice-info">
                    <div class="info-section">
                        <h3>Thông tin chung</h3>
                        <div class="info-row">
                            <span class="info-label">Mã hóa đơn:</span>
                            <span class="info-value">${invoiceId}</span>
                        </div>
                        <div class="info-row">
                            <span class="info-label">Tháng:</span>
                            <span class="info-value">${invoiceMonth}</span>
                        </div>
                        <div class="info-row">
                            <span class="info-label">Ngày tạo:</span>
                            <span class="info-value">${createdAt}</span>
                        </div>
                        <div class="info-row">
                            <span class="info-label">Hạn thanh toán:</span>
                            <span class="info-value">${dueDate}</span>
                        </div>
                        <div class="info-row">
                            <span class="info-label">Trạng thái:</span>
                            <span class="info-value">${status}</span>
                        </div>
                    </div>
                    
                    <div class="info-section">
                        <h3>Thông tin thuê</h3>
                        <div class="info-row">
                            <span class="info-label">Phòng:</span>
                            <span class="info-value">${propertyTitle}</span>
                        </div>
                        <div class="info-row">
                            <span class="info-label">Người thuê:</span>
                            <span class="info-value">${renterName}</span>
                        </div>
                        <div class="info-row">
                            <span class="info-label">Ngày thanh toán:</span>
                            <span class="info-value">${paymentDate}</span>
                        </div>
                        <div class="info-row">
                            <span class="info-label">Phương thức:</span>
                            <span class="info-value">${paymentMethod}</span>
                        </div>
                    </div>
                </div>
                
                <table class="invoice-table">
                    <thead>
                        <tr>
                            <th>Khoản thu</th>
                            <th class="text-end">Số lượng</th>
                            <th class="text-end">Đơn giá</th>
                            <th class="text-end">Thành tiền</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td>Tiền thuê phòng</td>
                            <td class="text-end">-</td>
                            <td class="text-end">-</td>
                            <td class="text-end">${roomRent}</td>
                        </tr>
                        <tr>
                            <td>Tiền điện</td>
                            <td class="text-end">${electricUsage}</td>
                            <td class="text-end">${electricReading}</td>
                            <td class="text-end">${electricAmount}</td>
                        </tr>
                        <tr>
                            <td>Tiền nước</td>
                            <td class="text-end">${waterUsage}</td>
                            <td class="text-end">${waterReading}</td>
                            <td class="text-end">${waterAmount}</td>
                        </tr>
                        <tr>
                            <td>Phí internet</td>
                            <td class="text-end">-</td>
                            <td class="text-end">-</td>
                            <td class="text-end">${internetFee}</td>
                        </tr>
                    </tbody>
                    <tfoot>
                        <tr>
                            <td colspan="3" class="text-end">Tổng cộng:</td>
                            <td class="text-end">${totalAmount}</td>
                        </tr>
                    </tfoot>
                </table>
                
                ${hasOtherFees ? `
                <div class="additional-info">
                    <h4>Phí khác</h4>
                    <p>${otherFees}</p>
                </div>
                ` : ''}
                
                ${hasNotes ? `
                <div class="additional-info">
                    <h4>Ghi chú</h4>
                    <p>${notes}</p>
                </div>
                ` : ''}
                
                <div class="invoice-footer">
                    <p>Cảm ơn quý khách đã sử dụng dịch vụ!</p>
                    <p>Hóa đơn được in tự động từ hệ thống NestFlow - ${new Date().toLocaleString('vi-VN')}</p>
                </div>
            </div>
            
            <script>
                window.onload = function() {
                    window.print();
                    // Đóng cửa sổ sau khi in (tùy chọn)
                    // window.onafterprint = function() { window.close(); }
                }
            </script>
        </body>
        </html>
    `;
    
    printWindow.document.write(printHTML);
    printWindow.document.close();
}

function editInvoice(invoiceId) {
    // TODO: Implement edit invoice
    showMessage('Chức năng sửa hóa đơn đang phát triển', 'info');
}
