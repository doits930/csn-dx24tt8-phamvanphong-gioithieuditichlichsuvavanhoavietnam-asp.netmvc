(function ($) {
    'use strict';

    if (!$) {
        return;
    }

    var SUCCESS_DELAY = 6000;
    var SIDEBAR_BREAKPOINT = 992;
    var SEARCH_BREAKPOINT = 768;
    var SUGGEST_DELAY = 220;
    var MIN_SUGGEST_LENGTH = 2;
    var STORAGE_KEY = 'admin-sidebar';

    var root = document.documentElement;

    function startToasts() {
        if (!window.bootstrap || !window.bootstrap.Toast) {
            return;
        }

        Array.prototype.forEach.call(document.querySelectorAll('[data-admin-toast]'), function (element) {
            var autohide = element.getAttribute('data-toast-autohide') === 'true';
            var toast = new window.bootstrap.Toast(element, {
                autohide: autohide,
                delay: SUCCESS_DELAY
            });

            toast.show();
        });
    }

    function isWideScreen() {
        return window.innerWidth >= SIDEBAR_BREAKPOINT;
    }

    function startSidebar() {
        var sidebar = document.getElementById('admin-sidebar');
        var toggle = document.querySelector('[data-sidebar-toggle]');
        if (!sidebar || !toggle) {
            return;
        }

        var labels = document.querySelectorAll('[data-nav-label]');

        function applyState() {
            var wide = isWideScreen();
            var collapsed = wide && root.classList.contains('admin-sidebar-collapsed');

            if (wide) {
                toggle.setAttribute('aria-expanded', collapsed ? 'false' : 'true');
                toggle.setAttribute('aria-label', collapsed ? 'Mở rộng điều hướng' : 'Thu gọn điều hướng');
            } else {
                toggle.setAttribute('aria-expanded', sidebar.classList.contains('show') ? 'true' : 'false');
                toggle.setAttribute('aria-label', 'Mở điều hướng quản trị');
            }

            Array.prototype.forEach.call(labels, function (link) {
                if (collapsed) {
                    link.setAttribute('title', link.getAttribute('data-nav-label'));
                } else {
                    link.removeAttribute('title');
                }
            });
        }

        function openOffcanvas() {
            if (!window.bootstrap || !window.bootstrap.Offcanvas) {
                return;
            }

            window.bootstrap.Offcanvas.getOrCreateInstance(sidebar).show();
        }

        toggle.addEventListener('click', function () {
            if (!isWideScreen()) {
                openOffcanvas();
                return;
            }

            var collapsed = root.classList.toggle('admin-sidebar-collapsed');
            try {
                localStorage.setItem(STORAGE_KEY, collapsed ? 'collapsed' : 'expanded');
            } catch (error) {
                applyState();
                return;
            }

            applyState();
        });

        sidebar.addEventListener('shown.bs.offcanvas', applyState);

        sidebar.addEventListener('hidden.bs.offcanvas', function () {
            applyState();
            toggle.focus();
        });

        $(window).on('resize', applyState);
        applyState();
    }

    function buildSuggestion(item, index) {
        var link = document.createElement('a');
        link.className = 'admin-suggestion';
        link.setAttribute('role', 'option');
        link.setAttribute('aria-selected', 'false');
        link.id = 'admin-suggestion-' + index;
        link.href = item.editUrl;

        if (item.thumbnailUrl) {
            var image = document.createElement('img');
            image.src = item.thumbnailUrl;
            image.alt = '';
            image.loading = 'lazy';
            image.decoding = 'async';
            link.appendChild(image);
        } else {
            var blank = document.createElement('span');
            blank.className = 'admin-suggestion-blank';
            blank.setAttribute('aria-hidden', 'true');
            link.appendChild(blank);
        }

        var text = document.createElement('span');
        text.className = 'admin-suggestion-text';

        var name = document.createElement('span');
        name.className = 'admin-suggestion-name';
        $(name).text(item.name);
        text.appendChild(name);

        var province = document.createElement('span');
        province.className = 'admin-suggestion-province';
        $(province).text(item.provinceName);
        text.appendChild(province);

        link.appendChild(text);

        return link;
    }

    function startSearch() {
        var form = document.querySelector('[data-admin-search]');
        if (!form) {
            return;
        }

        var input = form.querySelector('[data-admin-search-input]');
        var list = form.querySelector('[data-admin-search-list]');
        var openButton = document.querySelector('[data-admin-search-open]');
        var closeButton = form.querySelector('[data-admin-search-close]');
        var status = form.querySelector('[data-admin-search-status]');
        var suggestUrl = form.getAttribute('data-suggest-url');
        var keywordKey = form.getAttribute('data-keyword-key');
        var activeIndex = -1;
        var timer = null;
        var request = null;

        function isNarrowScreen() {
            return window.innerWidth < SEARCH_BREAKPOINT;
        }

        function announce(text) {
            if (status) {
                $(status).text(text);
            }
        }

        function closeList() {
            activeIndex = -1;
            list.hidden = true;
            list.innerHTML = '';
            input.setAttribute('aria-expanded', 'false');
            input.removeAttribute('aria-activedescendant');
            announce('');
        }

        function options() {
            return list.querySelectorAll('.admin-suggestion');
        }

        function markActive(index) {
            var items = options();
            if (items.length === 0) {
                return;
            }

            if (index < 0) {
                index = items.length - 1;
            } else if (index >= items.length) {
                index = 0;
            }

            activeIndex = index;
            Array.prototype.forEach.call(items, function (item, position) {
                var current = position === activeIndex;
                item.classList.toggle('is-active', current);
                item.setAttribute('aria-selected', current ? 'true' : 'false');
            });

            input.setAttribute('aria-activedescendant', items[activeIndex].id);
        }

        function render(items) {
            list.innerHTML = '';
            activeIndex = -1;
            input.removeAttribute('aria-activedescendant');

            if (items.length === 0) {
                var empty = document.createElement('li');
                empty.className = 'admin-search-empty';
                empty.setAttribute('role', 'presentation');
                $(empty).text('Không có di tích nào khớp từ khóa.');
                list.appendChild(empty);
            } else {
                items.forEach(function (item, index) {
                    var row = document.createElement('li');
                    row.setAttribute('role', 'presentation');
                    row.appendChild(buildSuggestion(item, index));
                    list.appendChild(row);
                });
            }

            list.hidden = false;
            input.setAttribute('aria-expanded', 'true');
            announce(items.length === 0 ? 'Không có gợi ý nào' : 'Có ' + items.length + ' gợi ý');
        }

        function fetchSuggestions() {
            var keyword = $.trim(input.value);
            if (keyword.length < MIN_SUGGEST_LENGTH) {
                closeList();
                return;
            }

            if (!suggestUrl || !keywordKey) {
                return;
            }

            if (request) {
                request.abort();
            }

            var query = {};
            query[keywordKey] = keyword;

            request = $.getJSON(suggestUrl, query)
                .done(function (items) {
                    render(items || []);
                })
                .fail(function () {
                    closeList();
                })
                .always(function () {
                    request = null;
                });
        }

        function schedule() {
            window.clearTimeout(timer);
            timer = window.setTimeout(fetchSuggestions, SUGGEST_DELAY);
        }

        function openPanel() {
            form.classList.add('is-open');
            if (openButton) {
                openButton.setAttribute('aria-expanded', 'true');
            }

            input.focus();
            input.select();
        }

        function closePanel(returnFocus) {
            form.classList.remove('is-open');
            closeList();
            if (openButton) {
                openButton.setAttribute('aria-expanded', 'false');
                if (returnFocus) {
                    openButton.focus();
                }
            }
        }

        if (openButton) {
            openButton.addEventListener('click', openPanel);
        }

        if (closeButton) {
            closeButton.addEventListener('click', function () {
                closePanel(true);
            });
        }

        input.addEventListener('input', schedule);

        input.addEventListener('keydown', function (event) {
            if (event.key === 'ArrowDown') {
                event.preventDefault();
                markActive(activeIndex + 1);
            } else if (event.key === 'ArrowUp') {
                event.preventDefault();
                markActive(activeIndex - 1);
            } else if (event.key === 'Home' && !list.hidden) {
                event.preventDefault();
                markActive(0);
            } else if (event.key === 'End' && !list.hidden) {
                event.preventDefault();
                markActive(options().length - 1);
            } else if (event.key === 'Enter') {
                var items = options();
                if (activeIndex >= 0 && items[activeIndex]) {
                    event.preventDefault();
                    window.location.href = items[activeIndex].href;
                }
            } else if (event.key === 'Escape') {
                if (!list.hidden) {
                    event.preventDefault();
                    closeList();
                } else if (isNarrowScreen() && form.classList.contains('is-open')) {
                    event.preventDefault();
                    closePanel(true);
                }
            }
        });

        form.addEventListener('submit', function (event) {
            var keyword = $.trim(input.value);
            input.value = keyword;
            if (keyword.length === 0) {
                event.preventDefault();
                input.focus();
            }
        });

        document.addEventListener('click', function (event) {
            if (!form.contains(event.target) && (!openButton || !openButton.contains(event.target))) {
                closeList();
            }
        });

        $(window).on('resize', function () {
            if (!isNarrowScreen()) {
                form.classList.remove('is-open');
                if (openButton) {
                    openButton.setAttribute('aria-expanded', 'false');
                }
            }
        });

        $(document).on('keydown', function (event) {
            if (event.key !== '/' || event.ctrlKey || event.altKey || event.metaKey) {
                return;
            }

            var active = document.activeElement;
            var tag = active ? active.tagName.toLowerCase() : '';
            if (tag === 'input' || tag === 'textarea' || tag === 'select') {
                return;
            }

            if (active && active.isContentEditable) {
                return;
            }

            event.preventDefault();
            if (isNarrowScreen()) {
                openPanel();
                return;
            }

            input.focus();
            input.select();
        });
    }

    $(function () {
        startToasts();
        startSidebar();
        startSearch();
    });
})(window.jQuery);
