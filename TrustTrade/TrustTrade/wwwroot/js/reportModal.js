// wwwroot/js/reportModal.js
document.addEventListener('DOMContentLoaded', function() {
    const reportModal = document.getElementById('reportModal');
    if (!reportModal) return;

    const modalInstance = new bootstrap.Modal(reportModal);
    const reportForm = document.getElementById('reportForm');
    const reportCategory = document.getElementById('reportCategory');
    const reportDescription = document.getElementById('reportDescription');
    const charCount = document.getElementById('charCount');
    const submitBtn = document.getElementById('submitReportBtn');
    const submitBtnText = document.getElementById('submitBtnText');
    const submitBtnSpinner = document.getElementById('submitBtnSpinner');
    const reportEntityId = document.getElementById('reportEntityId');
    const reportType = document.getElementById('reportType');
    const reporterInfo = document.getElementById('reporterInfo');
    const reportedContentInfo = document.getElementById('reportedContentInfo');
    const previousReportWarning = document.getElementById('previousReportWarning');
    const successToast = new bootstrap.Toast(document.getElementById('reportSuccessToast'));

    // Character counter
    reportDescription.addEventListener('input', function() {
        charCount.textContent = this.value.length;
    });

    // Open report modal function
    window.openReportModal = async function(type, entityId) {
        // Reset form
        reportForm.reset();
        charCount.textContent = '0';
        reportCategory.classList.remove('is-invalid');
        previousReportWarning.classList.add('d-none');

        // Set hidden values
        reportType.value = type;
        reportEntityId.value = entityId;

        // Update modal title
        document.getElementById('reportModalLabel').textContent = `Report ${type}`;

        // Load report info
        await loadReportInfo(type, entityId);

        // Show modal
        modalInstance.show();
    };

    // Load reporter and reportee information
    async function loadReportInfo(type, entityId) {
        try {
            const response = await fetch(`/Report/GetReportInfo?reportType=${type}&entityId=${entityId}`);
            const data = await response.json();

            if (data.success) {
                // Display reporter info
                reporterInfo.innerHTML = `
                    <strong>${data.reporter.reporterName}</strong><br>
                    <small class="text-muted">${data.reporter.reporterEmail}</small>
                `;

                // Display reportee info based on type
                if (type === 'Post' && data.reportee) {
                    reportedContentInfo.innerHTML = `
                        <strong>Post:</strong> "${data.reportee.postTitle}"<br>
                        <small class="text-muted">By: ${data.reportee.postAuthor}</small>
                    `;
                } else if (type === 'Profile' && data.reportee) {
                    reportedContentInfo.innerHTML = `
                        <strong>User Profile:</strong> ${data.reportee.profileUsername}<br>
                        <small class="text-muted">User ID: ${data.reportee.profileId}</small>
                    `;
                }
            }
        } catch (error) {
            console.error('Error loading report info:', error);
            reporterInfo.innerHTML = '<span class="text-danger">Error loading information</span>';
            reportedContentInfo.innerHTML = '<span class="text-danger">Error loading information</span>';
        }
    }

    // Submit report
    submitBtn.addEventListener('click', async function() {
        // Validate category
        if (!reportCategory.value) {
            reportCategory.classList.add('is-invalid');
            return;
        }

        reportCategory.classList.remove('is-invalid');

        // Disable submit button and show spinner
        submitBtn.disabled = true;
        submitBtnText.classList.add('d-none');
        submitBtnSpinner.classList.remove('d-none');

        try {
            const endpoint = reportType.value === 'Post'
                ? '/Report/ReportPost'
                : '/Report/ReportProfile';

            const formData = new FormData();
            formData.append(reportType.value === 'Post' ? 'postId' : 'profileId', reportEntityId.value);
            formData.append('category', reportCategory.value);
            formData.append('description', reportDescription.value);

            const response = await fetch(endpoint, {
                method: 'POST',
                body: formData
            });

            const result = await response.json();

            if (result.success) {
                // Close modal and show success toast
                modalInstance.hide();
                successToast.show();

                // Reset form
                reportForm.reset();
                charCount.textContent = '0';
            } else {
                if (result.warning) {
                    // Show previous report warning
                    previousReportWarning.classList.remove('d-none');
                } else {
                    // Show error message
                    alert(result.message || 'An error occurred while submitting your report.');
                }
            }
        } catch (error) {
            console.error('Error submitting report:', error);
            alert('An error occurred while submitting your report. Please try again.');
        } finally {
            // Re-enable submit button
            submitBtn.disabled = false;
            submitBtnText.classList.remove('d-none');
            submitBtnSpinner.classList.add('d-none');
        }
    });

    // Clear validation on category change
    reportCategory.addEventListener('change', function() {
        this.classList.remove('is-invalid');
    });
});