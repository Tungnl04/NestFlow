"use strict";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .build();

// Start connection
connection.start().then(function () {
    console.log("SignalR Connected.");
}).catch(function (err) {
    return console.error(err.toString());
});

// Receive Notification
connection.on("ReceiveNotification", function (notification) {
    console.log("Received notification: ", notification);

    // Update Counter
    let counter = document.getElementById("notif-counter");
    let currentCount = parseInt(counter.innerText) || 0;
    counter.innerText = currentCount + 1;
    counter.classList.remove("d-none");

    // Add to list (Toast or Dropdown)
    showToast(notification.title, notification.content);
    addToDropdown(notification);
});

function addToDropdown(notification) {
    const list = document.querySelector('#notificationBell .dropdown-menu');
    if (!list) return;

    // Remove "empty" message if it exists
    const emptyMsg = list.querySelector('.text-muted.small.text-center');
    if (emptyMsg) emptyMsg.remove();
    if (list.querySelector('li.text-center.text-muted')) list.querySelector('li.text-center.text-muted').remove();

    // Create new item
    const li = document.createElement('li');
    li.innerHTML = `
        <a class="dropdown-item" href="${notification.linkUrl || '#'}">
            <div class="d-flex flex-column">
                <span class="fw-bold small">${notification.title}</span>
                <span class="small text-muted text-truncate" style="max-width: 250px;">${notification.content}</span>
                <span class="text-xs text-muted text-end mt-1" style="font-size: 0.7rem;">Vừa xong</span>
            </div>
        </a>
    `;

    // Insert after the divider (assuming index 2: header, divider, ...)
    // Or just append to list if we strictly control structure. 
    // Let's look for the divider.
    const divider = list.querySelector('hr.dropdown-divider');
    if (divider && divider.parentElement) {
        divider.parentElement.after(li);
    } else {
        list.appendChild(li);
    }
}

function showToast(title, message) {
    // Simple toast implementation (requires Bootstrap Toast container in Layout)
    const toastHtml = `
        <div class="toast" role="alert" aria-live="assertive" aria-atomic="true">
          <div class="toast-header bg-success text-white">
            <strong class="me-auto"><i class="fas fa-bell me-2"></i>${title}</strong>
            <small class="text-white">Vừa xong</small>
            <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast" aria-label="Close"></button>
          </div>
          <div class="toast-body">
            ${message}
          </div>
        </div>
    `;

    const container = document.getElementById("toast-container");
    if (container) {
        container.insertAdjacentHTML('beforeend', toastHtml);
        const newToast = container.lastElementChild;
        const toast = new bootstrap.Toast(newToast);
        toast.show();
    }
}
