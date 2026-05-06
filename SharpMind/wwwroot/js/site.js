// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Функції для управління користувацькими модалями (замість Bootstrap для запобігання мигання)
function openCustomModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.classList.add('show');
        modal.style.display = 'flex';
    }
}

function closeCustomModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.classList.remove('show');
        modal.style.display = 'none';
    }
}

// Закриття модалі при кліку поза нею
document.addEventListener('click', function(event) {
    // Закрити модаль при кліку на фон
    if (event.target.classList.contains('custom-modal')) {
        closeCustomModal(event.target.id);
    }
});

// Закриття модалі при натисканні Escape
document.addEventListener('keydown', function(event) {
    if (event.key === 'Escape') {
        // Закрити всі відкриті модалі
        document.querySelectorAll('.custom-modal.show').forEach(modal => {
            closeCustomModal(modal.id);
        });
    }
});

