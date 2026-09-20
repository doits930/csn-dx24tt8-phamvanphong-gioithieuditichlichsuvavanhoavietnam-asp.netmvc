(function ($) {
    'use strict';

    if (!$) {
        return;
    }

    $(function () {
        var form = $('#login-form');
        if (form.length === 0) {
            return;
        }

        var toggle = form.find('[data-password-toggle]');
        var passwordField = $('#password');

        toggle.on('click', function () {
            var shown = passwordField.attr('type') === 'text';
            passwordField.attr('type', shown ? 'password' : 'text');
            toggle.attr('aria-pressed', shown ? 'false' : 'true').text(shown ? 'Hiện' : 'Ẩn');
        });

        if (!$.validator) {
            return;
        }

        $.validator.addMethod('accountEmail', function (value, element) {
            return this.optional(element) || /^[^@\s]+@[^@\s.]+(\.[^@\s.]+)+$/.test(value);
        });

        form.validate({
            onkeyup: false,
            errorElement: 'p',
            errorClass: 'field-error',
            rules: {
                UserName: { required: true, maxlength: 256, accountEmail: true },
                Password: { required: true, maxlength: 100 }
            },
            messages: {
                UserName: {
                    required: 'Nhập tài khoản.',
                    maxlength: 'Tài khoản tối đa 256 ký tự.',
                    accountEmail: 'Tài khoản phải là địa chỉ thư điện tử.'
                },
                Password: {
                    required: 'Nhập mật khẩu.',
                    maxlength: 'Mật khẩu tối đa 100 ký tự.'
                }
            },
            errorPlacement: function (error, element) {
                error.appendTo(element.closest('.login-field'));
            },
            highlight: function (element) {
                $(element).attr('aria-invalid', 'true');
            },
            unhighlight: function (element) {
                $(element).attr('aria-invalid', 'false');
            }
        });
    });
})(window.jQuery);
