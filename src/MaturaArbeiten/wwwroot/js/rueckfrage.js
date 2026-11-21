document.addEventListener('submit', function (ereignis) {
    var rueckfrage = ereignis.target.dataset ? ereignis.target.dataset.rueckfrage : null;

    if (rueckfrage && !window.confirm(rueckfrage)) {
        ereignis.preventDefault();
    }
});

document.addEventListener('click', function (ereignis) {
    if (ereignis.target.closest('[data-drucken]')) {
        window.print();
    }
});
