// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

const NOTIFICATION_LIMIT = 10;
let newsNotificationConnection = null;

document.addEventListener("DOMContentLoaded", function () {
    loadRecentNotifications();
    renderNotificationDropdown();
    updateNotificationBell();
    initializeNewsNotificationConnection();
});

function initializeNewsNotificationConnection() {
    if (window.newsNotificationConnection) return;

    const coreApiUrl = document.body.getAttribute('data-core-api-url');
    if (!coreApiUrl) {
        console.warn("Core API URL not found. SignalR connection aborted.");
        return;
    }

    const hubUrl = coreApiUrl + "hubs/notifications";

    window.newsNotificationConnection = new signalR.HubConnectionBuilder()
        .withUrl(hubUrl)
        .withAutomaticReconnect()
        .build();

    window.newsNotificationConnection.on("NewsCreated", function (payload) {
        handleNewsCreatedNotification(payload);
    });

    window.newsNotificationConnection.onreconnecting(function (error) {
        console.warn("SignalR reconnecting...", error);
    });

    window.newsNotificationConnection.onreconnected(function (connectionId) {
        console.log("SignalR reconnected.");
    });

    window.newsNotificationConnection.start()
        .then(() => {
            console.log("SignalR connected successfully.");
        })
        .catch(err => {
            console.warn("SignalR connection failed. UI will continue to function normally.", err);
        });
}

function handleNewsCreatedNotification(payload) {
    const notification = mapNewsCreatedEventToNotification(payload);
    
    if (isDuplicateNotification(notification)) {
        return;
    }

    saveRecentNotification(notification);
    renderNotificationDropdown();
    updateNotificationBell();
    showNewsCreatedToast(notification);
}

function mapNewsCreatedEventToNotification(payload) {
    return {
        newsArticleId: payload.id || payload.newsArticleId || payload.NewsArticleId || "",
        title: payload.title || payload.newsTitle || payload.NewsTitle || "New Article",
        categoryName: payload.category || payload.categoryName || payload.CategoryName || null,
        authorName: payload.author || payload.authorName || payload.AuthorName || null,
        createdAt: payload.createdAt || payload.CreatedDate || new Date().toISOString(),
        isRead: false
    };
}

function getStorageKey() {
    const accountId = document.body.getAttribute('data-account-id') || 'Guest';
    return `FUNewsRecentNotifications_${accountId}`;
}

function isDuplicateNotification(notification) {
    const notifications = loadRecentNotifications();
    // Identify by newsArticleId
    return notifications.some(n => n.newsArticleId === notification.newsArticleId);
}

function loadRecentNotifications() {
    const key = getStorageKey();
    const data = localStorage.getItem(key);
    if (!data) return [];

    try {
        const parsed = JSON.parse(data);
        if (Array.isArray(parsed)) {
            return parsed;
        }
        return [];
    } catch (e) {
        console.warn("localStorage corruption detected. Resetting notifications.", e);
        localStorage.removeItem(key);
        return [];
    }
}

function saveRecentNotification(notification) {
    let notifications = loadRecentNotifications();
    
    // newest first
    notifications.unshift(notification);

    if (notifications.length > NOTIFICATION_LIMIT) {
        notifications = notifications.slice(0, NOTIFICATION_LIMIT);
    }

    const key = getStorageKey();
    localStorage.setItem(key, JSON.stringify(notifications));
}

function renderNotificationDropdown() {
    const notifications = loadRecentNotifications();
    const dropdown = document.getElementById('notificationList');
    if (!dropdown) return;

    dropdown.innerHTML = '';

    if (notifications.length === 0) {
        dropdown.innerHTML = '<li><span class="dropdown-item text-muted text-center">No notifications</span></li>';
        return;
    }

    notifications.forEach(n => {
        const li = document.createElement('li');
        
        let meta = [];
        if (n.categoryName) meta.push(n.categoryName);
        if (n.authorName) meta.push(n.authorName);
        const metaText = meta.length > 0 ? meta.join(' &bull; ') + '<br>' : '';
        
        const dateObj = new Date(n.createdAt);
        const timeText = dateObj.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

        const unreadClass = n.isRead ? '' : 'fw-bold bg-light';

        li.innerHTML = `
            <a class="dropdown-item border-bottom py-2 notification-item ${unreadClass}" href="/Home/Detail/${n.newsArticleId}">
                <div class="d-flex justify-content-between align-items-start">
                    <div class="text-truncate" style="max-width: 200px;">
                        ${n.title}
                    </div>
                    <small class="text-muted ms-2" style="font-size: 0.75rem;">${timeText}</small>
                </div>
                <small class="text-muted">${metaText}</small>
            </a>
        `;
        dropdown.appendChild(li);
    });
}

