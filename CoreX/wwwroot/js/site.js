(function () {
  // Toast container — created lazily on first use.
  function getToastContainer() {
    var el = document.getElementById('toast-container');
    if (!el) {
      el = document.createElement('div');
      el.id = 'toast-container';
      el.className = 'fixed top-4 right-4 z-50 flex flex-col gap-2 pointer-events-none';
      el.setAttribute('aria-live', 'polite');
      el.setAttribute('aria-atomic', 'true');
      document.body.appendChild(el);
    }
    return el;
  }

  function showToast(message, type) {
    var container = getToastContainer();
    var toast = document.createElement('div');
    toast.className = 'toast toast-' + (type || 'info');
    toast.textContent = message;
    container.appendChild(toast);
    setTimeout(function () { toast.remove(); }, 4000);
  }

  // HTMX -> showToast event bridge.
  // Server sends: HX-Trigger: {"showToast": {"message": "...", "type": "success"}}
  // OR plain: HX-Trigger: showToast (with no detail; falls back to a generic message).
  document.body.addEventListener('showToast', function (evt) {
    var detail = evt.detail || {};
    showToast(detail.message || 'Готово.', detail.type || 'info');
  });

  // Expose for inline JS callers if needed.
  window.coreXShowToast = showToast;
})();
