(function ($) {
    'use strict';

    if (!$) {
        return;
    }

    $(function () {
        var dialog = document.querySelector('[data-confirm-dialog]');
        if (!dialog || typeof dialog.showModal !== 'function') {
            return;
        }

        var form = dialog.querySelector('[data-confirm-form]');
        var message = dialog.querySelector('[data-confirm-message]');
        var title = dialog.querySelector('[data-confirm-title]');
        var submit = dialog.querySelector('[data-confirm-submit]');
        var defaultTitle = title ? title.textContent : '';
        var defaultLabel = submit ? submit.textContent : '';
        var lastTrigger = null;

        $(document).on('click', '[data-delete-trigger]', function (event) {
            event.preventDefault();
            lastTrigger = this;
            form.setAttribute('action', this.getAttribute('data-delete-url'));
            message.textContent = this.getAttribute('data-delete-message') || '';

            if (title) {
                title.textContent = this.getAttribute('data-delete-title') || defaultTitle;
            }

            if (submit) {
                submit.textContent = this.getAttribute('data-delete-label') || defaultLabel;
            }

            dialog.showModal();
        });

        $(dialog).on('click', '[data-confirm-cancel]', function () {
            dialog.close();
        });

        $(dialog).on('click', function (event) {
            if (event.target === dialog) {
                dialog.close();
            }
        });

        $(dialog).on('close', function () {
            if (lastTrigger) {
                lastTrigger.focus();
            }
        });
    });
})(window.jQuery);
