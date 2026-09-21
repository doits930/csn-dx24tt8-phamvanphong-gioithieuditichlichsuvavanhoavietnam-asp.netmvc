(function ($) {
    'use strict';

    if (!$) {
        return;
    }

    var MAX_FILES = 10;
    var MAX_FILE_BYTES = 5 * 1024 * 1024;
    var MIN_FILE_BYTES = 128;
    var MAX_BATCH_BYTES = 30 * 1024 * 1024;
    var MAX_CAPTION = 300;
    var MAX_SOURCE = 500;
    var ALLOWED = ['image/jpeg', 'image/png', 'image/webp'];

    var SOURCE_REQUIRED = 'Nhập nguồn ảnh.';
    var SOURCE_TOO_LONG = 'Nguồn ảnh tối đa 500 ký tự.';
    var CAPTION_TOO_LONG = 'Chú thích tối đa 300 ký tự.';
    var IDLE_HINT = 'Chọn tệp ảnh để bật nút tải lên.';

    function describeSize(bytes) {
        if (bytes < 1024) {
            return bytes + ' B';
        }

        if (bytes < 1024 * 1024) {
            return Math.round(bytes / 1024) + ' KB';
        }

        return (Math.round(bytes / (1024 * 1024) * 10) / 10) + ' MB';
    }

    function markError(field, message, errorId) {
        var holder = field.closest('.admin-field');
        holder.addClass('admin-field-invalid');
        field.attr('aria-invalid', 'true');

        var described = (field.attr('aria-describedby') || '').split(/\s+/).filter(function (id) {
            return id.length > 0 && id !== errorId;
        });
        described.push(errorId);
        field.attr('aria-describedby', described.join(' '));

        holder.append($('<p>').addClass('field-error').attr('id', errorId).text(message));
    }

    function clearErrors(scope) {
        scope.find('.field-error').each(function () {
            var errorId = this.id;
            if (errorId) {
                scope.find('[aria-describedby~="' + errorId + '"]').each(function () {
                    var field = $(this);
                    var kept = (field.attr('aria-describedby') || '').split(/\s+/).filter(function (id) {
                        return id.length > 0 && id !== errorId;
                    });
                    if (kept.length > 0) {
                        field.attr('aria-describedby', kept.join(' '));
                    } else {
                        field.removeAttr('aria-describedby');
                    }
                });
            }
            $(this).remove();
        });

        scope.find('.admin-field-invalid').removeClass('admin-field-invalid');
        scope.find('[aria-invalid="true"]').attr('aria-invalid', 'false');
    }

    function Uploader(form) {
        this.form = form;
        this.input = form.find('[data-upload-input]');
        this.queue = form.find('[data-upload-queue]');
        this.dropZone = form.find('[data-drop-zone]');
        this.state = form.find('[data-upload-state]');
        this.actions = form.find('[data-upload-actions]');
        this.clearButton = form.find('[data-upload-clear]');
        this.submitButton = form.find('[data-upload-submit]');
        this.applyButton = form.find('[data-apply-source]');
        this.sharedSource = form.find('#field-shared-source');
        this.files = [];
        this.previews = [];
    }

    Uploader.prototype.start = function () {
        var self = this;

        this.input.on('change', function () {
            self.add(this.files);
        });

        this.dropZone.on('dragover dragenter', function (event) {
            event.preventDefault();
            self.dropZone.addClass('is-dragging');
        });

        this.dropZone.on('dragleave dragend drop', function () {
            self.dropZone.removeClass('is-dragging');
        });

        this.dropZone.on('drop', function (event) {
            event.preventDefault();
            var transfer = event.originalEvent.dataTransfer;
            if (transfer && transfer.files) {
                self.add(transfer.files);
            }
        });

        this.queue.on('click', '[data-remove-file]', function () {
            self.remove(parseInt($(this).attr('data-remove-file'), 10));
        });

        this.clearButton.on('click', function () {
            self.reset();
        });

        this.applyButton.on('click', function () {
            var value = $.trim(self.sharedSource.val() || '');
            if (value === '') {
                self.sharedSource.trigger('focus');
                return;
            }

            self.queue.find('[data-row-source]').val(value);
            self.state.text('Đã áp nguồn chung cho ' + self.files.length + ' ảnh trong lô.');
        });

        this.form.on('submit', function (event) {
            if (!self.check()) {
                event.preventDefault();
            }
        });

        this.render();
    };

    Uploader.prototype.add = function (fileList) {
        var incoming = Array.prototype.slice.call(fileList || []);
        var self = this;
        var dropped = 0;
        var duplicates = 0;

        incoming.forEach(function (file) {
            if (self.files.length >= MAX_FILES) {
                dropped += 1;
                return;
            }

            var duplicate = self.files.some(function (existing) {
                return existing.name === file.name && existing.size === file.size;
            });

            if (duplicate) {
                duplicates += 1;
                return;
            }

            self.files.push(file);
        });

        this.render();

        if (dropped > 0) {
            this.state.text('Mỗi lượt chỉ nhận ' + MAX_FILES + ' tệp, ' + dropped + ' tệp cuối đã bị bỏ qua.');
        } else if (duplicates > 0) {
            this.state.text(duplicates + ' tệp đã có sẵn trong lô nên không thêm lại.');
        }
    };

    Uploader.prototype.remove = function (index) {
        if (isNaN(index) || index < 0 || index >= this.files.length) {
            return;
        }

        var name = this.files[index].name;
        var kept = this.collectValues();
        kept.splice(index, 1);
        this.files.splice(index, 1);
        this.render(kept);
        this.state.text('Đã gỡ ' + name + ' khỏi lô. Còn ' + this.files.length + ' tệp.');
    };

    Uploader.prototype.reset = function () {
        this.files = [];
        this.render();
    };

    Uploader.prototype.collectValues = function () {
        var values = [];
        this.queue.find('[data-queue-row]').each(function () {
            var row = $(this);
            values.push({
                caption: row.find('[data-row-caption]').val() || '',
                source: row.find('[data-row-source]').val() || ''
            });
        });

        return values;
    };

    Uploader.prototype.render = function (values) {
        var self = this;
        var kept = values || this.collectValues();

        this.previews.forEach(function (url) {
            window.URL.revokeObjectURL(url);
        });
        this.previews = [];
        this.queue.empty();

        this.syncInput();

        if (this.files.length === 0) {
            this.queue.prop('hidden', true);
            this.clearButton.prop('hidden', true);
            this.applyButton.prop('hidden', true);
            this.submitButton.prop('disabled', true);
            this.actions.addClass('is-idle');
            this.state.text(IDLE_HINT);
            return;
        }

        this.queue.prop('hidden', false);
        this.clearButton.prop('hidden', false);
        this.applyButton.prop('hidden', false);
        this.submitButton.prop('disabled', false);
        this.actions.removeClass('is-idle');

        var total = 0;
        this.files.forEach(function (file, index) {
            total += file.size;
            var saved = kept[index] || { caption: '', source: '' };
            self.queue.append(self.buildRow(file, index, saved));
        });

        this.state.text(this.files.length + ' tệp đang chờ, tổng ' + describeSize(total) + '.');
    };

    Uploader.prototype.buildRow = function (file, index, saved) {
        var url = window.URL.createObjectURL(file);
        this.previews.push(url);

        var row = $('<li>').addClass('upload-row').attr('data-queue-row', '');
        var problem = this.inspect(file);
        if (problem) {
            row.addClass('is-rejected');
        }

        var thumb = $('<img>').addClass('upload-row-thumb').attr({ src: url, alt: '' });
        thumb.on('error', function () {
            $(this).replaceWith($('<span>')
                .addClass('upload-row-thumb upload-row-thumb-empty')
                .text('Không xem trước được tệp này'));
        });

        var head = $('<div>').addClass('upload-row-head');
        head.append($('<span>').addClass('upload-row-name').text(file.name));
        head.append($('<span>').addClass('upload-row-size').text(describeSize(file.size)));
        if (problem) {
            head.append($('<span>').addClass('upload-row-problem').text(problem));
        }

        var captionId = 'queue-caption-' + index;
        var sourceId = 'queue-source-' + index;

        var captionField = $('<div>').addClass('admin-field');
        captionField.append($('<label>').attr('for', captionId).text('Chú thích'));
        captionField.append($('<input>').attr({
            type: 'text',
            id: captionId,
            name: 'Captions[' + index + ']',
            maxlength: MAX_CAPTION,
            autocomplete: 'off'
        }).attr('data-row-caption', '').val(saved.caption));

        var sourceField = $('<div>').addClass('admin-field');
        sourceField.append($('<label>').attr('for', sourceId)
            .append(document.createTextNode('Nguồn ảnh '))
            .append($('<span>').addClass('admin-required').attr('aria-hidden', 'true').text('*')));
        sourceField.append($('<input>').attr({
            type: 'text',
            id: sourceId,
            name: 'Sources[' + index + ']',
            maxlength: MAX_SOURCE,
            autocomplete: 'off'
        }).attr('data-row-source', '').val(saved.source));

        var fields = $('<div>').addClass('upload-row-fields').append(captionField).append(sourceField);

        var remove = $('<button>')
            .attr({ type: 'button', 'data-remove-file': index })
            .addClass('admin-icon-btn admin-icon-btn-danger upload-row-remove')
            .attr('aria-label', 'Gỡ tệp ' + file.name + ' khỏi lô')
            .text('×');

        return row.append(thumb).append($('<div>').addClass('upload-row-main').append(head).append(fields)).append(remove);
    };

    Uploader.prototype.inspect = function (file) {
        if (file.size === 0) {
            return 'Tệp rỗng, máy chủ sẽ từ chối.';
        }

        if (file.type !== '' && ALLOWED.indexOf(file.type) < 0) {
            return 'Không phải ảnh JPEG, PNG hay WEBP.';
        }

        if (file.size < MIN_FILE_BYTES) {
            return 'Tệp quá nhỏ để là một tấm ảnh thật.';
        }

        if (file.size > MAX_FILE_BYTES) {
            return 'Nặng hơn 5 MB, máy chủ sẽ từ chối.';
        }

        return '';
    };

    Uploader.prototype.syncInput = function () {
        if (typeof window.DataTransfer !== 'function') {
            return;
        }

        var transfer = new window.DataTransfer();
        this.files.forEach(function (file) {
            transfer.items.add(file);
        });

        this.input[0].files = transfer.files;
    };

    Uploader.prototype.check = function () {
        var self = this;
        var shared = $.trim(this.sharedSource.val() || '');
        var valid = true;

        clearErrors(this.form);

        if (this.files.length === 0) {
            this.state.text('Chọn ít nhất một tệp ảnh để tải lên.');
            this.input.trigger('focus');
            return false;
        }

        if (this.files.length > MAX_FILES) {
            this.state.text('Mỗi lần chỉ tải được tối đa ' + MAX_FILES + ' tệp.');
            return false;
        }

        var total = this.files.reduce(function (sum, file) {
            return sum + file.size;
        }, 0);

        if (total > MAX_BATCH_BYTES) {
            this.state.text('Cả lượt không quá 30 MB. Lô đang chọn nặng ' + describeSize(total) + '.');
            return false;
        }

        this.queue.find('[data-queue-row]').each(function (position) {
            var row = $(this);
            var sourceField = row.find('[data-row-source]');
            var captionField = row.find('[data-row-caption]');
            var source = $.trim(sourceField.val() || '');
            var caption = captionField.val() || '';

            if (source === '' && shared === '') {
                markError(sourceField, SOURCE_REQUIRED, 'queue-source-' + position + '-error');
                valid = false;
            } else if (source.length > MAX_SOURCE) {
                markError(sourceField, SOURCE_TOO_LONG, 'queue-source-' + position + '-error');
                valid = false;
            }

            if (caption.length > MAX_CAPTION) {
                markError(captionField, CAPTION_TOO_LONG, 'queue-caption-' + position + '-error');
                valid = false;
            }
        });

        if (shared.length > MAX_SOURCE) {
            markError(this.sharedSource, SOURCE_TOO_LONG, 'field-shared-source-error');
            valid = false;
        }

        if (!valid) {
            this.state.text('Còn ô chưa hợp lệ trong lô ảnh.');
            var first = this.form.find('.admin-field-invalid input').first();
            if (first.length > 0) {
                first[0].scrollIntoView({ block: 'center' });
                first.trigger('focus');
            }
        }

        return valid;
    };

    function setUpEditForms() {
        $('[data-image-edit-form]').on('submit', function (event) {
            var form = $(this);
            var source = form.find('input[name="imageSource"]');
            clearErrors(form);

            if ($.trim(source.val() || '') !== '') {
                return;
            }

            event.preventDefault();
            markError(source, SOURCE_REQUIRED, source.attr('id') + '-error');
            source.trigger('focus');
        });
    }

    function setUpViewer() {
        var dialog = document.querySelector('[data-image-viewer]');
        if (!dialog || typeof dialog.showModal !== 'function') {
            return;
        }

        var image = dialog.querySelector('[data-viewer-image]');
        var caption = dialog.querySelector('[data-viewer-caption]');
        var lastTrigger = null;

        $(document).on('click', '[data-image-open]', function (event) {
            if (event.metaKey || event.ctrlKey || event.shiftKey || event.which > 1) {
                return;
            }

            event.preventDefault();
            lastTrigger = this;
            image.setAttribute('src', this.getAttribute('data-image-src'));
            image.setAttribute('alt', this.getAttribute('data-image-caption') || '');
            caption.textContent = this.getAttribute('data-image-caption') || '';
            dialog.showModal();
        });

        $(dialog).on('click', '[data-viewer-close]', function () {
            dialog.close();
        });

        $(dialog).on('click', function (event) {
            if (event.target === dialog) {
                dialog.close();
            }
        });

        $(dialog).on('close', function () {
            image.setAttribute('src', '');
            if (lastTrigger) {
                lastTrigger.focus();
            }
        });
    }

    $(function () {
        var form = $('#image-upload-form');
        if (form.length > 0) {
            new Uploader(form).start();
        }

        setUpEditForms();
        setUpViewer();
    });
})(window.jQuery);
