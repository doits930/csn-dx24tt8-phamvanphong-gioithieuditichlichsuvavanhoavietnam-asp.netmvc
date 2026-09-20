(function ($) {
    'use strict';

    if (!$) {
        return;
    }

    var SUCCESS_DELAY = 6000;

    function startToasts() {
        if (!window.bootstrap || !window.bootstrap.Toast) {
            return;
        }

        Array.prototype.forEach.call(document.querySelectorAll('[data-admin-toast]'), function (element) {
            var autohide = element.getAttribute('data-toast-autohide') === 'true';
            var toast = new window.bootstrap.Toast(element, {
                autohide: autohide,
                delay: SUCCESS_DELAY
            });

            toast.show();
        });
    }

    $(function () {
        startToasts();

        var sidebar = document.getElementById('admin-sidebar');
        var menuButton = document.querySelector('.admin-menu-button');
        if (sidebar && menuButton) {
            sidebar.addEventListener('hidden.bs.offcanvas', function () {
                menuButton.focus();
            });
        }

        var search = document.querySelector('[data-search-shortcut]');
        if (!search) {
            return;
        }

        $(document).on('keydown', function (event) {
            if (event.key !== '/' || event.ctrlKey || event.altKey || event.metaKey) {
                return;
            }

            var active = document.activeElement;
            var tag = active ? active.tagName.toLowerCase() : '';
            if (tag === 'input' || tag === 'textarea' || tag === 'select') {
                return;
            }

            event.preventDefault();
            search.focus();
            search.select();
        });
    });
})(window.jQuery);
