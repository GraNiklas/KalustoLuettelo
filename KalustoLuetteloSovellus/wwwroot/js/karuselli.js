// Karuselli koodi
// document.body.classList.add('darkMode');
window.onload = function () {

    const track = document.getElementById('carouselTrack');
    const btnLeft = document.getElementById('btnLeft');
    const btnRight = document.getElementById('btnRight');

    let currentIndex = 0;

    const items = document.querySelectorAll('.carousel-item');

    const totalItems = items.length;
    
    function updateCarousel(itemWidth) {
        // console.log(currentIndex)
        const offset = currentIndex * itemWidth; 
        track.style.transform = `translateX(-${offset}px)`;
    }
    function getItemWidth() {
        return items[0].offsetWidth; // Get the width of the first item
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

        if (currentIndex == totalItems - 3) {
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
        const itemWidth = getItemWidth();
        if (currentIndex < totalItems - 3) {  // Fix: Stop at totalItems - 3 näyttää paremmalle, toivottavasti ei ole sivuvaikutuksia
            currentIndex++;
        
            updateCarousel(itemWidth);
            updateButtons(currentIndex);
        }
    });

    btnLeft.addEventListener('click', () => {
        const itemWidth = getItemWidth();
        if (currentIndex > 0) {
            currentIndex--;
            updateCarousel(itemWidth);
            updateButtons(currentIndex);
        }
    });

};
