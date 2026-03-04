/* ============================================================
   TYPEWRITER — Hero typewriter effect
   Per-character typing/deleting with blinking cursor
   ============================================================ */

(function () {
  'use strict';

  const phrases = [
    'Sitefinity Architecture',
    'Azure Cloud Integration',
    'SPA Modernization',
    'Enterprise CMS Migration',
    'Team Technical Lead',
    '.NET Full-Stack Engineering',
  ];

  const el = document.getElementById('typewriter-text');
  if (!el) return;

  let phraseIndex = 0;
  let charIndex = 0;
  let isDeleting = false;
  let isPaused = false;

  const TYPING_SPEED  = 70;   // ms per character when typing
  const DELETING_SPEED = 40;  // ms per character when deleting
  const PAUSE_AFTER_TYPE = 2000;  // ms pause at end of phrase
  const PAUSE_BEFORE_TYPE = 300;  // ms pause before typing next phrase

  function tick() {
    const currentPhrase = phrases[phraseIndex];

    if (isPaused) {
      isPaused = false;
      isDeleting = true;
      setTimeout(tick, PAUSE_BEFORE_TYPE);
      return;
    }

    if (isDeleting) {
      charIndex--;
      el.textContent = currentPhrase.substring(0, charIndex);

      if (charIndex === 0) {
        isDeleting = false;
        phraseIndex = (phraseIndex + 1) % phrases.length;
        setTimeout(tick, PAUSE_BEFORE_TYPE);
        return;
      }
      setTimeout(tick, DELETING_SPEED);
    } else {
      charIndex++;
      el.textContent = currentPhrase.substring(0, charIndex);

      if (charIndex === currentPhrase.length) {
        isPaused = true;
        setTimeout(tick, PAUSE_AFTER_TYPE);
        return;
      }
      setTimeout(tick, TYPING_SPEED);
    }
  }

  // Respect reduced motion preference — show static text instead
  if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
    el.textContent = phrases[0];
    return;
  }

  // Small initial delay before starting
  setTimeout(tick, 800);
})();
