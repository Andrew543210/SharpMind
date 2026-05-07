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

// ===== ПЕРЕМИКАЧ ТЕМ =====
const THEME_STORAGE_KEY = 'sharpmind-theme';
const DEFAULT_THEME = 'light-green';

// Ініціалізація теми при завантаженні сторінки
document.addEventListener('DOMContentLoaded', function() {
    initializeTheme();
    setupThemeSwitcher();
});

function initializeTheme() {
    const savedTheme = localStorage.getItem(THEME_STORAGE_KEY) || DEFAULT_THEME;
    applyTheme(savedTheme);
}

function applyTheme(themeName) {
    // Видалити всі класи теми
    document.body.classList.remove(
        'theme-light-green',
        'theme-dark-blue',
        'theme-beige-brown',
        'theme-purple',
        'theme-orange'
    );
    
    // Застосувати нову тему
    document.body.classList.add(`theme-${themeName}`);
    
    // Зберегти вибір в localStorage
    localStorage.setItem(THEME_STORAGE_KEY, themeName);
    
    // Оновити активну кнопку перемикача
    updateThemeSwitcherUI(themeName);
}

function updateThemeSwitcherUI(themeName) {
    const themeColors = document.querySelectorAll('.theme-color');
    themeColors.forEach(color => {
        color.classList.remove('active');
        if (color.dataset.theme === themeName) {
            color.classList.add('active');
        }
    });
}

function setupThemeSwitcher() {
    const themeColors = document.querySelectorAll('.theme-color');
    themeColors.forEach(color => {
        color.addEventListener('click', function() {
            const themeName = this.dataset.theme;
            applyTheme(themeName);
        });
    });
    
    // Встановити активний перемикач при завантаженні
    const currentTheme = localStorage.getItem(THEME_STORAGE_KEY) || DEFAULT_THEME;
    updateThemeSwitcherUI(currentTheme);
}
