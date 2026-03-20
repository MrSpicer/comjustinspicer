// Admin JS — burger toggle + delete modal

document.addEventListener('DOMContentLoaded', function () {

    // ── Navbar burger ──────────────────────────────────────────
    const burger = document.getElementById('adminBurger');
    const navMenu = document.getElementById('adminNav');

    if (burger && navMenu) {
        burger.addEventListener('click', function () {
            const isActive = navMenu.classList.toggle('is-active');
            burger.classList.toggle('is-active', isActive);
            burger.setAttribute('aria-expanded', isActive.toString());
        });
    }

    // ── Delete Confirmation Modal ──────────────────────────────
    (function () {
        const modal = document.getElementById('deleteConfirmModal');
        if (!modal) return;

        const form = document.getElementById('deleteConfirmForm');
        const msgEl = document.getElementById('deleteModalMessage');
        const titleEl = document.getElementById('deleteModalTitle');
        let triggerEl = null;

        function getFocusable() {
            return Array.from(modal.querySelectorAll(
                'button:not([disabled]), [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
            ));
        }

        function openModal(name, actionUrl) {
            triggerEl = document.activeElement;
            form.action = actionUrl;
            msgEl.textContent = 'Delete "' + name + '"? This cannot be undone.';
            titleEl.textContent = 'Confirm Delete';
            modal.style.display = '';
            modal.classList.add('is-active');
            const focusable = getFocusable();
            if (focusable[0]) focusable[0].focus();
        }

        function closeModal() {
            modal.classList.remove('is-active');
            modal.style.display = 'none';
            if (triggerEl) triggerEl.focus();
        }

        // Focus trap
        modal.addEventListener('keydown', function (e) {
            if (!modal.classList.contains('is-active')) return;
            if (e.key === 'Escape') { closeModal(); return; }
            if (e.key !== 'Tab') return;
            const focusable = getFocusable();
            const first = focusable[0];
            const last = focusable[focusable.length - 1];
            if (e.shiftKey && document.activeElement === first) {
                e.preventDefault();
                last.focus();
            } else if (!e.shiftKey && document.activeElement === last) {
                e.preventDefault();
                first.focus();
            }
        });

        document.getElementById('deleteModalClose').addEventListener('click', closeModal);
        document.getElementById('deleteModalCancel').addEventListener('click', closeModal);
        document.getElementById('deleteModalBg').addEventListener('click', closeModal);

        // Wire delete trigger buttons
        document.addEventListener('click', function (e) {
            const btn = e.target.closest('[data-delete-trigger]');
            if (!btn) return;
            e.preventDefault();
            openModal(btn.dataset.deleteName || 'this item', btn.dataset.deleteUrl);
        });
    })();

});
