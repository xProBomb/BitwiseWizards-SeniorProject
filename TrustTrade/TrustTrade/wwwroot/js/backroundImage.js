// Profile background image functionality with Cropper.js
document.addEventListener('DOMContentLoaded', function() {
    console.log('Enhanced profile background image functionality loaded');

    // DOM Elements
    const backgroundModal = document.getElementById('updateBackgroundImageModal');
    const fileInput = document.getElementById('backgroundImageInput');
    const urlInput = document.getElementById('backgroundImageUrl');
    const imagePreviewContainer = document.getElementById('imagePreviewContainer');
    const previewImage = document.getElementById('previewImage');
    const uploadForm = document.getElementById('backgroundImageForm');
    const sourceTypeToggle = document.querySelectorAll('input[name="sourceType"]');
    const fileUploadContainer = document.getElementById('fileUploadContainer');
    const urlInputContainer = document.getElementById('urlInputContainer');
    const rotateLeftBtn = document.getElementById('rotateLeftBtn');
    const rotateRightBtn = document.getElementById('rotateRightBtn');
    const zoomInBtn = document.getElementById('zoomInBtn');
    const zoomOutBtn = document.getElementById('zoomOutBtn');
    const resetBtn = document.getElementById('resetCropperBtn');
    const saveBtn = document.getElementById('saveCropBtn');

    // Cropper instance
    let cropper = null;

    // Initialize source type (default to File)
    let currentSourceType = 'File';
    document.getElementById('sourceType-file').checked = true;

    // Toggle between file upload and URL input
    sourceTypeToggle.forEach(radio => {
        radio.addEventListener('change', function() {
            currentSourceType = this.value;
            toggleSourceInputs(currentSourceType);

            // Reset form when switching source types
            if (cropper) {
                cropper.destroy();
                cropper = null;
            }

            if (previewImage.src) {
                previewImage.src = '';
                previewImage.style.display = 'none';
            }

            fileInput.value = '';
            if (urlInput) urlInput.value = '';

            document.getElementById('backgroundSourceInput').value = currentSourceType;
        });
    });

    function toggleSourceInputs(sourceType) {
        if (sourceType === 'File') {
            fileUploadContainer.style.display = 'block';
            if (urlInputContainer) urlInputContainer.style.display = 'none';
        } else {
            fileUploadContainer.style.display = 'none';
            if (urlInputContainer) urlInputContainer.style.display = 'block';
        }
    }

    // Initialize file input change event
    if (fileInput) {
        fileInput.addEventListener('change', handleFileSelect);
    }

    // Initialize URL input change event (if available)
    if (urlInput) {
        urlInput.addEventListener('change', handleUrlInput);
        urlInput.addEventListener('paste', function() {
            // Small delay to get the pasted content
            setTimeout(handleUrlInput, 100);
        });
    }

    // Handle file selection
    function handleFileSelect(event) {
        const file = event.target.files[0];

        if (!file) {
            console.log('No file selected');
            return;
        }

        // Validate file type
        if (!file.type.match('image.*')) {
            alert('Please select a valid image file.');
            fileInput.value = '';
            return;
        }

        // Validate file size (5MB limit)
        if (file.size > 5 * 1024 * 1024) {
            alert('Image file is too large. Maximum size is 5MB.');
            fileInput.value = '';
            return;
        }

        const reader = new FileReader();

        reader.onload = function(e) {
            initializeImagePreview(e.target.result);
        };

        reader.onerror = function() {
            console.error('Error reading file:', reader.error);
            alert('Error reading file. Please try again.');
        };

        try {
            reader.readAsDataURL(file);
        } catch (err) {
            console.error('Error initiating file read:', err);
            alert('Error reading file: ' + err.message);
        }
    }

    // Handle URL input
    function handleUrlInput() {
        if (!urlInput) return;

        const imageUrl = urlInput.value.trim();

        if (!imageUrl) {
            return;
        }

        // Simple URL validation
        if (!isValidUrl(imageUrl)) {
            alert('Please enter a valid image URL.');
            return;
        }

        // Load the image from URL
        initializeImagePreview(imageUrl);
    }

    // Validate URL format
    function isValidUrl(string) {
        try {
            new URL(string);
            return true;
        } catch (_) {
            return false;
        }
    }

    // Initialize image preview with Cropper.js
    function initializeImagePreview(imageSrc) {
        if (!previewImage || !imagePreviewContainer) {
            console.error('Preview elements not found');
            return;
        }

        // Display the image
        previewImage.src = imageSrc;
        previewImage.style.display = 'block';

        // Destroy existing cropper if any
        if (cropper) {
            cropper.destroy();
        }

        // Initialize cropper with a slight delay to ensure image is loaded
        setTimeout(() => {
            cropper = new Cropper(previewImage, {
                viewMode: 1, // Restrict the crop box to not exceed the size of the canvas
                dragMode: 'move', // Move the image by default instead of crop
                aspectRatio: 16 / 5, // Set a fixed aspect ratio for the profile banner
                autoCropArea: 1, // The initial crop area will be the whole image area
                restore: false, // Don't restore the cropped area after resizing the window
                guides: true, // Show the dashed lines for guiding
                center: true, // Show the center indicator for guiding
                highlight: false, // Don't highlight the crop box
                cropBoxMovable: true, // Allow to move the crop box
                cropBoxResizable: true, // Allow to resize the crop box
                toggleDragModeOnDblclick: false, // Don't toggle between crop and move on double click
                responsive: true, // Re-render the cropper when the window resizes
                minContainerWidth: 250,
                minContainerHeight: 150,
                // Enable cropper.js backdrop with custom styling to match theme
                modal: true,
                background: true
            });

            // Enable the control buttons once cropper is initialized
            enableCropperControls(true);

            console.log('Cropper initialized successfully');
        }, 200);
    }

    // Add event listeners for cropper controls
    if (rotateLeftBtn) {
        rotateLeftBtn.addEventListener('click', () => {
            if (cropper) cropper.rotate(-90);
        });
    }

    if (rotateRightBtn) {
        rotateRightBtn.addEventListener('click', () => {
            if (cropper) cropper.rotate(90);
        });
    }

    if (zoomInBtn) {
        zoomInBtn.addEventListener('click', () => {
            if (cropper) cropper.zoom(0.1);
        });
    }

    if (zoomOutBtn) {
        zoomOutBtn.addEventListener('click', () => {
            if (cropper) cropper.zoom(-0.1);
        });
    }

    if (resetBtn) {
        resetBtn.addEventListener('click', () => {
            if (cropper) cropper.reset();
        });
    }

    // Handle modal open/close events
    if (backgroundModal) {
        backgroundModal.addEventListener('show.bs.modal', function() {
            console.log('Background image modal opening');

            // Reset the form
            if (uploadForm) uploadForm.reset();

            // Reset cropper if it exists
            if (cropper) {
                cropper.destroy();
                cropper = null;
            }

            // Reset preview
            if (previewImage) {
                previewImage.src = '';
                previewImage.style.display = 'none';
            }

            // Reset source type to default (File)
            document.getElementById('sourceType-file').checked = true;
            currentSourceType = 'File';
            toggleSourceInputs(currentSourceType);
            document.getElementById('backgroundSourceInput').value = 'File';

            // Disable cropper controls
            enableCropperControls(false);
        });

        backgroundModal.addEventListener('hidden.bs.modal', function() {
            // Clean up when modal is closed
            if (cropper) {
                cropper.destroy();
                cropper = null;
            }
        });
    }

    // Enable/disable cropper control buttons
    function enableCropperControls(enabled) {
        const controls = [
            rotateLeftBtn,
            rotateRightBtn,
            zoomInBtn,
            zoomOutBtn,
            resetBtn,
            saveBtn
        ];

        controls.forEach(control => {
            if (control) {
                control.disabled = !enabled;
                if (enabled) {
                    control.classList.remove('disabled');
                } else {
                    control.classList.add('disabled');
                }
            }
        });
    }

    // Handle form submission
    if (uploadForm) {
        uploadForm.addEventListener('submit', function(e) {
            // Only if we're using Cropper and it's been initialized
            if (cropper && document.getElementById('includeCroppedData').checked) {
                e.preventDefault();

                // Get the cropped canvas data
                const canvas = cropper.getCroppedCanvas({
                    width: 1200, // Set a reasonable max width
                    height: 400, // Set a reasonable max height
                    fillColor: '#fff',
                    imageSmoothingEnabled: true,
                    imageSmoothingQuality: 'high',
                });

                // Convert canvas to blob
                canvas.toBlob(function(blob) {
                    // Create a new File object from the blob
                    const croppedImage = new File([blob], 'background.jpg', { type: 'image/jpeg' });

                    // Create a new FormData and append the cropped image
                    const formData = new FormData();
                    formData.append('BackgroundImage', croppedImage);
                    formData.append('BackgroundSource', 'File');

                    // Use fetch API to submit the form
                    fetch(uploadForm.action, {
                        method: 'POST',
                        body: formData,
                        credentials: 'same-origin'
                    })
                        .then(response => {
                            if (response.ok) {
                                // Close the modal
                                const modal = bootstrap.Modal.getInstance(backgroundModal);
                                if (modal) modal.hide();

                                // Reload the page to show the new background
                                window.location.reload();
                            } else {
                                throw new Error('Network response was not ok');
                            }
                        })
                        .catch(error => {
                            console.error('There was a problem with the fetch operation:', error);
                            alert('Error uploading image. Please try again.');
                        });
                }, 'image/jpeg', 0.9); // 90% quality JPEG
            }
            // Otherwise, let the normal form submission happen
        });
    }

    console.log('Profile background image functionality initialization complete');
});