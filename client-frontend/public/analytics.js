// ============================================================
// Google Analytics 4 — cargador único para todo el sitio
// ============================================================
// Mismo ID en los 4 sitios (portafolio, ATLAS Lab, Certis y Praxis TOGAF);
// en los informes se separan por dominio. Política de privacidad:
// https://curricula-fawn.vercel.app/privacidad/
// ============================================================
(function () {
  var GA_ID = 'G-N2H066P5K6';

  if (!/^G-[A-Z0-9]+$/.test(GA_ID)) return;

  var script = document.createElement('script');
  script.async = true;
  script.src = 'https://www.googletagmanager.com/gtag/js?id=' + GA_ID;
  document.head.appendChild(script);

  window.dataLayer = window.dataLayer || [];
  window.gtag = function () { window.dataLayer.push(arguments); };
  window.gtag('js', new Date());
  window.gtag('config', GA_ID);
})();
