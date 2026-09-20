(function ($) {
    'use strict';

    if (!$) {
        return;
    }

    function parseDecimal(value) {
        var text = $.trim(value).replace(',', '.');
        if (!/^[-+]?\d*\.?\d+$/.test(text)) {
            return null;
        }

        var parsed = parseFloat(text);
        return isNaN(parsed) ? null : parsed;
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

        source.on('input', function () {
            var slug = toSlug(source.val());
            preview.text(slug.length > 0 ? slug : original);
        });
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
                area.style.height = Math.min(area.scrollHeight + 2, 640) + 'px';
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
            state.text('Đang lưu...');
            submitButton.prop('disabled', true).text('Đang lưu...');
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

    function setUpValidation(form) {
        if (!$.validator) {
            return;
        }

        var maxYear = parseInt($('#field-year').attr('data-max-year'), 10) || new Date().getFullYear();

        $.validator.addMethod('wholeNumber', function (value, element) {
            return this.optional(element) || /^\d+$/.test($.trim(value));
        });

        $.validator.addMethod('yearRange', function (value, element) {
            if (this.optional(element)) {
                return true;
            }

            var year = parseInt($.trim(value), 10);
            return isNaN(year) || (year >= 1000 && year <= maxYear);
        });

        $.validator.addMethod('decimalNumber', function (value, element) {
            return this.optional(element) || parseDecimal(value) !== null;
        });

        $.validator.addMethod('decimalRange', function (value, element, param) {
            if (this.optional(element)) {
                return true;
            }

            var number = parseDecimal(value);
            return number === null || (number >= param[0] && number <= param[1]);
        });

        $.validator.addMethod('webAddress', function (value, element) {
            return this.optional(element) || /^https?:\/\/[^\s]+$/i.test($.trim(value));
        });

        var hasValue = function (selector) {
            return $.trim($(selector).val() || '') !== '';
        };

        form.validate({
            onkeyup: false,
            errorElement: 'p',
            errorClass: 'field-error',
            rules: {
                Name: { required: true, maxlength: 200 },
                Address: { required: true, maxlength: 300 },
                Description: { required: true, maxlength: 20000 },
                History: { maxlength: 20000 },
                VisitInfo: { maxlength: 4000 },
                ProvinceId: { required: true },
                RelicTypeId: { required: true },
                RankingLevel: { required: true },
                RecognizedYear: { wholeNumber: true, yearRange: true },
                SourceUrl: { required: true, maxlength: 500, webAddress: true },
                Latitude: {
                    required: { depends: function () { return hasValue('#field-longitude'); } },
                    decimalNumber: true,
                    decimalRange: [-90, 90]
                },
                Longitude: {
                    required: { depends: function () { return hasValue('#field-latitude'); } },
                    decimalNumber: true,
                    decimalRange: [-180, 180]
                }
            },
            messages: {
                Name: {
                    required: 'Nhập tên di tích.',
                    maxlength: 'Tên di tích tối đa 200 ký tự.'
                },
                Address: {
                    required: 'Nhập địa chỉ của di tích.',
                    maxlength: 'Địa chỉ tối đa 300 ký tự.'
                },
                Description: {
                    required: 'Nhập phần mô tả di tích.',
                    maxlength: 'Mô tả tối đa 20000 ký tự.'
                },
                History: { maxlength: 'Lịch sử tối đa 20000 ký tự.' },
                VisitInfo: { maxlength: 'Thông tin tham quan tối đa 4000 ký tự.' },
                ProvinceId: { required: 'Chọn tỉnh thành.' },
                RelicTypeId: { required: 'Chọn loại di tích.' },
                RankingLevel: { required: 'Chọn cấp xếp hạng.' },
                RecognizedYear: {
                    wholeNumber: 'Năm xếp hạng phải là một số, ví dụ 1962.',
                    yearRange: 'Năm xếp hạng phải từ 1000 đến ' + maxYear + '.'
                },
                SourceUrl: {
                    required: 'Nhập đường dẫn nguồn tham khảo.',
                    maxlength: 'Đường dẫn nguồn tham khảo tối đa 500 ký tự.',
                    webAddress: 'Đường dẫn nguồn tham khảo phải bắt đầu bằng http:// hoặc https://.'
                },
                Latitude: {
                    required: 'Nhập đủ cả vĩ độ và kinh độ, hoặc bỏ trống cả hai.',
                    decimalNumber: 'Vĩ độ phải là một số, ví dụ 21.0293.',
                    decimalRange: 'Vĩ độ phải nằm trong khoảng từ -90 đến 90.'
                },
                Longitude: {
                    required: 'Nhập đủ cả vĩ độ và kinh độ, hoặc bỏ trống cả hai.',
                    decimalNumber: 'Kinh độ phải là một số, ví dụ 105.8342.',
                    decimalRange: 'Kinh độ phải nằm trong khoảng từ -180 đến 180.'
                }
            },
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
                    focusFirstError($('#relic-form'));
                }, 0);
            }
        });
    }

    $(function () {
        var form = $('#relic-form');
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
