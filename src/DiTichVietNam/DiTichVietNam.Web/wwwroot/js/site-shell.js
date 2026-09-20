(function () {
    'use strict';

    var root = document.documentElement;

    function prefersReducedMotion() {
        return window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    }

    function watchHeader() {
        var sentinel = document.querySelector('[data-header-sentinel]');
        if (!sentinel || !window.IntersectionObserver) {
            return;
        }

        var observer = new IntersectionObserver(function (entries) {
            root.classList.toggle('is-header-stuck', !entries[0].isIntersecting);
        }, { threshold: 0 });

        observer.observe(sentinel);
    }

    function setVisible(button, visible) {
        button.classList.toggle('is-hidden', !visible);
        if (visible) {
            button.removeAttribute('inert');
        } else {
            button.setAttribute('inert', '');
        }
    }

    function focusTop() {
        var target = document.querySelector('main[tabindex="-1"]') || document.body;
        if (!target.hasAttribute('tabindex')) {
            target.setAttribute('tabindex', '-1');
        }

        target.focus({ preventScroll: true });
    }

    function watchBackToTop() {
        var button = document.querySelector('[data-back-to-top]');
        var sentinel = document.querySelector('[data-scroll-sentinel]');
        if (!button || !sentinel || !window.IntersectionObserver) {
            return;
        }

        setVisible(button, false);

        var observer = new IntersectionObserver(function (entries) {
            setVisible(button, !entries[0].isIntersecting);
        }, { threshold: 0 });

        observer.observe(sentinel);

        button.addEventListener('click', function () {
            window.scrollTo({ top: 0, behavior: prefersReducedMotion() ? 'auto' : 'smooth' });
            focusTop();
        });
    }

    function start() {
        watchHeader();
        watchBackToTop();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', start);
    } else {
        start();
    }
})();
