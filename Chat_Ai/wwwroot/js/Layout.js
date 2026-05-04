
        // Hide the loader once the page is fully loaded
    window.addEventListener('load', function () {
            const loader = document.getElementById('page-loader');
    if (loader) {
        // Add a small delay for a smoother visual experience
        setTimeout(function () {
            loader.style.opacity = '0';
            setTimeout(function () {
                loader.style.display = 'none';
            }, 500); // Matches the CSS transition duration
        }, 400); 
            }
        });