function updateNotificationBell() {
    const notifications = loadRecentNotifications();
    const unreadCount = notifications.filter(n => !n.isRead).length;
    const badge = document.getElementById('notificationBadge');
    if (!badge) return;

    if (unreadCount > 0) {
        badge.innerText = unreadCount > 9 ? '9+' : unreadCount;
        badge.style.display = 'block';
    } else {
        badge.style.display = 'none';
    }
}

function markNotificationsAsRead() {
    const notifications = loadRecentNotifications();
    let changed = false;

    notifications.forEach(n => {
        if (!n.isRead) {
            n.isRead = true;
            changed = true;
        }
    });

    if (changed) {
        const key = getStorageKey();
        localStorage.setItem(key, JSON.stringify(notifications));
        renderNotificationDropdown();
        updateNotificationBell();
    }
}

function showNewsCreatedToast(notification) {
    const toastEl = document.getElementById('globalNotificationToast');
    const toastBody = document.getElementById('globalNotificationToastBody');
    if (!toastEl || !toastBody) return;

    toastBody.innerHTML = `
        <div>New article published!</div>
        <strong>${notification.title}</strong>
        <div class="mt-2 text-end">
            <a href="/Home/Detail/${notification.newsArticleId}" class="btn btn-sm btn-primary">View</a>
        </div>
    `;

    const toast = new bootstrap.Toast(toastEl, { delay: 5000 });
    toast.show();
}

// Global UX Functions

function showLoading() {
    const overlay = document.getElementById('globalLoadingOverlay');
    if (overlay) {
        overlay.classList.remove('d-none');
        overlay.classList.add('d-flex');
    }
}

function hideLoading() {
    const overlay = document.getElementById('globalLoadingOverlay');
    if (overlay) {
        overlay.classList.remove('d-flex');
        overlay.classList.add('d-none');
    }
}

function showAppToast(title, message, type = 'info') {
    const toastEl = document.getElementById('globalAppToast');
    const toastTitle = document.getElementById('globalAppToastTitle');
    const toastBody = document.getElementById('globalAppToastBody');
    const toastHeader = document.getElementById('globalAppToastHeader');
    
    if (!toastEl) return;

    toastTitle.textContent = title;
    toastBody.innerHTML = message;
    
    // Clear previous colors
    toastHeader.className = 'toast-header text-white';
    
    switch (type) {
        case 'success':
            toastHeader.classList.add('bg-success');
            break;
        case 'error':
        case 'danger':
            toastHeader.classList.add('bg-danger');
            break;
        case 'warning':
            toastHeader.classList.add('bg-warning', 'text-dark');
            toastHeader.classList.remove('text-white');
            break;
        default:
            toastHeader.classList.add('bg-primary');
            break;
    }

    const toast = new bootstrap.Toast(toastEl, { delay: 5000 });
    toast.show();
}

let globalConfirmCallback = null;

function confirmAction(message, callback) {
    const modalEl = document.getElementById('globalConfirmModal');
    if (!modalEl) {
        // Fallback if modal not present
        if (confirm(message)) {
            callback();
        }
        return;
    }
    
    document.getElementById('globalConfirmModalMessage').textContent = message;
    globalConfirmCallback = callback;
    
    const modal = new bootstrap.Modal(modalEl);
    modal.show();
}

document.addEventListener("DOMContentLoaded", function () {
    const confirmBtn = document.getElementById('globalConfirmModalBtn');
    if (confirmBtn) {
        confirmBtn.addEventListener('click', function () {
            if (globalConfirmCallback) {
                globalConfirmCallback();
            }
            const modalEl = document.getElementById('globalConfirmModal');
            const modal = bootstrap.Modal.getInstance(modalEl);
            if (modal) {
                modal.hide();
            }
        });
    }
    
    // Attach loading to all forms by default, except those explicitly opting out
    document.querySelectorAll('form').forEach(form => {
        if (!form.classList.contains('no-loading')) {
            form.addEventListener('submit', function (e) {
                if (!e.defaultPrevented && form.checkValidity()) {
                    showLoading();
                }
            });
        }
    });
});
