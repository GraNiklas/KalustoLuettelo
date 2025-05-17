window.onload = function () {
    const track = document.getElementById('carouselTrack');
    const btnLeft = document.getElementById('btnLeft');
    const btnRight = document.getElementById('btnRight');
    const items = document.querySelectorAll('.carousel-item');
    const view = document.querySelector('.carousel-view');

    let currentIndex = 0;
    const totalItems = items.length;

    function getItemWidth() {
        return items[0].offsetWidth;
    }
    function getItemFullWidth() {
        const style = window.getComputedStyle(items[0]);
        const marginLeft = parseFloat(style.marginLeft);
        const marginRight = parseFloat(style.marginRight);
        return items[0].offsetWidth + marginLeft + marginRight;
    }
    function getVisibleCount() {
        return Math.floor(view.offsetWidth / getItemFullWidth());
    }

    function getMaxIndex() {
        return Math.max(0, totalItems - getVisibleCount());
    }

    function updateCarousel(itemWidth) {
        const offset = currentIndex * itemWidth;
        track.style.transform = `translateX(-${offset}px)`;
    }

    function updateButtons(index) {
        const maxIndex = getMaxIndex();
        btnLeft.classList.toggle("disabled", index === 0);
        btnLeft.classList.toggle("hidden", index === 0);
        btnRight.classList.toggle("disabled", index >= maxIndex);
        btnRight.classList.toggle("hidden", index >= maxIndex);
    }

    updateButtons(currentIndex);

    btnRight.addEventListener('click', () => {
        const itemWidth = getItemFullWidth();
        const maxIndex = getMaxIndex();
        if (currentIndex < maxIndex) {
            currentIndex++;
            updateCarousel(itemWidth);
            updateButtons(currentIndex);
        }
    });

    btnLeft.addEventListener('click', () => {
        const itemWidth = getItemFullWidth();
        if (currentIndex > 0) {
            currentIndex--;
            updateCarousel(itemWidth);
            updateButtons(currentIndex);
        }
    });

    // Auto-scroll
    const interval = 3000; // ms
    setInterval(() => {
        const itemWidth = getItemFullWidth();
        const maxIndex = getMaxIndex();
        if (currentIndex < maxIndex) {
            currentIndex++;
        } else {
            currentIndex = 0;
        }
        updateCarousel(itemWidth);
        updateButtons(currentIndex);
    }, interval);
};
