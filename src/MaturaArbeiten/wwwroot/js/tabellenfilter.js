document.addEventListener('input', function (ereignis) {
    var feld = ereignis.target;
    var tabelle = feld.dataset ? feld.dataset.filtertabelle : null;

    if (!tabelle) {
        return;
    }

    var suchtext = feld.value.trim().toLowerCase();

    document.querySelectorAll('#' + tabelle + ' tbody tr').forEach(function (zeile) {
        zeile.hidden = suchtext.length > 0 && zeile.textContent.toLowerCase().indexOf(suchtext) < 0;
    });
});
