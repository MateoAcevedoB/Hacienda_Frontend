

(function () {
 document.addEventListener('DOMContentLoaded', function () {
 const alerts = document.querySelectorAll('.alert');
 alerts.forEach(function (al) {
 const timeoutAttr = al.getAttribute('data-timeout');
 const timeout = timeoutAttr ? parseInt(timeoutAttr,10) : 15000;
 if (!isNaN(timeout) && timeout >0) {
 setTimeout(() => {
 al.classList.add('fade');
 const btn = al.querySelector('.btn-close');
 if (btn) btn.click(); else al.remove();
 }, timeout);
 }
 });

 const reveal = document.querySelectorAll('.aw-card, .aw-page-header, .aw-reveal');
 const onScroll = () => {
 const trigger = window.innerHeight *0.92;
 reveal.forEach(el => {
 const top = el.getBoundingClientRect().top;
 if (top < trigger) el.classList.add('aw-show');
 });
 };
 onScroll();
 window.addEventListener('scroll', onScroll, { passive: true });

 document.querySelectorAll('.aw-btn').forEach(btn => {
 btn.addEventListener('mouseenter', () => btn.classList.add('shadow-sm'));
 btn.addEventListener('mouseleave', () => btn.classList.remove('shadow-sm'));
 });
 });
})();
