/* MegaMenu.js - mở/đóng bảng danh mục khi rê chuột hoặc bấm vào "Shop".
   Không cần jQuery hay Bootstrap. Thiếu phần tử nào thì tự thoát, không làm hỏng trang. */
(function () {
    'use strict';

    var trigger = document.querySelector('[data-mega-trigger]');
    var panel = document.getElementById('mm-panel');
    var backdrop = document.getElementById('mm-backdrop');
    if (!trigger || !panel) return;

    var header = document.querySelector('.main-header') ||
                 document.querySelector('header') ||
                 trigger.parentElement;
    var root = document.documentElement;
    var canHover = window.matchMedia('(hover: hover) and (pointer: fine)');

    var isOpen = false;
    var pinned = false;          // true = mở bằng click/phím, không tự đóng khi rời chuột
    var openTimer = null;
    var closeTimer = null;

    function place() {
        var bottom = header.getBoundingClientRect().bottom;
        root.style.setProperty('--mm-top', Math.max(0, Math.round(bottom)) + 'px');
    }

    function open() {
        clearTimeout(closeTimer);
        if (isOpen) return;
        place();
        isOpen = true;
        panel.classList.add('is-open');
        if (backdrop) backdrop.classList.add('is-open');
        trigger.classList.add('is-open');
        trigger.setAttribute('aria-expanded', 'true');
        panel.setAttribute('aria-hidden', 'false');
        root.classList.add('mm-lock');
    }

    function close(returnFocus) {
        clearTimeout(openTimer);
        clearTimeout(closeTimer);
        if (!isOpen) return;
        isOpen = false;
        pinned = false;
        panel.classList.remove('is-open');
        if (backdrop) backdrop.classList.remove('is-open');
        trigger.classList.remove('is-open');
        trigger.setAttribute('aria-expanded', 'false');
        panel.setAttribute('aria-hidden', 'true');
        root.classList.remove('mm-lock');
        if (returnFocus) trigger.focus();
    }

    function scheduleClose() {
        clearTimeout(openTimer);
        if (pinned) return;
        clearTimeout(closeTimer);
        closeTimer = setTimeout(function () { close(false); }, 220);
    }

    function firstLink() {
        return panel.querySelector('a[href]');
    }

    /* ----- Rê chuột (chỉ máy có chuột; điện thoại/tablet dùng bấm) ----- */
    trigger.addEventListener('mouseenter', function () {
        if (!canHover.matches) return;
        clearTimeout(closeTimer);
        openTimer = setTimeout(open, 90);
    });
    trigger.addEventListener('mouseleave', function () {
        if (canHover.matches) scheduleClose();
    });
    panel.addEventListener('mouseenter', function () {
        if (canHover.matches) clearTimeout(closeTimer);
    });
    panel.addEventListener('mouseleave', function () {
        if (canHover.matches) scheduleClose();
    });

    /* ----- Bấm vào Shop ----- */
    trigger.addEventListener('click', function (e) {
        // Ctrl/Cmd/Shift + click hoặc chuột giữa: vẫn đi tới trang Shop như link thường
        if (e.ctrlKey || e.metaKey || e.shiftKey || e.button === 1) return;
        e.preventDefault();

        if (!isOpen) {
            open();
            pinned = true;
            if (e.detail === 0) {            // bấm bằng bàn phím (Enter)
                var link = firstLink();
                if (link) link.focus();
            }
        } else if (!pinned) {
            pinned = true;                    // đang mở do rê chuột -> giữ lại
        } else {
            close(false);
        }
    });

    /* ----- Bàn phím ----- */
    trigger.addEventListener('keydown', function (e) {
        if (e.key === 'ArrowDown') {
            e.preventDefault();
            open();
            pinned = true;
            var link = firstLink();
            if (link) link.focus();
        }
    });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && isOpen) close(true);
    });

    panel.addEventListener('focusout', function (e) {
        var next = e.relatedTarget;
        if (next && !panel.contains(next) && next !== trigger) close(false);
    });

    /* ----- Bấm ra ngoài / bấm vào một link trong menu ----- */
    document.addEventListener('click', function (e) {
        if (!isOpen) return;
        if (panel.contains(e.target) || trigger.contains(e.target)) return;
        close(false);
    });

    /* ----- Cuộn / đổi kích thước: giữ bảng dính vào mép dưới header ----- */
    var ticking = false;
    function onMove() {
        if (!isOpen || ticking) return;
        ticking = true;
        window.requestAnimationFrame(function () {
            place();
            ticking = false;
        });
    }
    window.addEventListener('scroll', onMove, { passive: true });
    window.addEventListener('resize', onMove);

    // quay lại trang bằng nút Back: đảm bảo menu đóng
    window.addEventListener('pageshow', function () { close(false); });
})();
