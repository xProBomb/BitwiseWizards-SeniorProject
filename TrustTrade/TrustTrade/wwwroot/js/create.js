document.addEventListener('DOMContentLoaded', function () {
    // Add a new tag functionality
    const addTagButton = document.getElementById('addTagButton');
    const newTagInput = document.getElementById('newTag');
    const tagsContainer = document.querySelector('.scrollable-checkbox-tags .row');

    if (addTagButton && newTagInput && tagsContainer) {
        addTagButton.addEventListener('click', function () {
            const newTagValue = newTagInput.value.trim();
            if (newTagValue) {
                // Create a new checkbox for the tag
                const colDiv = document.createElement('div');
                colDiv.className = 'col-sm-6';

                const formCheckDiv = document.createElement('div');
                formCheckDiv.className = 'form-check mb-2';

                const checkbox = document.createElement('input');
                checkbox.type = 'checkbox';
                checkbox.className = 'form-check-input';
                checkbox.value = newTagValue;
                checkbox.id = `tag-${newTagValue}`;
                checkbox.name = 'SelectedTags';
                checkbox.checked = true;

                const label = document.createElement('label');
                label.className = 'form-check-label';
                label.htmlFor = `tag-${newTagValue}`;
                label.textContent = newTagValue;

                formCheckDiv.appendChild(checkbox);
                formCheckDiv.appendChild(label);
                colDiv.appendChild(formCheckDiv);
                tagsContainer.appendChild(colDiv);

                // Clear the input field
                newTagInput.value = '';
            } else {
                alert('Please enter a tag name.');
            }
        });
    }

    // Image upload functionality
    const imageInput = document.getElementById('imageInput');
    const addImageButton = document.getElementById('addImageButton');
    const imagePreview = document.getElementById('imagePreview');
    const form = document.querySelector('form');
    
    // Array to store the Base64 strings
    const imageBase64Array = [];
    
    // Maximum number of images allowed
    const MAX_IMAGES = 5;
    // Maximum size per image in bytes (2MB)
    const MAX_FILE_SIZE = 2 * 1024 * 1024;
    // Allowed file types
    const ALLOWED_TYPES = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif'];

    const photosInput = document.querySelector('input[name="Photos"]'); 

    if (addImageButton && imageInput && imagePreview) {
        addImageButton.addEventListener('click', function () {
            if (imageInput.files.length === 0) {
                alert('Please select at least one image file.');
                return;
            }

            if (imageBase64Array.length + imageInput.files.length > MAX_IMAGES) {
                alert(`You can only upload a maximum of ${MAX_IMAGES} images.`);
                return;
            }

            Array.from(imageInput.files).forEach(file => {
                // Check file type
                if (!ALLOWED_TYPES.includes(file.type)) {
                    alert(`File "${file.name}" is not an allowed image type. Please use JPG, PNG, or GIF.`);
                    return;
                }

                // Check file size
                if (file.size > MAX_FILE_SIZE) {
                    alert(`File "${file.name}" exceeds the 2MB size limit.`);
                    return;
                }

                // Create file reader to read the file as Data URL (Base64)
                const reader = new FileReader();

                reader.onload = function (e) {
                    const base64String = e.target.result;

                    // Add to array
                    imageBase64Array.push(base64String);

                    // Update the value of the hidden input
                    photosInput.value = imageBase64Array.join(',');

                    // Create image preview
                    const imgContainer = document.createElement('div');
                    imgContainer.className = 'position-relative';

                    const img = document.createElement('img');
                    img.src = base64String;
                    img.className = 'img-thumbnail';
                    img.style.width = '100px';
                    img.style.height = '100px';
                    img.style.objectFit = 'cover';

                    // Add delete button
                    const deleteBtn = document.createElement('button');
                    deleteBtn.type = 'button';
                    deleteBtn.className = 'btn btn-sm btn-danger position-absolute top-0 end-0';
                    deleteBtn.innerHTML = '&times;';
                    deleteBtn.style.borderRadius = '50%';
                    deleteBtn.style.padding = '0 6px';

                    deleteBtn.addEventListener('click', function () {
                        // Remove from array
                        const index = imageBase64Array.indexOf(base64String);
                        if (index > -1) {
                            imageBase64Array.splice(index, 1);
                        }

                        // Update the value of the hidden input
                        photosInput.value = imageBase64Array.join(',');

                        // Remove preview
                        imgContainer.remove();
                    });

                    imgContainer.appendChild(img);
                    imgContainer.appendChild(deleteBtn);
                    imagePreview.appendChild(imgContainer);
                };

                reader.readAsDataURL(file);
            });

            // Clear the file input
            imageInput.value = '';
        });
    }

    document.getElementById('submitButton').addEventListener('click', function () {
        this.disabled = true;
        this.innerHTML = '<i class="bi bi-hourglass-split me-1"></i> Posting...';
        this.form.submit();
    });
});