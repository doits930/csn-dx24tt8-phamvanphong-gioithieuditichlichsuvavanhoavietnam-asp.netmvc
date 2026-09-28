(function ($) {
    'use strict';

    var $gallery = $('[data-relic-gallery]');
    if ($gallery.length === 0) {
        return;
    }

    var slides = [];
    $gallery.find('[data-gallery-open]').each(function () {
        var index = parseInt($(this).attr('data-gallery-open'), 10);
        var $item = $(this).closest('.gallery-fallback-item');
        slides[index] = {
            src: $(this).attr('href'),
            alt: $(this).find('img').attr('alt') || '',
            caption: $item.find('[data-gallery-caption]').text().trim(),
            source: $item.find('[data-gallery-source]').html() || ''
        };
    });

    if (slides.length === 0) {
        return;
    }

    var current = 0;
    var $opener = null;
    var $viewer = null;
    var $image = null;
    var $caption = null;
    var $source = null;
    var $counter = null;
    var $previous = null;
    var $next = null;

    function build() {
        $viewer = $('<div class="image-viewer" role="dialog" aria-modal="true" aria-label="Ảnh phóng lớn" hidden></div>');
        var $panel = $('<div class="image-viewer-panel"></div>');
        var $close = $('<button type="button" class="image-viewer-close" aria-label="Đóng ảnh">Đóng</button>');
        $previous = $('<button type="button" class="image-viewer-step image-viewer-previous" aria-label="Ảnh trước">Ảnh trước</button>');
        $next = $('<button type="button" class="image-viewer-step image-viewer-next" aria-label="Ảnh sau">Ảnh sau</button>');
        $image = $('<img class="image-viewer-image" alt="" decoding="async" />');
        $caption = $('<p class="image-viewer-caption"></p>');
        $source = $('<p class="image-viewer-source"></p>');
        $counter = $('<p class="image-viewer-counter"></p>');

        var $text = $('<div class="image-viewer-text"></div>').append($counter, $caption, $source);
        var $stage = $('<div class="image-viewer-stage"></div>').append($image);

        $panel.append($close, $stage, $text, $('<div class="image-viewer-steps"></div>').append($previous, $next));
        $viewer.append($panel);
        $('body').append($viewer);

        $close.on('click', close);
        $previous.on('click', function () { show(current - 1); });
        $next.on('click', function () { show(current + 1); });
        $viewer.on('click', function (event) {
            if (event.target === $viewer[0]) {
                close();
            }
        });
        $viewer.on('keydown', onKeyDown);
    }

    function focusables() {
        return $viewer.find('button:visible').toArray();
    }

    function onKeyDown(event) {
        if (event.key === 'Escape') {
            event.preventDefault();
            close();
            return;
        }

        if (event.key === 'ArrowLeft') {
            event.preventDefault();
            show(current - 1);
            return;
        }

        if (event.key === 'ArrowRight') {
            event.preventDefault();
            show(current + 1);
            return;
        }

        if (event.key !== 'Tab') {
            return;
        }

        var items = focusables();
        if (items.length === 0) {
            return;
        }

        var first = items[0];
        var last = items[items.length - 1];

        if (event.shiftKey && document.activeElement === first) {
            event.preventDefault();
            last.focus();
        } else if (!event.shiftKey && document.activeElement === last) {
            event.preventDefault();
            first.focus();
        }
    }

    function show(index) {
        if (slides.length === 0) {
            return;
        }

        current = (index + slides.length) % slides.length;
        var slide = slides[current];

        $image.attr('src', slide.src).attr('alt', slide.alt);
        $caption.text(slide.caption).toggle(slide.caption.length > 0);
        $source.html(slide.source).toggle(slide.source.length > 0);
        $counter.text('Ảnh ' + (current + 1) + ' trên ' + slides.length);
        $previous.add($next).toggle(slides.length > 1);
    }

    function open(index, opener) {
        if ($viewer === null) {
            build();
        }

        $opener = opener;
        show(index);
        $viewer.prop('hidden', false);
        $('body').addClass('has-open-viewer');
        $viewer.find('.image-viewer-close').trigger('focus');
    }

    function close() {
        if ($viewer === null) {
            return;
        }

        $viewer.prop('hidden', true);
        $('body').removeClass('has-open-viewer');

        if (stage !== null) {
            stage.show(current);
        }

        if ($opener !== null && $opener.length > 0) {
            $opener.trigger('focus');
            $opener = null;
        }
    }

    var stage = (function () {
        var $zone = $('[data-gallery-stage]');
        if ($zone.length === 0) {
            return null;
        }

        var $image = $zone.find('[data-stage-image]');
        var $backdrop = $zone.find('[data-stage-backdrop]');
        var $caption = $zone.find('[data-stage-caption]');
        var $source = $zone.find('[data-stage-source]');
        var $counter = $('[data-stage-counter]');
        var $picks = $zone.find('[data-stage-pick]');
        var index = 0;

        function preload(position) {
            var slide = slides[(position + slides.length) % slides.length];
            if (slide) {
                var image = new Image();
                image.src = slide.src;
            }
        }

        function render(next) {
            index = (next + slides.length) % slides.length;
            var slide = slides[index];

            $image.attr('src', slide.src).attr('alt', slide.alt);
            $backdrop.attr('src', slide.src);
            $caption.text(slide.caption).toggle(slide.caption.length > 0);
            $source.html(slide.source).toggle(slide.source.length > 0);
            $counter.text((index + 1) + ' / ' + slides.length);

            $picks.each(function () {
                var isCurrent = parseInt($(this).attr('data-stage-pick'), 10) === index;
                $(this).toggleClass('is-current', isCurrent);
                if (isCurrent) {
                    $(this).attr('aria-current', 'true');
                } else {
                    $(this).removeAttr('aria-current');
                }
            });

            preload(index + 1);
            preload(index - 1);
        }

        function step(delta) {
            $zone.addClass('is-changing');
            window.setTimeout(function () {
                render(index + delta);
                $zone.removeClass('is-changing');
            }, 120);
        }

        $zone.find('[data-stage-previous]').on('click', function () { step(-1); });
        $zone.find('[data-stage-next]').on('click', function () { step(1); });

        $picks.on('click', function () {
            var picked = parseInt($(this).attr('data-stage-pick'), 10);
            if (!isNaN(picked) && picked !== index) {
                step(picked - index);
            }
        });

        $zone.find('[data-stage-open]').on('click', function () {
            open(index, $(this));
        });

        $zone.on('keydown', function (event) {
            if (event.key === 'ArrowLeft') {
                event.preventDefault();
                step(-1);
            } else if (event.key === 'ArrowRight') {
                event.preventDefault();
                step(1);
            }
        });

        var touchStartX = 0;
        var touchStartY = 0;

        $zone.find('.gallery-stage').on('touchstart', function (event) {
            var touch = event.originalEvent.changedTouches[0];
            touchStartX = touch.clientX;
            touchStartY = touch.clientY;
        });

        $zone.find('.gallery-stage').on('touchend', function (event) {
            var touch = event.originalEvent.changedTouches[0];
            var deltaX = touch.clientX - touchStartX;
            var deltaY = touch.clientY - touchStartY;
            if (Math.abs(deltaX) > 48 && Math.abs(deltaX) > Math.abs(deltaY)) {
                step(deltaX < 0 ? 1 : -1);
            }
        });

        render(0);

        return { show: render };
    }());

    $(document).on('click', '[data-gallery-open]', function (event) {
        if (event.which > 1 || event.metaKey || event.ctrlKey || event.shiftKey) {
            return;
        }

        var index = parseInt($(this).attr('data-gallery-open'), 10);
        if (isNaN(index) || !slides[index]) {
            return;
        }

        event.preventDefault();
        open(index, $(this));
    });
}(jQuery));
