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

    document.getElementById('submitButton').addEventListener('click', function () {
        this.disabled = true;
        this.innerHTML = '<i class="bi bi-hourglass-split me-1"></i> Posting...';
        this.form.submit();
    });
});