// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(function () {
    $('#followersModal').modal({
        show: false
    });

    $('#followingModal').modal({
        show: false
    });

    // Handle follow/unfollow button click
    $('form.follow-unfollow-form').on('submit', function (e) {
        e.preventDefault();
        var form = $(this);
        var actionUrl = form.attr('action');
        var formData = form.serialize();

        $.post(actionUrl, formData, function (response) {
            location.reload();
        }).fail(function () {
            alert('An error occurred. Please try again.');
        });
    });
});
