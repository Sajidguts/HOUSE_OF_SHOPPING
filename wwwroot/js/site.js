// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(function () {
    let otpSent = false; // track whether OTP was sent

    $("#showOtpBtn").on("click", function () {
        debugger;
        if (!otpSent) {
            // First click - submit registration to server via AJAX
            $.ajax({
                url: '/Account/register',
                method: 'POST',
                data: $("#registerForm").serialize(),
                success: function (response) {
                    if (response.success) {
                        debugger;
                        $("#otpSection").slideDown(); // Show OTP input field
                        otpSent = true;
                        $("#showOtpBtn").val("Submit OTP"); // Change button text
                    } else {
                      //  alert("Registration Failed: " + (response.message || response.errors.join(", ")));
                    }
                }
            });
        } else {
            debugger;
            // After registration - now submitting the entered OTP
            var otp = $("#otp").val().trim();
            if (otp === "") {
                $("#otpError").text("Please enter the OTP before submitting.");
            } else {
               
                $("#otpError").text("");
                // Submit OTP to verify
                $.ajax({
                    url: '/Account/VerifyOtpAjax',
                    method: 'POST',
                    data: { enteredOtp: otp },
                    success: function (response) {
                     
                        if (response.success) {


                            alert(response.message);
                            window.location.href = '/Account/login';
                        } else {
                            $("#otpError").text(response.message);
                        }
                    }
                });
            }
        }
    });





    //////////////////////////////////////////////////////////////////////////
    $.ajax({
        url: '/Cart/AddToCartAjax',
        method: 'Post',
        data: { productId: 0, selectedsize: null },
        success: function (response) {
            $('#cartStatus').text(response.message);
            if (response.success) {

                if (response.cartItemCount != 0) {
                    $('#cartIconCount').text(response.cartItemCount);
                    $('#cartIconCount').css('display', 'block');
                } // Assuming cartItemCount is returned from the server
                
            }
        },
        error: function () {
            $('#cartStatus').text("Failed to add to cart.");
        }
    });

    $('#addTocartPage').click(function () {
       

        window.location.href = '/Cart/ViewCart';
    })
    //$('#addToCartBtn').click(function () {
    //    debugger;
    //    const productId = $('#productId').val();
    //    var selectedSizeId = $('input[name="selectedSize"]:checked').val();
    //    if (selectedSizeId == undefined)
    //    {
    //        alert("please select size")
    //        return
    //    }

    //        $.ajax({
    //            url: '/Cart/AddToCartAjax',
    //            method: 'Post',
    //            data: { productId: productId, selectedsize: selectedSizeId },
    //            success: function (response) {
    //                $('#cartStatus').text("Product added to cart!");
    //                if (response.success) {
    //                    debugger;
    //                    $('#cartIconCount').text(response.cartItemCount); // Assuming cartItemCount is returned from the server
    //                    $('#cartIconCount').css('display', 'block');
    //                }
    //            },
    //            error: function () {
    //                $('#cartStatus').text("Failed to add to cart.");
    //            }
    //        });
    //    });


    $('.imageclick').click(function () {
        debugger;
        var productId = $(this).data('id');
        window.location.href = `/Home/Details/${productId}`;

    })
    $('#addToCartBtn').click(function () {
        const productId = $('#productId').val();
        const selectedSizeId = $('input[name="selectedSize"]:checked').val();

        if (!selectedSizeId) {
            alert("Please select a size");
            return;
        }

        const $button = $(this);
        $button.prop('disabled', true); // Disable button to prevent multiple clicks
        $('#cartStatus').text('Adding to cart...');

        $.ajax({
            url: '/Cart/AddToCartAjax',
            method: 'POST',
            data: { productId: productId, selectedsize: selectedSizeId },
            success: function (response) {
                if (response.success) {
                    $('#cartStatus').text(response.message);
                    $('#cartIconCount').text(response.cartItemCount).show(); // Show updated cart count
                } else {
                    $('#cartStatus').text(response.message || "Failed to add to cart.");
                }
            },
            error: function (xhr, status, error) {
                console.error("Add to Cart error:", error);
                $('#cartStatus').text("Something went wrong. Please try again.");
            },
            complete: function () {
                $button.prop('disabled', false); // Re-enable button
                setTimeout(() => {
                    $('#cartStatus').text(""); // Clear status after 3 sec
                }, 3000);
            }
        });
    });


      $('#buyNowBtn').on(function () {
            const productId = $('#productId').val();

            $.ajax({
                url: '/Order/BuyNowAjax',
                method: 'POST',
                data: { productId: productId },
                success: function (response) {
                    // Redirect to order confirmation or payment page
                    window.location.href = `/Order/Details/${response.orderId}`;
                },
                error: function () {
                    alert("Something went wrong. Try again.");
                }
            });
        });
    });
function onImageClick(element) {

        const productId = element.getAttribute('data-product-id');
    const imageUrl = element.getAttribute('data-setbg');

    //console.log("Product clicked:", productId);
    //alert("Product clicked: " + productId);
    
        // You can redirect, open modal, etc.
        window.location.href = `/Home/Details/${productId}`;
}


function updateMainImage(imgElement) {
    
    var mainImage = document.getElementById('mainImage');
    var newSrc = imgElement.getAttribute('data-img');
    mainImage.src = newSrc;

    // Optional: add visual cue to selected image
    document.querySelectorAll('.img-thumbnail').forEach(el => el.classList.remove('border-primary'));
    imgElement.classList.add('border-primary');
}