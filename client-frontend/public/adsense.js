// ============================================================
// Google AdSense — cargador único para todo el sitio
// ============================================================
// Pega aquí tu ID de editor (AdSense → Cuenta → Información de la cuenta).
// Mientras siga el valor de ejemplo, no se carga nada (ni anuncios ni red).
// Recuerda poner el mismo ID en /ads.txt.
// ============================================================
(function () {
  var ADSENSE_CLIENT = 'ca-pub-XXXXXXXXXXXXXXXX';

  if (!/^ca-pub-\d{16}$/.test(ADSENSE_CLIENT)) return;

  var meta = document.createElement('meta');
  meta.name = 'google-adsense-account';
  meta.content = ADSENSE_CLIENT;
  document.head.appendChild(meta);

  // Auto ads: Google decide ubicación y cantidad (se configura en el panel)
  var script = document.createElement('script');
  script.async = true;
  script.crossOrigin = 'anonymous';
  script.src = 'https://pagead2.googlesyndication.com/pagead/js/adsbygoogle.js?client=' + ADSENSE_CLIENT;
  document.head.appendChild(script);

  // Bloques manuales: <ins class="adsbygoogle" data-ad-slot="..."> en las páginas estáticas
  window.addEventListener('DOMContentLoaded', function () {
    var slots = document.querySelectorAll('ins.adsbygoogle[data-ad-slot]:not([data-ad-slot=""])');
    for (var i = 0; i < slots.length; i++) {
      slots[i].setAttribute('data-ad-client', ADSENSE_CLIENT);
      (window.adsbygoogle = window.adsbygoogle || []).push({});
    }
  });
})();
