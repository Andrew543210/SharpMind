// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Обробка форм в модальних вікнах для запобігання мигання
document.addEventListener('DOMContentLoaded', function() {
    // Знаходимо всі форми в модалях
    const modalForms = document.querySelectorAll('.modal form');
    
    modalForms.forEach(form => {
        form.addEventListener('submit', async function(e) {
            e.preventDefault();

            const formData = new FormData(this);
            const actionUrl = this.getAttribute('action');
            const method = this.getAttribute('method') || 'post';
            const modalElement = this.closest('.modal');
            const modalInstance = modalElement ? window.bootstrap.Modal.getInstance(modalElement) || new window.bootstrap.Modal(modalElement) : null;

            try {
                const response = await fetch(actionUrl, {
                    method: method.toUpperCase(),
                    body: formData
                });

                if (response.ok) {
                    // Закриємо модаль
                    if (modalInstance) {
                        modalInstance.hide();
                    }
                    // Перезавантажимо сторінку після невеликої затримки
                    setTimeout(() => {
                        location.reload();
                    }, 300);
                } else {
                    alert('Помилка при збереженні даних.');
                }
            } catch (error) {
                console.error('Помилка:', error);
                alert('Помилка при відправці форми.');
            }
        });
    });
});

