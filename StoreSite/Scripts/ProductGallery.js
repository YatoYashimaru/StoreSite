/* ProductGallery.js - bấm ảnh nhỏ / mũi tên / phím ← → / vuốt để đổi ảnh lớn. Không cần jQuery. */
(function () {
    'use strict';

    var root = document.getElementById('pg');
    if (!root) return;

    var main = document.getElementById('pg-main');
    var thumbs = Array.prototype.slice.call(root.querySelectorAll('.pg-thumb'));
    if (!main || thumbs.length < 2) return;

    var counter = document.getElementById('pg-counter');
    var list = document.getElementById('pg-thumbs');
    var index = 0;
    var token = 0;

    function show(i, fromUser) {
        var n = thumbs.length;
        i = (i + n) % n;                       // đi vòng tròn: sau ảnh cuối là ảnh đầu
        if (i === index && !fromUser) return;
        index = i;

        var src = thumbs[i].getAttribute('data-src');
        var my = ++token;

        // tải ảnh xong rồi mới đổi để không bị nháy trắng
        var pre = new Image();
        pre.onload = pre.onerror = function () {
            if (my !== token) return;          // người dùng đã bấm ảnh khác trong lúc chờ
            main.classList.add('is-fading');
            setTimeout(function () {
                if (my !== token) return;
                main.src = src;
                main.classList.remove('is-fading');
            }, 120);
        };
        pre.src = src;

        thumbs.forEach(function (t, k) {
            var active = k === i;
            t.classList.toggle('is-active', active);
            t.setAttribute('aria-current', active ? 'true' : 'false');
        });

        if (counter) counter.textContent = (i + 1) + ' / ' + thumbs.length;

        // cuộn cột ảnh nhỏ để ảnh đang chọn luôn nằm trong tầm nhìn (không cuộn cả trang)
        var t = thumbs[i];
        if (list) {
            if (list.scrollHeight > list.clientHeight + 1 && getComputedStyle(list).flexDirection === 'column') {
                list.scrollTo({ top: t.offsetTop - (list.clientHeight - t.offsetHeight) / 2, behavior: 'smooth' });
            } else {
                list.scrollTo({ left: t.offsetLeft - (list.clientWidth - t.offsetWidth) / 2, behavior: 'smooth' });
            }
        }
    }

    thumbs.forEach(function (t, k) {
        t.addEventListener('click', function () { show(k, true); });
    });

    var prev = document.getElementById('pg-prev');
    var next = document.getElementById('pg-next');
    if (prev) prev.addEventListener('click', function () { show(index - 1, true); });
    if (next) next.addEventListener('click', function () { show(index + 1, true); });

    // Phím mũi tên khi đang chọn một phần của bộ ảnh
    root.addEventListener('keydown', function (e) {
        if (e.key === 'ArrowLeft' || e.key === 'ArrowUp') { e.preventDefault(); show(index - 1, true); }
        else if (e.key === 'ArrowRight' || e.key === 'ArrowDown') { e.preventDefault(); show(index + 1, true); }
    });

    // Vuốt ngang trên ảnh lớn (điện thoại / tablet)
    var stage = main.parentElement;
    var x0 = null, y0 = null;
    stage.addEventListener('touchstart', function (e) {
        var t = e.changedTouches[0];
        x0 = t.clientX; y0 = t.clientY;
    }, { passive: true });
    stage.addEventListener('touchend', function (e) {
        if (x0 === null) return;
        var t = e.changedTouches[0];
        var dx = t.clientX - x0, dy = t.clientY - y0;
        x0 = y0 = null;
        if (Math.abs(dx) > 40 && Math.abs(dx) > Math.abs(dy) * 1.5) show(index + (dx < 0 ? 1 : -1), true);
    }, { passive: true });

    // Tải sẵn các ảnh còn lại khi trình duyệt rảnh để bấm là đổi ngay
    var preload = function () {
        thumbs.forEach(function (t) { (new Image()).src = t.getAttribute('data-src'); });
    };
    if ('requestIdleCallback' in window) window.requestIdleCallback(preload);
    else setTimeout(preload, 800);
})();
