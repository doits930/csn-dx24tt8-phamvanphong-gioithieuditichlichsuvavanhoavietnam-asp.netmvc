(function ($) {
    'use strict';

    if (!$) {
        return;
    }

    $(function () {
        $(document).on('click', '[data-alert-close]', function () {
            $(this).closest('.admin-alert').remove();
        });

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
