$(function () {
    var $root = $('[data-lookbook]');
    if ($root.length === 0) {
        return;
    }

    var slideDuration = 7000;
    var compactBreakpoint = 768;
    var parallaxBreakpoint = 992;
    var mainPlateShift = 0.06;
    var insetPlateShift = 0.12;
    var parallaxLimit = 36;
    var swipeThreshold = 40;

    var $pages = $root.find('[data-lookbook-page]');
    var $tocItems = $root.find('[data-lookbook-jump]');
    var $counter = $root.find('[data-lookbook-counter]');
    var $live = $root.find('[data-lookbook-live]');

    var total = $pages.length;
    var current = 0;
    var timer = null;
    var stepTimers = [];
    var transitioning = false;
    var userStopped = false;
    var hoverHold = false;
    var focusHold = false;

    var reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)');
    var finePointer = window.matchMedia('(hover: hover)');

    function padded(value) {
        return value < 10 ? '0' + value : String(value);
    }

    function timings() {
        return window.innerWidth < compactBreakpoint
            ? { exit: 260, total: 800 }
            : { exit: 400, total: 1100 };
    }

    function loadImage(element) {
        var $image = $(element);
        var source = $image.attr('data-lookbook-src');
        if (source && !$image.attr('src')) {
            $image.attr('src', source);
        }
    }

    function preload(index) {
        $pages.eq((index + total) % total).find('img[data-lookbook-src]').each(function () {
            loadImage(this);
        });
    }

    function clearStepTimers() {
        while (stepTimers.length > 0) {
            window.clearTimeout(stepTimers.pop());
        }
        transitioning = false;
    }

    function applyState(index) {
        $pages
            .removeClass('is-active is-entering is-leaving is-opening')
            .attr('aria-hidden', 'true')
            .prop('inert', true)
            .find('.lookbook-action')
            .attr('tabindex', '-1');

        var $page = $pages.eq(index);
        $page
            .addClass('is-active')
            .attr('aria-hidden', 'false')
            .prop('inert', false)
            .find('.lookbook-action')
            .attr('tabindex', '0');

        current = index;
        $tocItems.removeClass('is-active').removeAttr('aria-current');
        $tocItems.eq(index).addClass('is-active').attr('aria-current', 'true');
        $counter.text(padded(index + 1) + ' / ' + padded(total));
    }

    function go(target, announce) {
        var index = (target + total) % total;
        if (index === current && !transitioning) {
            return;
        }

        var instant = reducedMotion.matches || transitioning;
        var $old = $pages.eq(current);
        clearStepTimers();

        if (instant) {
            applyState(index);
        } else {
            var plan = timings();
            transitioning = true;
            $old.addClass('is-leaving');

            stepTimers.push(window.setTimeout(function () {
                applyState(index);
                $pages.eq(index).addClass('is-entering');
            }, plan.exit));

            stepTimers.push(window.setTimeout(function () {
                $pages.eq(index).removeClass('is-entering');
                transitioning = false;
            }, plan.total));
        }

        preload(index);
        preload(index + 1);

        if (announce) {
            var pageName = $pages.eq(index).find('.lookbook-name').text().replace(/\s+/g, ' ').trim();
            $live.text(pageName + ', ' + (index + 1) + ' trên ' + total);
        }
    }

    function canAutoPlay() {
        return total > 1
            && !reducedMotion.matches
            && !userStopped
            && !hoverHold
            && !focusHold
            && !document.hidden;
    }

    function syncAutoPlay() {
        if (timer !== null) {
            window.clearInterval(timer);
            timer = null;
        }

        var running = canAutoPlay();
        $root.toggleClass('is-paused', !running);
        if (running) {
            timer = window.setInterval(function () {
                go(current + 1, false);
            }, slideDuration);
        }
    }

    function goByUser(target) {
        userStopped = true;
        $root.addClass('is-stopped');
        go(target, true);
        syncAutoPlay();
    }

    if (total > 1) {
        $tocItems.on('click', function () {
            goByUser(parseInt($(this).attr('data-lookbook-index'), 10) || 0);
        });

        $root.on('keydown', function (event) {
            if (event.key === 'ArrowLeft') {
                event.preventDefault();
                goByUser(current - 1);
            } else if (event.key === 'ArrowRight') {
                event.preventDefault();
                goByUser(current + 1);
            }
        });

        $root.on('mouseenter', function () {
            hoverHold = true;
            syncAutoPlay();
        }).on('mouseleave', function () {
            hoverHold = false;
            syncAutoPlay();
        });

        $root.on('focusin', function () {
            focusHold = true;
            syncAutoPlay();
        }).on('focusout', function (event) {
            if (!$.contains($root[0], event.relatedTarget)) {
                focusHold = false;
                syncAutoPlay();
            }
        });

        $(document).on('visibilitychange', syncAutoPlay);
        reducedMotion.addEventListener('change', syncAutoPlay);

        var pointerStartX = 0;
        var pointerStartY = 0;
        var pointerActive = false;

        $root.on('pointerdown', function (event) {
            if (event.pointerType === 'mouse') {
                return;
            }
            pointerActive = true;
            pointerStartX = event.clientX;
            pointerStartY = event.clientY;
        });

        $root.on('pointerup pointercancel', function (event) {
            if (!pointerActive) {
                return;
            }
            pointerActive = false;
            var deltaX = event.clientX - pointerStartX;
            var deltaY = event.clientY - pointerStartY;
            if (Math.abs(deltaX) < swipeThreshold || Math.abs(deltaX) <= Math.abs(deltaY)) {
                return;
            }
            goByUser(deltaX < 0 ? current + 1 : current - 1);
        });

        if (document.readyState === 'complete') {
            preload(1);
        } else {
            window.addEventListener('load', function () {
                preload(1);
            });
        }

        $tocItems.on('pointerenter focus', function () {
            preload(Number($(this).attr('data-lookbook-index')));
        });

        syncAutoPlay();
    }

    if (!reducedMotion.matches) {
        var $first = $pages.eq(0).addClass('is-opening');
        window.setTimeout(function () {
            $first.removeClass('is-opening');
        }, timings().total);
    }

    var parallaxPending = false;
    var parallaxApplied = false;

    function parallaxEnabled() {
        return !reducedMotion.matches
            && finePointer.matches
            && window.innerWidth >= parallaxBreakpoint;
    }

    function shift(factor) {
        var raw = window.scrollY * factor;
        return Math.round(Math.max(-parallaxLimit, Math.min(parallaxLimit, raw)));
    }

    function move($elements, offset) {
        $elements.css('transform', offset === 0 ? '' : 'translate3d(0, ' + offset + 'px, 0)');
    }

    function applyParallax() {
        parallaxPending = false;

        if (!parallaxEnabled()) {
            if (parallaxApplied) {
                parallaxApplied = false;
                move($root.find('.lookbook-plate-frame'), 0);
                move($root.find('.lookbook-plate-inset'), 0);
            }
            return;
        }

        parallaxApplied = true;
        move($root.find('.lookbook-plate-frame'), shift(mainPlateShift));
        move($root.find('.lookbook-plate-inset'), shift(insetPlateShift));
    }

    function requestParallax() {
        if (parallaxPending) {
            return;
        }
        parallaxPending = true;
        window.requestAnimationFrame(applyParallax);
    }

    $(window).on('scroll resize', requestParallax);
    requestParallax();
});
