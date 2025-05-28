// ingredient_stock_check.js
// Real-time ingredient stock checking for Order Modal on Index page

(function() {
    // Helper: collect dish data from the form
    function collectDishes() {
        const dishItems = document.querySelectorAll('#dish-list .dish-item');
        let dishes = [];
        dishItems.forEach(item => {
            const select = item.querySelector('select');
            const qty = item.querySelector('input');
            if (select && qty && select.value && qty.value && parseInt(qty.value) > 0) {
                dishes.push({
                    DishId: select.value,
                    Quantity: parseInt(qty.value)
                });
            }
        });
        return dishes;
    }

    // Helper: show/hide error message
    function showStockError(msg) {
        let errBox = document.getElementById('stock-error-message');
        if (!errBox) return;
        if (msg) {
            errBox.innerHTML = msg;
            errBox.style.display = '';
        } else {
            errBox.innerHTML = '';
            errBox.style.display = 'none';
        }
    }

    // Main: check stock via AJAX
    function checkStockRealtime() {
        const dishes = collectDishes();
        if (dishes.length === 0) {
            showStockError('');
            return;
        }
        fetch('/Orders/CheckStock', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify(dishes)
        })
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                showStockError('');
            } else {
                showStockError(data.message || 'Không đủ nguyên liệu cho một số món.');
            }
        })
        .catch(() => {
            showStockError('Không thể kiểm tra tồn kho.');
        });
    }

    // Attach listeners only on Index page/modal
    document.addEventListener('DOMContentLoaded', function() {
        const dishList = document.getElementById('dish-list');
        if (!dishList) return;
        dishList.addEventListener('change', function(e) {
            if (e.target.classList.contains('dish-select') || e.target.classList.contains('dish-quantity')) {
                checkStockRealtime();
            }
        });
        dishList.addEventListener('input', function(e) {
            if (e.target.classList.contains('dish-quantity')) {
                checkStockRealtime();
            }
        });
        document.getElementById('add-dish')?.addEventListener('click', function() {
            setTimeout(checkStockRealtime, 100); // Wait for DOM update
        });
        // Also check on modal show
        const orderModal = document.getElementById('orderModal');
        if (orderModal) {
            orderModal.addEventListener('show', checkStockRealtime);
        }
    });

    // Prevent submit if stock error
    document.addEventListener('DOMContentLoaded', function() {
        const form = document.getElementById('orderCreateForm');
        if (!form) return;
        form.addEventListener('submit', function(e) {
            const errBox = document.getElementById('stock-error-message');
            if (errBox && errBox.innerHTML && errBox.style.display !== 'none') {
                e.preventDefault();
                errBox.scrollIntoView({behavior: 'smooth', block: 'center'});
            }
        });
    });
})();
