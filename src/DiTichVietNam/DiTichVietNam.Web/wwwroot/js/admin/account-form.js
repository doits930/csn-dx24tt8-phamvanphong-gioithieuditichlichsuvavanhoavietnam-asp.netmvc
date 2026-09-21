(function ($) {
    'use strict';

    if (!$) {
        return;
    }

    var LOWERCASE = /\p{Ll}/u;
    var UPPERCASE = /\p{Lu}/u;
    var DIGIT = /\p{Nd}/u;
    var SYMBOL = /[^\p{L}\p{N}]/u;

    function isRuleMet(code, threshold, value) {
        switch (code) {
            case 'length':
                return value.length >= threshold;
            case 'lowercase':
                return LOWERCASE.test(value);
            case 'uppercase':
                return UPPERCASE.test(value);
            case 'digit':
                return DIGIT.test(value);
            case 'symbol':
                return SYMBOL.test(value);
            case 'unique':
                return new Set(Array.from(value)).size >= threshold;
            default:
                return true;
        }
    }

    function panelFor(fieldId) {
        return $('[data-password-rules][data-password-for="' + fieldId + '"]');
    }

    function missingLabels(fieldId, value) {
        var missing = [];

        panelFor(fieldId).find('.password-rule').each(function () {
            var item = $(this);
            var threshold = parseInt(item.attr('data-rule-value'), 10) || 0;
            if (!isRuleMet(item.attr('data-rule'), threshold, value)) {
                missing.push(item.attr('data-rule-label'));
            }
        });

        return missing;
    }

    function setUpChecklists(form) {
        form.find('[data-password-rules]').each(function () {
            var panel = $(this);
            var field = $('#' + panel.attr('data-password-for'));
            var status = panel.find('[data-password-status]');
            if (field.length === 0) {
                return;
            }

            var update = function () {
                var value = field.val() || '';
                var missing = [];

                panel.find('.password-rule').each(function () {
                    var item = $(this);
                    var threshold = parseInt(item.attr('data-rule-value'), 10) || 0;
                    var met = value.length > 0 && isRuleMet(item.attr('data-rule'), threshold, value);
                    item.toggleClass('is-met', met);
                    if (!met) {
                        missing.push(item.attr('data-rule-label'));
                    }
                });

                if (value.length === 0) {
                    status.text('');
                } else if (missing.length === 0) {
                    status.text('Mật khẩu đã đạt mọi yêu cầu.');
                } else {
                    status.text('Mật khẩu còn thiếu: ' + missing.join(', ') + '.');
                }
            };

            field.on('input', update);
            update();
        });
    }

    function setUpToggles(form) {
        form.on('click', '[data-password-toggle]', function () {
            var button = $(this);
            var field = $('#' + button.attr('aria-controls'));
            var shown = field.attr('type') === 'text';
            field.attr('type', shown ? 'password' : 'text');
            button.attr('aria-pressed', shown ? 'false' : 'true').text(shown ? 'Hiện' : 'Ẩn');
        });
    }

    function setUpCounters(form) {
        form.find('[data-counter]').each(function () {
            var counter = $(this);
            var field = counter.closest('.admin-field').find('input').first();
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

    function setUpSubmitState(form) {
        var state = form.find('[data-form-state]');
        var submitButton = form.find('[data-submit-button]');
        var submitLabel = submitButton.text();

        form.on('submit', function () {
            if (form.data('validator') && !form.valid()) {
                return;
            }

            state.text('Đang lưu...');
            submitButton.prop('disabled', true).text('Đang lưu...');
        });

        $(window).on('pageshow', function () {
            submitButton.prop('disabled', false).text(submitLabel);
        });
    }

    function focusFirstError(form) {
        var field = form.find('.admin-field-invalid').first().find('input').first();
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
        var rules = {};
        var messages = {};

        if (form.find('#field-email').length > 0) {
            rules.Email = { required: true, maxlength: 256, allowedEmailCharacters: true, accountEmail: true };
            messages.Email = {
                required: 'Nhập địa chỉ thư điện tử.',
                maxlength: 'Địa chỉ thư điện tử tối đa 256 ký tự.',
                allowedEmailCharacters: 'Địa chỉ email chỉ gồm chữ cái không dấu, chữ số và các ký tự . _ - + @',
                accountEmail: 'Địa chỉ thư điện tử chưa đúng dạng.'
            };
        }

        if (form.find('#field-current-password').length > 0) {
            rules.CurrentPassword = { required: true, maxlength: 100 };
            messages.CurrentPassword = {
                required: 'Nhập mật khẩu hiện tại.',
                maxlength: 'Mật khẩu tối đa 100 ký tự.'
            };
        }

        if (form.find('#field-password').length > 0) {
            rules.Password = { required: true, maxlength: 100, passwordPolicy: true };
            messages.Password = {
                required: form.find('#field-email').length > 0 ? 'Nhập mật khẩu.' : 'Nhập mật khẩu mới.',
                maxlength: 'Mật khẩu tối đa 100 ký tự.'
            };
            rules.ConfirmPassword = { required: true, equalTo: '#field-password' };
        }

        if (form.find('#field-new-password').length > 0) {
            rules.NewPassword = {
                required: true,
                maxlength: 100,
                passwordPolicy: true,
                notEqualToCurrent: true
            };
            messages.NewPassword = {
                required: 'Nhập mật khẩu mới.',
                maxlength: 'Mật khẩu tối đa 100 ký tự.',
                notEqualToCurrent: 'Mật khẩu mới phải khác mật khẩu hiện tại.'
            };
            rules.ConfirmPassword = { required: true, equalTo: '#field-new-password' };
        }

        if (rules.ConfirmPassword) {
            messages.ConfirmPassword = {
                required: 'Nhập lại mật khẩu.',
                equalTo: 'Hai lần nhập mật khẩu chưa khớp nhau.'
            };
        }

        return { rules: rules, messages: messages };
    }

    function setUpValidation(form) {
        if (!$.validator) {
            return;
        }

        $.validator.addMethod('allowedEmailCharacters', function (value, element) {
            var allowed = $(element).attr('data-email-allowed') || '';
            if (this.optional(element) || allowed.length === 0) {
                return true;
            }

            return $.trim(value).split('').every(function (character) {
                return allowed.indexOf(character) >= 0;
            });
        });

        $.validator.addMethod('accountEmail', function (value, element) {
            return this.optional(element) || /^[^@\s]+@[^@\s.]+(\.[^@\s.]+)+$/.test(value);
        });

        $.validator.addMethod('notEqualToCurrent', function (value, element) {
            return this.optional(element) || value !== $('#field-current-password').val();
        });

        $.validator.addMethod('passwordPolicy', function (value, element) {
            return this.optional(element) || missingLabels(element.id, value).length === 0;
        }, function (params, element) {
            return 'Mật khẩu còn thiếu: ' + missingLabels(element.id, $(element).val() || '').join(', ') + '.';
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
            },
            invalidHandler: function () {
                window.setTimeout(function () {
                    focusFirstError($('#account-form'));
                }, 0);
            }
        });
    }

    $(function () {
        var form = $('#account-form');
        if (form.length === 0) {
            return;
        }

        setUpToggles(form);
        setUpChecklists(form);
        setUpCounters(form);
        setUpSubmitState(form);
        setUpValidation(form);
        focusFirstError(form);
    });
})(window.jQuery);
