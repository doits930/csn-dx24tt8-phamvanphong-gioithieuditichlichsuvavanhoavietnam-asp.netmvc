(function ($) {
    'use strict';

    if (!$) {
        return;
    }

    function toSlug(value) {
        return $.trim(value)
            .replace(/đ/g, 'd')
            .replace(/Đ/g, 'D')
            .normalize('NFD')
            .replace(/[̀-ͯ]/g, '')
            .toLowerCase()
            .replace(/[^a-z0-9]+/g, '-')
            .replace(/^-+|-+$/g, '');
    }

    function setUpSlugPreview(form) {
        var source = form.find('[data-slug-source]');
        var preview = form.find('[data-slug-preview]');
        if (source.length === 0 || preview.length === 0) {
            return;
        }

        var original = $.trim(preview.text());

        var update = function () {
            var slug = toSlug(source.val());
            preview.text(slug.length > 0 ? slug : original);
        };

        source.on('input', update);
        update();
    }

    function setUpCounters(form) {
        form.find('[data-counter]').each(function () {
            var counter = $(this);
            var field = counter.closest('.admin-field').find('input, textarea').first();
            var limit = parseInt(field.attr('maxlength'), 10);
            if (field.length === 0 || isNaN(limit)) {
                return;
            }

            var update = function () {
                counter.text(field.val().length + '/' + limit);
            };

            field.on('input', update);
            update();
        });
    }

    function setUpAutoGrow(form) {
        form.find('[data-autogrow]').each(function () {
            var area = this;
            var grow = function () {
                area.style.height = 'auto';
                area.style.height = Math.min(area.scrollHeight + 2, 480) + 'px';
            };

            $(area).on('input', grow);
            grow();
        });
    }

    function setUpSubmitState(form) {
        var dirty = false;
        var state = form.find('[data-form-state]');
        var submitButton = form.find('[data-submit-button]');
        var submitLabel = submitButton.text();

        form.on('input change', 'input, select, textarea', function () {
            if (dirty) {
                return;
            }

            dirty = true;
            state.text('Chưa lưu thay đổi.');
        });

        form.on('submit', function () {
            if (form.data('validator') && !form.valid()) {
                return;
            }

            dirty = false;
            state.text('Đang lưu…');
            submitButton.prop('disabled', true).text('Đang lưu…');
        });

        $(document).on('click', '[data-discard]', function () {
            dirty = false;
        });

        $(window).on('pageshow', function () {
            submitButton.prop('disabled', false).text(submitLabel);
        });

        $(window).on('beforeunload', function (event) {
            if (!dirty) {
                return undefined;
            }

            event.preventDefault();
            event.originalEvent.returnValue = '';
            return '';
        });
    }

    function removeEmptyError(element) {
        var errorId = (element.id || element.name) + '-error';
        var error = document.getElementById(errorId);
        if (!error || $.trim(error.textContent) !== '') {
            return;
        }

        var field = $(element);
        var described = (field.attr('aria-describedby') || '').split(/\s+/).filter(function (id) {
            return id.length > 0 && id !== errorId;
        });

        if (described.length > 0) {
            field.attr('aria-describedby', described.join(' '));
        } else {
            field.removeAttr('aria-describedby');
        }

        error.parentNode.removeChild(error);
    }

    function focusFirstError(form) {
        var field = form.find('.admin-field-invalid').first().find('input, select, textarea').first();
        if (field.length === 0) {
            return;
        }

        var element = field[0];
        if (element.scrollIntoView) {
            element.scrollIntoView({ block: 'center' });
        }

        element.focus({ preventScroll: true });
    }

    function buildRules(form) {
        var rules = { Name: { required: true, maxlength: 100, nameHasLetterOrDigit: true } };
        var messages = {};

        if (form.find('#field-region').length > 0) {
            rules.Region = { required: true };
            messages.Name = {
                required: 'Nhập tên tỉnh thành.',
                maxlength: 'Tên tỉnh thành tối đa 100 ký tự.',
                nameHasLetterOrDigit: 'Tên tỉnh thành phải có ít nhất một chữ cái hoặc chữ số.'
            };
            messages.Region = { required: 'Chọn miền của tỉnh thành.' };
        } else {
            rules.Description = { maxlength: 500 };
            messages.Name = {
                required: 'Nhập tên loại di tích.',
                maxlength: 'Tên loại di tích tối đa 100 ký tự.',
                nameHasLetterOrDigit: 'Tên loại di tích phải có ít nhất một chữ cái hoặc chữ số.'
            };
            messages.Description = { maxlength: 'Mô tả tối đa 500 ký tự.' };
        }

        return { rules: rules, messages: messages };
    }

    function setUpValidation(form) {
        if (!$.validator) {
            return;
        }

        $.validator.addMethod('nameHasLetterOrDigit', function (value, element) {
            return this.optional(element)
                || (/[\p{L}\p{N}]/u.test(value) && toSlug(value).length > 0);
        });

        var declared = buildRules(form);

        form.validate({
            onkeyup: false,
            errorElement: 'p',
            errorClass: 'field-error',
            rules: declared.rules,
            messages: declared.messages,
            errorPlacement: function (error, element) {
                error.appendTo(element.closest('.admin-field'));
            },
            highlight: function (element) {
                $(element).attr('aria-invalid', 'true').closest('.admin-field').addClass('admin-field-invalid');
            },
            unhighlight: function (element) {
                $(element).attr('aria-invalid', 'false').closest('.admin-field').removeClass('admin-field-invalid');
                window.setTimeout(function () {
                    removeEmptyError(element);
                }, 0);
            },
            invalidHandler: function () {
                window.setTimeout(function () {
                    focusFirstError($('#catalog-form'));
                }, 0);
            }
        });
    }

    $(function () {
        var form = $('#catalog-form');
        if (form.length === 0) {
            return;
        }

        setUpSlugPreview(form);
        setUpCounters(form);
        setUpAutoGrow(form);
        setUpSubmitState(form);
        setUpValidation(form);
        focusFirstError(form);
    });
})(window.jQuery);
