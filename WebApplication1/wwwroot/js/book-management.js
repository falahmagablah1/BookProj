<script>
             // ========================================
    // Enhanced AJAX JavaScript for Book Management System
    // ========================================

    $(document).ready(function() {
        // تهيئة CSRF token لجميع طلبات AJAX
        setupAjaxDefaults();

    // تهيئة وظائف الكتب
    initializeBookFunctions();

    // تهيئة نظام التقييم
    initializeRatingSystem();

    // تهيئة نظام التنزيل
    initializeDownloadSystem();

    // تهيئة فحص التكرار
    initializeDuplicateCheck();

    // تهيئة الفلاتر والبحث
    initializeFiltersAndSearch();
        });

    // ========================================
    // إعداد AJAX الأساسي
    // ========================================
    function setupAjaxDefaults() {
        // إضافة CSRF token لجميع طلبات AJAX
        $.ajaxSetup({
            beforeSend: function (xhr, settings) {
                if (!(/^http:.*/.test(settings.url) || /^https:.*/.test(settings.url))) {
                    // إضافة CSRF token للطلبات المحلية فقط
                    var token = $('input[name="__RequestVerificationToken"]').val();
                    if (token) {
                        xhr.setRequestHeader("RequestVerificationToken", token);
                    }
                }
            },
            error: function (xhr, status, error) {
                console.error('AJAX Error:', {
                    status: status,
                    error: error,
                    responseText: xhr.responseText
                });

                if (xhr.status === 401) {
                    showAlert('Session expired. Please login again.', 'warning');
                    setTimeout(() => {
                        window.location.href = '/Account/Login';
                    }, 2000);
                } else if (xhr.status === 403) {
                    showAlert('Access denied. You do not have permission for this action.', 'danger');
                } else if (xhr.status === 500) {
                    showAlert('Server error occurred. Please try again later.', 'danger');
                } else {
                    showAlert('An unexpected error occurred. Please try again.', 'danger');
                }
            }
        });
        }

    // ========================================
    // وظائف الكتب الأساسية
    // ========================================
    function initializeBookFunctions() {
        let currentItemId = 0;

    // عرض تفاصيل الكتاب
    $(document).on('click', '.btn-view', function(e) {
        e.preventDefault();
    currentItemId = $(this).data('id');
    loadBookDetails(currentItemId);
            });

    // إغلاق Modal
    $('#bookDetailsModal').on('hidden.bs.modal', function() {
        resetBookModal();
            });

    function loadBookDetails(itemId) {
        showModalLoader(true);
    resetBookModal();

    $.ajax({
        url: '/Items/GetItemDetails',
    method: 'GET',
    data: {id: itemId },
    success: function(res) {
        populateBookModal(res);
    initializeModalFunctions(itemId);
    showModalLoader(false);
                    },
    error: function(xhr) {
        showModalLoader(false);
    showAlert('Could not load book details. Please try again.', 'danger');
                    }
                });
            }

    function populateBookModal(res) {
        $('#bookTitle').text(res.name || 'Unknown Title');
    $('#mAuthor').text(res.authorName || 'Unknown Author');
    $('#mCategory').text(res.categoryName || 'Uncategorized');
    $('#mCreated').text(res.createdDate || '');
    $('#modalDownloadCount').text(res.downloadCount || 0);
    $('#avgRating').html('<i class="bi bi-star-fill"></i> ' + (res.averageRating || 0));
    $('#ratingsCount').text(res.totalRatings || 0);

    // إدارة قسم التنزيل
    if (res.hasPdf && res.pdfPath) {
        $('#downloadBlock').show();
                } else {
        $('#downloadBlock').hide();
                }

    // تحميل التقييم السابق للمستخدم
    if (res.userRating) {
        $('#selectedRating').val(res.userRating);
    $('#ratingComment').val(res.userComment || '');
    updateStarDisplay(res.userRating);
                }

                // عرض تقييمات الأدمن
                if (res.ratings && res.ratings.length > 0) {
        displayAdminRatings(res.ratings);
                }
            }

    function displayAdminRatings(ratings) {
        $('#adminRatingsBlock').show();
    $('#ratingsList').empty();

    ratings.forEach(function(rating) {
                    const stars = '★'.repeat(rating.rating) + '☆'.repeat(5 - rating.rating);
    const ratingHtml = `
    <div class="card mb-2">
        <div class="card-body py-2">
            <div class="d-flex justify-content-between">
                <strong>${rating.userName || 'Unknown User'}</strong>
                <span class="text-warning">${stars}</span>
            </div>
            ${rating.comment ? `<p class="mb-0 mt-1 small">${escapeHtml(rating.comment)}</p>` : ''}
            <small class="text-muted">${rating.createdDate}</small>
        </div>
    </div>
    `;
    $('#ratingsList').append(ratingHtml);
                });
            }

    function resetBookModal() {
        $('#selectedRating').val(0);
    $('#ratingComment').val('');
    $('#adminRatingsBlock').hide();
    $('#ratingsList').empty();
    updateStarDisplay(0);
            }

    function showModalLoader(show) {
                if (show) {
        $('#modalLoader').show();
    $('#modalContent').hide();
                } else {
        $('#modalLoader').hide();
    $('#modalContent').show();
                }
            }

    function initializeModalFunctions(itemId) {
        // تهيئة التنزيل
        $('#btnDownloadPdf').off('click').on('click', function () {
            downloadPdf(itemId);
        });

    // تهيئة التقييم
    $('#btnSubmitRating').off('click').on('click', function() {
        submitRating(itemId);
                });
            }
        }

    // ========================================
    // نظام التقييم
    // ========================================
    function initializeRatingSystem() {
        // تهيئة النجوم عند تحميل الصفحة
        initializeStars();
        }

    function initializeStars() {
        $(document).off('click mouseenter mouseleave', '.rating-star');

    $(document).on('click', '.rating-star', function() {
                const rating = parseInt($(this).data('rating'));
    $('#selectedRating').val(rating);
    updateStarDisplay(rating);
            });

    $(document).on('mouseenter', '.rating-star', function() {
                const rating = parseInt($(this).data('rating'));
    updateStarDisplay(rating, true);
            });

    $(document).on('mouseleave', '#ratingStars', function() {
                const selectedRating = parseInt($('#selectedRating').val()) || 0;
    updateStarDisplay(selectedRating);
            });
        }

    function updateStarDisplay(rating, isHover = false) {
        $('.rating-star').each(function (index) {
            const $star = $(this);
            if (index < rating) {
                $star.removeClass('bi-star').addClass('bi-star-fill');
                $star.css('color', '#ffc107');
            } else {
                $star.removeClass('bi-star-fill').addClass('bi-star');
                $star.css('color', '#6c757d');
            }
        });
        }

    function submitRating(itemId) {
            const rating = parseInt($('#selectedRating').val() || '0');
    const comment = $('#ratingComment').val().trim();

    if (!rating || rating < 1 || rating > 5) {
        showAlert('Please select a rating from 1 to 5 stars.', 'warning');
    return;
            }

    const $submitBtn = $('#btnSubmitRating');
    const originalText = $submitBtn.html();

    $submitBtn.prop('disabled', true).html('<i class="bi bi-hourglass-split"></i> Submitting...');

    $.ajax({
        url: '/Items/RateBook',
    method: 'POST',
    data: {
        itemId: itemId,
    rating: rating,
    comment: comment,
    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                },
    success: function(response) {
                    if (response.success) {
        // تحديث العرض
        $('#avgRating').html('<i class="bi bi-star-fill"></i> ' + response.averageRating);
    $('#ratingsCount').text(response.totalRatings);

    // تحديث الجدول الرئيسي
    updateBookRatingInTable(itemId, response.averageRating, response.totalRatings);

    showAlert(response.message, 'success');
                    } else {
        showAlert('Error: ' + (response.message || 'Could not save rating'), 'danger');
                    }
                },
    error: function() {
        showAlert('An error occurred while submitting your rating.', 'danger');
                },
    complete: function() {
        $submitBtn.prop('disabled', false).html(originalText);
                }
            });
        }

    // ========================================
    // نظام التنزيل
    // ========================================
    function initializeDownloadSystem() {
        // تنزيل PDF من الجدول الرئيسي
        window.downloadPdf = function (itemId) {
            performDownload(itemId);
        };
        }

    function performDownload(itemId) {
        $.ajax({
            url: '/Items/DownloadPdf',
            method: 'POST',
            data: {
                itemId: itemId,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (response) {
                if (response.success) {
                    // تحديث العدادات
                    $('#modalDownloadCount').text(response.downloadCount);
                    $(`#download-count-${itemId}`).text(response.downloadCount);

                    // فتح الملف في تبويب جديد
                    window.open(response.downloadUrl, '_blank');
                    showAlert('PDF download started!', 'success');
                } else {
                    showAlert('Error: ' + (response.message || 'Could not download PDF'), 'danger');
                }
            },
            error: function () {
                showAlert('An error occurred while downloading the PDF.', 'danger');
            }
        });
        }

    // ========================================
    // فحص التكرار
    // ========================================
    function initializeDuplicateCheck() {
        let duplicateCheckTimeout;

    // فحص التكرار للكتب الجديدة
    $(document).on('input blur', '#bookNameInput', function() {
        runDuplicateCheck();
            });

    $(document).on('change', '#authorSelect, select[name="AuthorId"]', function() {
        runDuplicateCheck();
            });

    function runDuplicateCheck() {
                const name = $('#bookNameInput').val().trim();
    const authorId = $('#authorSelect, select[name="AuthorId"]').val();
    const itemId = $('input[name="Id"]').val(); // للتعديل

    if (!name || !authorId || authorId === '0') {
        $('#duplicateMsg').addClass('d-none');
    $('#btnSubmit').prop('disabled', false);
    return;
                }

    // إلغاء الطلب السابق
    if (duplicateCheckTimeout) {
        clearTimeout(duplicateCheckTimeout);
                }

                // تأخير الطلب لتجنب الطلبات الكثيرة
                duplicateCheckTimeout = setTimeout(() => {
        $.ajax({
            url: '/Items/CheckDuplicate',
            method: 'POST',
            data: {
                name: name,
                authorId: authorId,
                itemId: itemId,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (response) {
                if (response.exists) {
                    $('#duplicateMsg').removeClass('d-none');
                    $('#btnSubmit').prop('disabled', true);
                } else {
                    $('#duplicateMsg').addClass('d-none');
                    $('#btnSubmit').prop('disabled', false);
                }
            },
            error: function () {
                console.warn('Failed to check for duplicates');
            }
        });
                }, 500);
            }
        }

    // ========================================
    // الفلاتر والبحث
    // ========================================
    function initializeFiltersAndSearch() {
        // البحث في التقييمات
        window.searchRatings = function () {
            const searchTerm = $('#searchInput').val().toLowerCase();
            const rows = $('.rating-row');
            let visibleCount = 0;

            rows.each(function () {
                const $row = $(this);
                const bookName = $row.find('td:eq(0)').text().toLowerCase();
                const authorName = $row.find('td:eq(1)').text().toLowerCase();
                const userName = $row.find('td:eq(3)').text().toLowerCase();

                if (bookName.includes(searchTerm) ||
                    authorName.includes(searchTerm) ||
                    userName.includes(searchTerm)) {
                    $row.show();
                    visibleCount++;
                } else {
                    $row.hide();
                }
            });

            updateRatingCount(visibleCount);
        };

    // فلترة حسب التقييم
    window.filterRatings = function() {
                const selectedRating = $('#ratingFilter').val();
    const rows = $('.rating-row');
    let visibleCount = 0;

    rows.each(function() {
                    const $row = $(this);
    const rating = $row.data('rating').toString();

    if (!selectedRating || rating === selectedRating) {
        $row.show();
    visibleCount++;
                    } else {
        $row.hide();
                    }
                });

    updateRatingCount(visibleCount);
            };

    // ترتيب التقييمات
    window.sortRatings = function() {
                // تنفيذ الترتيب عبر AJAX إذا لزم الأمر
                const sortValue = $('#sortSelect').val();

    $.ajax({
        url: '/Items/ViewRatings',
    method: 'GET',
    data: {sort: sortValue },
    success: function(response) {
        // تحديث الجدول بالبيانات المرتبة
        $('#ratingsTableBody').html($(response).find('#ratingsTableBody').html());
                    },
    error: function() {
        console.warn('Failed to sort ratings');
                    }
                });
            };
        }

    // ========================================
    // وظائف مساعدة
    // ========================================

    // تحديث تقييم الكتاب في الجدول الرئيسي
    function updateBookRatingInTable(bookId, avgRating, totalRatings) {
            const ratingCell = $(`#rating-cell-${bookId}`);

            if (totalRatings > 0) {
        ratingCell.html(`
                    <div>
                        <span class="badge bg-warning text-dark">
                            <i class="bi bi-star-fill"></i> ${avgRating}
                        </span>
                        <br><small class="text-muted">(${totalRatings} reviews)</small>
                    </div>
                `);
            } else {
        ratingCell.html('<span class="badge bg-secondary">No ratings</span>');
            }
        }

    // عرض التنبيهات
    function showAlert(message, type = 'info') {
        // إزالة التنبيهات السابقة
        $('.alert').not('.form-text').remove();

    const alertHtml = `
    <div class="alert alert-${type} alert-dismissible fade show" role="alert">
        ${escapeHtml(message)}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    </div>
    `;

    // إضافة التنبيه
    if ($('#modalContent').is(':visible')) {
        $('#modalContent').prepend(alertHtml);
            } else {
        $('.container').first().prepend(alertHtml);
            }

            // إزالة تلقائية بعد 5 ثواني
            setTimeout(() => {
        $('.alert').alert('close');
            }, 5000);

    // التمرير للأعلى إذا لزم الأمر
    if (!$('#modalContent').is(':visible')) {
        $('html, body').animate({ scrollTop: 0 }, 300);
            }
        }

    // حماية من XSS
    function escapeHtml(text) {
            const map = {
        '&': '&amp;',
    '<': '&lt;',
                '>': '&gt;',
    '"': '&quot;',
    "'": '&#039;'
            };

    return text ? text.replace(/[&<>"']/g, function(m) { return map[m]; }) : '';
        }

        // تحديث عداد التقييمات
        function updateRatingCount(count) {
            const countElement = $('#ratingCount');
        if (countElement.length) {
                const totalCount = count !== undefined ? count : $('.rating-row:visible').length;
        countElement.html('Showing <span class="fw-bold">' + totalCount + '</span> ratings');
            }
        }

        // تصدير البيانات
        window.exportRatingsData = function() {
            $.ajax({
                url: '/Items/ExportRatings',
                method: 'GET',
                success: function (response) {
                    // إنشاء ملف CSV وتنزيله
                    const blob = new Blob([response], { type: 'text/csv' });
                    const url = window.URL.createObjectURL(blob);
                    const a = document.createElement('a');
                    a.href = url;
                    a.download = 'book_ratings_' + new Date().toISOString().slice(0, 10) + '.csv';
                    document.body.appendChild(a);
                    a.click();
                    document.body.removeChild(a);
                    window.URL.revokeObjectURL(url);

                    showAlert('Ratings exported successfully!', 'success');
                },
                error: function () {
                    showAlert('Failed to export ratings', 'danger');
                }
            });
        };

        // ========================================
        // معالجة أخطاء خاصة
        // ========================================

        // معالجة انتهاء الجلسة
        $(document).ajaxError(function(event, xhr, settings) {
            if (xhr.status === 419) { // CSRF token mismatch
            showAlert('Security token expired. Please refresh the page.', 'warning');
                setTimeout(() => {
            location.reload();
                }, 3000);
            }
        });

        // تحديث CSRF token عند انتهاء صلاحيته
        function refreshCSRFToken() {
            $.get('/Home/GetCSRFToken')
                .done(function (token) {
                    $('input[name="__RequestVerificationToken"]').val(token);
                })
                .fail(function () {
                    location.reload();
                });
        }
    </script>