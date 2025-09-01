// Author.js - Fixed Ajax Implementation for Authors Management

class AuthorsManager {
    constructor() {
        this.authorsData = [];
        this.canEditUser = false;
        this.canDeleteUser = false;
        this.featuresEnabled = false;
        this.originalTableContent = '';
        this.baseUrl = '';
        this.antiForgeryToken = '';

        this.init();
    }

    // تهيئة الكلاس
    init() {
        // انتظار تحميل DOM
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => {
                this.setupAfterDOMReady();
            });
        } else {
            this.setupAfterDOMReady();
        }
    }

    setupAfterDOMReady() {
        this.originalTableContent = document.getElementById('authorsTableBody')?.innerHTML || '';
        this.setupEventListeners();
        this.initializeCustomStyles();
        this.loadAntiForgeryToken();
        console.log('Authors Manager initialized');
    }

    // تحميل Anti-Forgery Token
    loadAntiForgeryToken() {
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        if (tokenInput) {
            this.antiForgeryToken = tokenInput.value;
        }
    }

    // إعداد Event Listeners المُصحح
    setupEventListeners() {
        // البحث المباشر
        const searchInput = document.getElementById('searchInput');
        if (searchInput) {
            searchInput.removeEventListener('input', this.searchHandler);
            this.searchHandler = this.debounce(() => this.searchAuthors(), 300);
            searchInput.addEventListener('input', this.searchHandler);
        }

        // الترتيب
        const sortSelect = document.getElementById('sortSelect');
        if (sortSelect) {
            sortSelect.removeEventListener('change', this.sortHandler);
            this.sortHandler = () => this.sortAuthors();
            sortSelect.addEventListener('change', this.sortHandler);
        }

        // زر الميزات المتقدمة
        const toggleBtn = document.getElementById('toggleFeaturesBtn');
        if (toggleBtn) {
            toggleBtn.removeEventListener('click', this.toggleHandler);
            this.toggleHandler = () => this.toggleAdvancedFeatures();
            toggleBtn.addEventListener('click', this.toggleHandler);
        }

        // أزرار الفلتر
        this.setupFilterButtons();
    }

    // إعداد أزرار الفلتر
    setupFilterButtons() {
        const filterButtons = document.querySelectorAll('.btn-group .btn');
        filterButtons.forEach((btn, index) => {
            btn.removeEventListener('click', btn.filterHandler);
            btn.filterHandler = () => {
                switch (index) {
                    case 0: this.showAllAuthors(); break;
                    case 1: this.showAuthorsWithBooks(); break;
                    case 2: this.showAuthorsWithoutBooks(); break;
                }
            };
            btn.addEventListener('click', btn.filterHandler);
        });
    }

    // تبديل الميزات المتقدمة
    toggleAdvancedFeatures() {
        const featuresDiv = document.getElementById('advancedFeatures');
        const toggleBtn = document.getElementById('toggleFeaturesBtn');

        if (!featuresDiv || !toggleBtn) return;

        if (this.featuresEnabled) {
            featuresDiv.style.display = 'none';
            toggleBtn.innerHTML = '<i class="bi bi-sliders"></i> Enable Advanced Features';
            toggleBtn.className = 'btn btn-outline-primary';
            this.resetTableToOriginal();
            this.featuresEnabled = false;
        } else {
            featuresDiv.style.display = 'block';
            toggleBtn.innerHTML = '<i class="bi bi-x-circle"></i> Disable Advanced Features';
            toggleBtn.className = 'btn btn-outline-danger';
            this.featuresEnabled = true;
            this.updateAuthorCount();

            // إعادة تهيئة Event Listeners للعناصر الجديدة
            this.setupEventListeners();
        }
    }

    // إعادة تعيين الجدول للحالة الأصلية
    resetTableToOriginal() {
        const tableBody = document.getElementById('authorsTableBody');
        if (tableBody && this.originalTableContent) {
            tableBody.innerHTML = this.originalTableContent;
        }

        const searchInput = document.getElementById('searchInput');
        const sortSelect = document.getElementById('sortSelect');

        if (searchInput) searchInput.value = '';
        if (sortSelect) sortSelect.value = 'name-asc';

        this.setActiveButton(0);
    }

    // عرض جميع المؤلفين
    async showAllAuthors() {
        if (!this.featuresEnabled) return;

        try {
            this.showLoading(true);
            this.setActiveButton(0);

            const response = await this.makeAjaxRequest('/Author/GetAllAuthors', 'GET');

            if (response.success) {
                this.renderAuthorsTable(response.data);
                this.updateAuthorCount(response.data.length);
            } else {
                this.showNotification('Error loading authors: ' + response.message, 'error');
            }
        } catch (error) {
            console.error('Error loading all authors:', error);
            this.showNotification('Error loading authors', 'error');
        } finally {
            this.showLoading(false);
        }
    }

    // عرض المؤلفين مع كتب
    async showAuthorsWithBooks() {
        if (!this.featuresEnabled) return;

        try {
            this.showLoading(true);
            this.setActiveButton(1);

            const response = await this.makeAjaxRequest('/Author/GetAuthorsWithBooks', 'GET');

            if (response.success) {
                this.renderAuthorsTable(response.data);
                this.updateAuthorCount(response.data.length);
            } else {
                this.showNotification('Error loading authors with books: ' + response.message, 'error');
            }
        } catch (error) {
            console.error('Error loading authors with books:', error);
            this.showNotification('Error loading authors with books', 'error');
        } finally {
            this.showLoading(false);
        }
    }

    // عرض المؤلفين بدون كتب
    async showAuthorsWithoutBooks() {
        if (!this.featuresEnabled) return;

        try {
            this.showLoading(true);
            this.setActiveButton(2);

            const response = await this.makeAjaxRequest('/Author/GetAuthorsWithoutBooks', 'GET');

            if (response.success) {
                this.renderAuthorsTable(response.data);
                this.updateAuthorCount(response.data.length);
            } else {
                this.showNotification('Error loading authors without books: ' + response.message, 'error');
            }
        } catch (error) {
            console.error('Error loading authors without books:', error);
            this.showNotification('Error loading authors without books', 'error');
        } finally {
            this.showLoading(false);
        }
    }

    // البحث في المؤلفين - مُصحح
    async searchAuthors() {
        if (!this.featuresEnabled) return;

        const searchTerm = document.getElementById('searchInput')?.value?.trim() || '';

        if (searchTerm.length === 0) {
            this.showAllAuthors();
            return;
        }

        try {
            this.showLoading(true);

            const response = await this.makeAjaxRequest('/Author/SearchAuthors', 'POST', {
                searchTerm: searchTerm
            });

            if (response.success) {
                this.renderAuthorsTable(response.data);
                this.updateAuthorCount(response.data.length);
            } else {
                this.showNotification('Search failed: ' + response.message, 'error');
            }
        } catch (error) {
            console.error('Search error:', error);
            this.showNotification('Search failed', 'error');
        } finally {
            this.showLoading(false);
        }
    }

    // ترتيب المؤلفين - مُصحح
    async sortAuthors() {
        if (!this.featuresEnabled) return;

        const sortValue = document.getElementById('sortSelect')?.value;
        if (!sortValue) return;

        try {
            this.showLoading(true);

            const response = await this.makeAjaxRequest('/Author/SortAuthors', 'POST', {
                sortBy: sortValue
            });

            if (response.success) {
                this.renderAuthorsTable(response.data);
                this.updateAuthorCount(response.data.length);
            } else {
                this.showNotification('Sort failed: ' + response.message, 'error');
            }
        } catch (error) {
            console.error('Sort error:', error);
            this.showNotification('Sort failed', 'error');
        } finally {
            this.showLoading(false);
        }
    }

    // تحديد الزر النشط
    setActiveButton(index) {
        const buttons = document.querySelectorAll('.btn-group .btn');
        buttons.forEach(btn => btn.classList.remove('active'));

        if (buttons[index]) {
            buttons[index].classList.add('active');
        }
    }

    // رسم الجدول
    renderAuthorsTable(authors) {
        if (!this.featuresEnabled) return;

        const tbody = document.getElementById('authorsTableBody');
        if (!tbody) return;

        tbody.innerHTML = '';

        if (!authors || authors.length === 0) {
            tbody.innerHTML = `
                <tr>
                    <td colspan="6" class="text-center text-muted py-4">
                        <i class="bi bi-person-x display-4"></i>
                        <p class="mt-2">No authors found</p>
                    </td>
                </tr>
            `;
            return;
        }

        authors.forEach(author => {
            const row = this.createAuthorRow(author);
            tbody.insertAdjacentHTML('beforeend', row);
        });
    }

    // إنشاء صف المؤلف
    createAuthorRow(author) {
        const editButton = this.canEditUser ?
            `<a href="${this.baseUrl}/Author/Edit/${author.Id}" class="btn btn-sm btn-success" title="Edit">
                <i class="bi bi-pencil"></i>
            </a>` : '';

        const deleteButton = this.canDeleteUser ?
            `<button type="button" class="btn btn-sm btn-danger" onclick="authorsManager.deleteAuthor(${author.Id})" title="Delete">
                <i class="bi bi-trash"></i>
            </button>` : '';

        return `
            <tr data-author-id="${author.Id}" class="author-row" data-books-count="${author.BooksCount || 0}">
                <td class="author-name">
                    <strong>${this.escapeHtml(author.AuthorName || '')}</strong>
                </td>
                <td class="author-email">${this.escapeHtml(author.AuthorEmail || '')}</td>
                <td class="author-phone d-none d-md-table-cell">${this.escapeHtml(author.AuthorPhone || '')}</td>
                <td class="author-age d-none d-lg-table-cell">${author.AuthorAge || ''}</td>
                <td class="books-count">
                    ${(author.BooksCount || 0) > 0 ?
                `<span class="badge bg-success">${author.BooksCount}</span>` :
                `<span class="badge bg-secondary">0</span>`
            }
                </td>
                <td>
                    <button type="button" class="btn btn-sm btn-primary" onclick="authorsManager.showAuthorDetailsModal(${author.Id})" title="View Details">
                        <i class="bi bi-eye"></i>
                    </button>
                    ${editButton}
                    ${deleteButton}
                </td>
            </tr>
        `;
    }

    // عرض تفاصيل المؤلف في Modal
    async showAuthorDetailsModal(authorId) {
        try {
            this.showLoadingModal();

            const response = await this.makeAjaxRequest(`/Author/GetAuthorDetails/${authorId}`, 'GET');

            if (response.success) {
                this.renderAuthorDetailsModal(response.data);
            } else {
                this.showNotification('Author not found: ' + response.message, 'error');
            }
        } catch (error) {
            console.error('Error loading author details:', error);
            this.showNotification('Error loading author details', 'error');
        }
    }

    // رسم محتوى Modal التفاصيل
    renderAuthorDetailsModal(author) {
        let booksHtml = '';

        if (author.Books && author.Books.length > 0) {
            booksHtml = '<div class="row">';
            author.Books.forEach(book => {
                if (book.Name) {
                    booksHtml += `
                        <div class="col-md-6 mb-3">
                            <div class="card">
                                <div class="card-body">
                                    <h6 class="card-title">
                                        <i class="bi bi-book-fill text-primary"></i> ${this.escapeHtml(book.Name)}
                                    </h6>
                                    <p class="card-text">
                                        <small class="text-muted">Category: ${this.escapeHtml(book.CategoryName || 'Uncategorized')}</small><br>
                                        <small class="text-muted">Created: ${book.TimeCreated || 'Unknown'}</small>
                                    </p>
                                </div>
                            </div>
                        </div>
                    `;
                }
            });
            booksHtml += '</div>';
        } else {
            booksHtml = `
                <div class="alert alert-info text-center">
                    <i class="bi bi-info-circle"></i> This author has not published any books yet.
                </div>
            `;
        }

        const modalContent = `
            <div class="container-fluid">
                <div class="row mb-4">
                    <div class="col-md-4">
                        <div class="card border-primary">
                            <div class="card-header bg-primary text-white">
                                <h6 class="mb-0"><i class="bi bi-person-fill"></i> Personal Information</h6>
                            </div>
                            <div class="card-body">
                                <table class="table table-borderless">
                                    <tr><td><strong>Name:</strong></td><td>${this.escapeHtml(author.AuthorName || 'N/A')}</td></tr>
                                    <tr><td><strong>Email:</strong></td><td>${this.escapeHtml(author.AuthorEmail || 'Not provided')}</td></tr>
                                    <tr><td><strong>Phone:</strong></td><td>${this.escapeHtml(author.AuthorPhone || 'Not provided')}</td></tr>
                                    <tr><td><strong>Age:</strong></td><td>${author.AuthorAge || 'N/A'} years</td></tr>
                                </table>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-8">
                        <div class="card border-success">
                            <div class="card-header bg-success text-white">
                                <h6 class="mb-0">
                                    <i class="bi bi-journals"></i> Published Books 
                                    <span class="badge bg-light text-dark ms-2">${author.BooksCount || 0}</span>
                                </h6>
                            </div>
                            <div class="card-body">
                                ${booksHtml}
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `;

        document.getElementById('authorDetailsContent').innerHTML = modalContent;

        const modal = new bootstrap.Modal(document.getElementById('authorDetailsModal'));
        modal.show();
    }

    // حذف مؤلف
    async deleteAuthor(authorId) {
        const confirmMessage = 'Are you sure you want to delete this author? This action cannot be undone.';

        if (!confirm(confirmMessage)) {
            return;
        }

        try {
            this.showLoading(true);

            const response = await this.makeAjaxRequest(`/Author/DeleteAjax/${authorId}`, 'DELETE');

            if (response.success) {
                this.showNotification(response.message || 'Author deleted successfully', 'success');

                // إعادة تحميل البيانات
                if (this.featuresEnabled) {
                    await this.showAllAuthors();
                } else {
                    // إزالة الصف من الجدول الأصلي
                    const row = document.querySelector(`tr[data-author-id="${authorId}"]`);
                    if (row) {
                        row.remove();
                    }
                }
            } else {
                this.showNotification(response.message || 'Delete failed', 'error');
            }
        } catch (error) {
            console.error('Delete error:', error);
            this.showNotification('Delete failed', 'error');
        } finally {
            this.showLoading(false);
        }
    }

    // تحديث عداد المؤلفين
    updateAuthorCount(count) {
        if (!this.featuresEnabled) return;

        const countElement = document.getElementById('authorCount');
        if (!countElement) return;

        const totalCount = count !== undefined ? count :
            document.querySelectorAll('.author-row:not([style*="display: none"])').length;

        countElement.innerHTML = `Total Authors: <span class="fw-bold">${totalCount}</span>`;
    }

    // عرض Loading
    showLoading(show) {
        const spinner = document.getElementById('loadingSpinner');
        if (spinner) {
            spinner.style.display = show ? 'block' : 'none';
        }
    }

    // عرض Loading Modal
    showLoadingModal() {
        const content = `
            <div class="text-center p-4">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
                <p class="mt-2">Loading author details...</p>
            </div>
        `;

        document.getElementById('authorDetailsContent').innerHTML = content;

        const modal = new bootstrap.Modal(document.getElementById('authorDetailsModal'));
        modal.show();
    }

    // عرض الإشعارات
    showNotification(message, type = 'info') {
        const alertClass = type === 'success' ? 'alert-success' :
            type === 'error' ? 'alert-danger' : 'alert-info';

        const alertHtml = `
            <div class="alert ${alertClass} alert-dismissible fade show" role="alert">
                ${this.escapeHtml(message)}
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>
        `;

        // إزالة الإشعارات السابقة
        document.querySelectorAll('.alert').forEach(alert => alert.remove());

        // البحث عن container للإشعارات
        const container = document.querySelector('.container.p-3') || document.body;
        container.insertAdjacentHTML('afterbegin', alertHtml);

        // إزالة الإشعار تلقائياً بعد 5 ثواني
        setTimeout(() => {
            const alert = container.querySelector('.alert');
            if (alert) {
                alert.remove();
            }
        }, 5000);
    }

    // تهيئة التأثيرات البصرية
    initializeCustomStyles() {
        if (document.getElementById('authorsCustomStyles')) return;

        const style = document.createElement('style');
        style.id = 'authorsCustomStyles';
        style.textContent = `
            #advancedFeatures { transition: all 0.3s ease; }
            #toggleFeaturesBtn { transition: all 0.2s ease; }
            #toggleFeaturesBtn:hover { transform: translateY(-1px); box-shadow: 0 2px 4px rgba(0,0,0,0.2); }
            .table tbody tr { transition: background-color 0.2s ease; }
            .table tbody tr:hover { background-color: rgba(0,123,255,0.1) !important; }
            .author-row { cursor: pointer; }
            .btn { transition: all 0.2s ease; }
            .btn:hover { transform: translateY(-1px); box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
            .modal-content { border: none; box-shadow: 0 10px 30px rgba(0,0,0,0.2); border-radius: 10px; }
            .modal-header { border-radius: 10px 10px 0 0; }
            .card { transition: transform 0.2s ease; }
            .card:hover { transform: translateY(-2px); }
            .spinner-border { width: 3rem; height: 3rem; }
            #loadingSpinner { z-index: 1000; }
        `;

        document.head.appendChild(style);
    }

    // إجراء طلب Ajax مُصحح
    async makeAjaxRequest(url, method = 'GET', data = null) {
        const options = {
            method: method,
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            }
        };

        // إضافة Anti-Forgery Token للطلبات POST/PUT/DELETE
        if (method !== 'GET') {
            if (this.antiForgeryToken) {
                options.headers['RequestVerificationToken'] = this.antiForgeryToken;
            }

            // إضافة البيانات
            if (data) {
                options.body = JSON.stringify(data);
            }
        }

        try {
            const response = await fetch(url, options);

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const contentType = response.headers.get('content-type');
            if (contentType && contentType.includes('application/json')) {
                return await response.json();
            } else {
                const text = await response.text();
                throw new Error('Response is not JSON: ' + text);
            }
        } catch (error) {
            console.error('AJAX Request failed:', error);
            throw error;
        }
    }

    // تأخير تنفيذ الدالة (Debounce)
    debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func.apply(this, args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }

    // تأمين النص من XSS
    escapeHtml(text) {
        if (!text) return '';
        const map = {
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#039;'
        };
        return String(text).replace(/[&<>"']/g, m => map[m]);
    }
}

// إنشاء instance عام للكلاس
let authorsManager;

// تهيئة المدير عند تحميل الصفحة
document.addEventListener('DOMContentLoaded', function () {
    authorsManager = new AuthorsManager();
});

// دوال عامة للاستخدام من HTML (backward compatibility)
function toggleAdvancedFeatures() {
    if (authorsManager) authorsManager.toggleAdvancedFeatures();
}

function showAllAuthors() {
    if (authorsManager) authorsManager.showAllAuthors();
}

function showAuthorsWithBooks() {
    if (authorsManager) authorsManager.showAuthorsWithBooks();
}

function showAuthorsWithoutBooks() {
    if (authorsManager) authorsManager.showAuthorsWithoutBooks();
}

function searchAuthors() {
    if (authorsManager) authorsManager.searchAuthors();
}

function sortAuthors() {
    if (authorsManager) authorsManager.sortAuthors();
}

function showAuthorDetailsModal(authorId) {
    if (authorsManager) authorsManager.showAuthorDetailsModal(authorId);
}