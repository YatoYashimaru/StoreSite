/* ProductGallery.js - bấm ảnh nhỏ / mũi tên / phím ← → / vuốt để đổi ảnh lớn.
   Không cần jQuery. Dùng "ủy quyền sự kiện" trên document nên chạy đúng dù script được nạp
   ở <head>, cuối <body> hay trong @section Scripts, và không bị hỏng khi trang được ghép từ nhiều nhánh. */
(function () {
    'use strict';

    if (window.__pgReady) return;          // tránh gắn hai lần nếu file bị nạp 2 lần (hay gặp sau khi merge)
    window.__pgReady = true;

    var token = 0;

    function parts() {
        var root = document.getElementById('pg');
        var main = document.getElementById('pg-main');
        if (!root || !main) return null;
        var thumbs = Array.prototype.slice.call(root.querySelectorAll('.pg-thumb'));
        if (thumbs.length < 2) return null;
        return {
            root: root,
            main: main,
            thumbs: thumbs,
            counter: document.getElementById('pg-counter'),
            list: document.getElementById('pg-thumbs')
        };
    }

    function currentIndex(p) {
        for (var i = 0; i < p.thumbs.length; i++) {
            if (p.thumbs[i].classList.contains('is-active')) return i;
        }
        return 0;
    }

    function show(p, i) {
        var n = p.thumbs.length;
        i = (i + n) % n;                       // đi vòng tròn: sau ảnh cuối là ảnh đầu

        var src = p.thumbs[i].getAttribute('data-src');
        var my = ++token;

        // tải ảnh xong rồi mới đổi để không bị nháy trắng
        var pre = new Image();
        pre.onload = pre.onerror = function () {
            if (my !== token) return;          // người dùng đã bấm ảnh khác trong lúc chờ
            p.main.classList.add('is-fading');
            setTimeout(function () {
                if (my !== token) return;
                p.main.removeAttribute('srcset');
                p.main.src = src;
                p.main.classList.remove('is-fading');
            }, 120);
        };
        pre.src = src;

        p.thumbs.forEach(function (t, k) {
            var active = k === i;
            t.classList.toggle('is-active', active);
            t.setAttribute('aria-current', active ? 'true' : 'false');
        });

        if (p.counter) p.counter.textContent = (i + 1) + ' / ' + n;

        // cuộn cột ảnh nhỏ để ảnh đang chọn luôn nằm trong tầm nhìn (không cuộn cả trang)
        var t = p.thumbs[i], list = p.list;
        if (list && list.scrollTo) {
            if (getComputedStyle(list).flexDirection === 'column') {
                list.scrollTo({ top: t.offsetTop - (list.clientHeight - t.offsetHeight) / 2, behavior: 'smooth' });
            } else {
                list.scrollTo({ left: t.offsetLeft - (list.clientWidth - t.offsetWidth) / 2, behavior: 'smooth' });
            }
        }
    }

    function closest(el, selector) {
        while (el && el.nodeType === 1) {
            if ((el.matches || el.msMatchesSelector).call(el, selector)) return el;
            el = el.parentNode;
        }
        return null;
    }

    /* ----- Bấm ảnh nhỏ / nút trước / nút sau ----- */
    document.addEventListener('click', function (e) {
        var hit = closest(e.target, '.pg-thumb, #pg-prev, #pg-next');
        if (!hit) return;

        var p = parts();
        if (!p || !p.root.contains(hit)) return;

        e.preventDefault();
        if (hit.id === 'pg-prev') show(p, currentIndex(p) - 1);
        else if (hit.id === 'pg-next') show(p, currentIndex(p) + 1);
        else show(p, p.thumbs.indexOf(hit));
    });

    /* ----- Phím mũi tên khi đang chọn một phần của bộ ảnh ----- */
    document.addEventListener('keydown', function (e) {
        if (!closest(e.target, '#pg')) return;
        var dir = (e.key === 'ArrowLeft' || e.key === 'ArrowUp') ? -1
            : (e.key === 'ArrowRight' || e.key === 'ArrowDown') ? 1 : 0;
        if (!dir) return;

        var p = parts();
        if (!p) return;
        e.preventDefault();
        show(p, currentIndex(p) + dir);
    });

    /* ----- Vuốt ngang trên ảnh lớn (điện thoại / tablet) ----- */
    var x0 = null, y0 = null;
    document.addEventListener('touchstart', function (e) {
        if (!closest(e.target, '.pg-stage')) { x0 = null; return; }
        var t = e.changedTouches[0];
        x0 = t.clientX; y0 = t.clientY;
    }, { passive: true });

    document.addEventListener('touchend', function (e) {
        if (x0 === null) return;
        var t = e.changedTouches[0];
        var dx = t.clientX - x0, dy = t.clientY - y0;
        x0 = y0 = null;
        if (Math.abs(dx) < 40 || Math.abs(dx) < Math.abs(dy) * 1.5) return;

        var p = parts();
        if (p) show(p, currentIndex(p) + (dx < 0 ? 1 : -1));
    }, { passive: true });

    /* ----- Tải sẵn các ảnh còn lại khi trình duyệt rảnh để bấm là đổi ngay ----- */
    function preload() {
        var p = parts();
        if (!p) return;
        p.thumbs.forEach(function (t) { (new Image()).src = t.getAttribute('data-src'); });
    }
    function schedule() {
        if ('requestIdleCallback' in window) window.requestIdleCallback(preload);
        else setTimeout(preload, 800);
    }
    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', schedule);
    else schedule();
})();
