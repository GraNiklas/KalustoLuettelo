// Karuselli koodi
// document.body.classList.add('darkMode');
window.onload = function () {

    const track = document.getElementById('carouselTrack');
    const btnLeft = document.getElementById('btnLeft');
    const btnRight = document.getElementById('btnRight');

    let currentIndex = 0;

    const items = document.querySelectorAll('.carousel-item');

    const totalItems = items.length;

    function updateCarousel() {
        // console.log(currentIndex)
        let itemWidth = 500
        const offset = currentIndex * itemWidth; // 500px item width * index
        track.style.transform = `translateX(-${offset}px)`;
    }
    function updateButtons(currentIndex) {
        if (currentIndex == 0) {
            btnLeft.classList.add("disabled");
            btnLeft.classList.add("hidden");   // Piilota
        }
        else {
            btnLeft.classList.remove("disabled");
            btnLeft.classList.remove("hidden"); // Näytä
        }

        if (currentIndex == totalItems - 1) {
            btnRight.classList.add("disabled");
            btnRight.classList.add("hidden"); // Piilota
        }
        else {
            btnRight.classList.remove("disabled");
            btnRight.classList.remove("hidden"); // Näytä
        }
    }

    updateButtons(currentIndex);

    btnRight.addEventListener('click', () => {
        if (currentIndex < totalItems) {
            currentIndex++;
            updateCarousel();
            updateButtons(currentIndex);
        }
    });

    btnLeft.addEventListener('click', () => {
        if (currentIndex > 0) {
            currentIndex--;
            updateCarousel();
            updateButtons(currentIndex);
        }
    });

};
