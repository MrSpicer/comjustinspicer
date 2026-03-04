/* ============================================================
   MAIN — Nav, Mobile Menu, Clipboard
   ============================================================ */

(function () {
  'use strict';

  /* ---- Nav Scroll Behavior ---- */

  function initNavScroll() {
    const nav = document.querySelector('.nav');
    if (!nav) return;

    let ticking = false;
    let lastScrollY = window.scrollY;

    function updateNav() {
      if (lastScrollY > 80) {
        nav.classList.add('is-scrolled');
      } else {
        nav.classList.remove('is-scrolled');
      }
      ticking = false;
    }

    window.addEventListener('scroll', () => {
      lastScrollY = window.scrollY;
      if (!ticking) {
        requestAnimationFrame(updateNav);
        ticking = true;
      }
    }, { passive: true });

    // Run immediately
    updateNav();
  }

  /* ---- Mobile Menu ---- */

  function initMobileMenu() {
    const hamburger = document.querySelector('.nav__hamburger');
    const overlay = document.querySelector('.nav__overlay');
    if (!hamburger || !overlay) return;

    let isOpen = false;
    let previouslyFocused = null;

    function getFocusableElements() {
      return Array.from(overlay.querySelectorAll(
        'a[href], button:not([disabled]), [tabindex]:not([tabindex="-1"])'
      ));
    }

    function openMenu() {
      isOpen = true;
      previouslyFocused = document.activeElement;
      hamburger.classList.add('is-open');
      hamburger.setAttribute('aria-expanded', 'true');
      overlay.classList.add('is-open');
      overlay.removeAttribute('aria-hidden');
      document.body.style.overflow = 'hidden';

      const focusable = getFocusableElements();
      if (focusable.length) {
        setTimeout(() => focusable[0].focus(), 50);
      }
    }

    function closeMenu() {
      isOpen = false;
      hamburger.classList.remove('is-open');
      hamburger.setAttribute('aria-expanded', 'false');
      overlay.classList.remove('is-open');
      overlay.setAttribute('aria-hidden', 'true');
      document.body.style.overflow = '';

      if (previouslyFocused) {
        previouslyFocused.focus();
      }
    }

    hamburger.addEventListener('click', () => {
      isOpen ? closeMenu() : openMenu();
    });

    overlay.addEventListener('click', (e) => {
      if (e.target.tagName === 'A' || e.target.closest('a')) {
        closeMenu();
      }
    });

    document.addEventListener('keydown', (e) => {
      if (e.key === 'Escape' && isOpen) {
        closeMenu();
      }
    });

    overlay.addEventListener('keydown', (e) => {
      if (e.key !== 'Tab') return;
      const focusable = getFocusableElements();
      if (!focusable.length) return;

      const first = focusable[0];
      const last = focusable[focusable.length - 1];

      if (e.shiftKey) {
        if (document.activeElement === first) {
          e.preventDefault();
          last.focus();
        }
      } else {
        if (document.activeElement === last) {
          e.preventDefault();
          first.focus();
        }
      }
    });

    hamburger.setAttribute('aria-expanded', 'false');
    overlay.setAttribute('aria-hidden', 'true');
  }

  /* ---- Copy to Clipboard ---- */

  function initClipboard() {
    const copyButtons = document.querySelectorAll('.copy-btn');
    if (!copyButtons.length) return;

    copyButtons.forEach(btn => {
      btn.addEventListener('click', async () => {
        const text = btn.dataset.copy;
        if (!text) return;

        try {
          await navigator.clipboard.writeText(text);
          showCopied(btn);
        } catch (err) {
          const textarea = document.createElement('textarea');
          textarea.value = text;
          textarea.style.position = 'fixed';
          textarea.style.left = '-9999px';
          document.body.appendChild(textarea);
          textarea.select();
          document.execCommand('copy');
          document.body.removeChild(textarea);
          showCopied(btn);
        }
      });
    });

    function showCopied(btn) {
      const copyIcon = btn.querySelector('.icon-copy');
      const checkIcon = btn.querySelector('.icon-check');
      const tooltip = btn.querySelector('.copy-tooltip');

      btn.classList.add('copied');
      btn.setAttribute('aria-label', 'Copied!');

      if (copyIcon) copyIcon.style.display = 'none';
      if (checkIcon) checkIcon.style.display = 'block';
      if (tooltip) {
        tooltip.textContent = 'Copied!';
        tooltip.style.opacity = '1';
      }

      setTimeout(() => {
        btn.classList.remove('copied');
        btn.setAttribute('aria-label', 'Copy to clipboard');

        if (copyIcon) copyIcon.style.display = '';
        if (checkIcon) checkIcon.style.display = 'none';
        if (tooltip) {
          tooltip.textContent = 'Copy';
          tooltip.style.opacity = '';
        }
      }, 2000);
    }
  }

  /* ---- Prevent FOUC on icons ---- */

  function initIconsVisibility() {
    document.querySelectorAll('.copy-btn .icon-check').forEach(el => {
      el.style.display = 'none';
    });
  }

  /* ---- Init ---- */

  function init() {
    initNavScroll();
    initMobileMenu();
    initClipboard();
    initIconsVisibility();
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
  } else {
    init();
  }

})();
