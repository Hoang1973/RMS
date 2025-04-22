// order.js - Script for Order Management Modal and Payment UI
// Refactored for clarity and maintainability

(function () {
    // Helper: Format VND currency
    function formatVND(amount) {
        return parseInt(amount).toLocaleString('vi-VN') + ' VND';
    }

    // Helper: Render dish list HTML
    function renderDishes(dishes) {
        if (!dishes || dishes.length === 0) {
            return `<div class='text-gray-400 italic text-center py-4'>Không có món ăn nào.</div>`;
        }
        return dishes.map(d => `
            <div class='py-2 border-b last:border-b-0'>
                <div class='font-semibold text-base mb-1'>${d.name}</div>
                <div class='text-gray-500 mb-1'>${formatVND(d.price)} x ${d.quantity}</div>
                ${d.note ? `<div class='italic text-xs text-gray-400 mb-1'>Ghi chú: ${d.note}</div>` : ''}
                <div class='font-bold text-right text-base'>${formatVND(d.subtotal)}</div>
            </div>`).join('');
    }

    // Helper: Show toast/notification
    function showToast(msg, type) {
        // You can replace this with your real notification system
        alert(msg);
    }

    // Render Order Detail Modal
    window.showOrderDetail = function (orderId) {
        removeOrderModal();
        const modal = document.createElement('div');
        modal.id = 'order-detail-modal';
        modal.className = 'fixed inset-0 bg-black bg-opacity-40 flex items-center justify-center z-50';
        modal.innerHTML = `
            <div class="bg-white rounded-lg shadow-lg w-full max-w-xl min-w-[350px] p-6 sm:p-8 relative animate-fadein flex flex-col">
                <button onclick="document.getElementById('order-detail-modal').remove()" class="absolute top-3 right-4 text-gray-400 hover:text-gray-600 text-3xl">&times;</button>
                <div id="order-detail-content" class="flex flex-col w-full min-h-[160px] justify-center">
                    <span class="text-gray-400 self-center">Đang tải chi tiết đơn hàng...</span>
                </div>
            </div>
        `;
        document.body.appendChild(modal);

        fetch(`/Orders/DetailsJson/${orderId}`)
            .then(res => {
                if (!res.ok) throw new Error('Not found');
                return res.json();
            })
            .then(order => {
                renderOrderDetailContent(order);
                window._lastOrderDetail = order;
            })
            .catch(() => {
                document.getElementById('order-detail-content').innerHTML = `<span class='text-red-500'>Không tìm thấy đơn hàng hoặc lỗi server.</span>`;
            });
    };

    // Remove modal if exists
    function removeOrderModal() {
        let oldModal = document.getElementById('order-detail-modal');
        if (oldModal) oldModal.remove();
    }

    // Render order detail content
    function renderOrderDetailContent(order) {
        const statusColor = order.status === 'Pending' ? 'bg-yellow-100 text-yellow-800' : (order.status === 'Completed' ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-600');
        const statusText = order.status === 'Pending' ? 'Đang phục vụ' : (order.status === 'Completed' ? 'Đã thanh toán' : 'Đã hủy');
        const content = document.getElementById('order-detail-content');
        content.innerHTML = `
            <div class="flex flex-col space-y-6">
                <div>
                    <div class="font-bold text-2xl text-gray-900 mb-1">Đơn hàng - <span class="text-primary">${order.tableNumber ?? '-'}</span></div>
                    <div class="flex flex-col space-y-1 text-sm text-gray-600">
                        <div><span class="font-semibold">Bàn:</span> <span class="text-blue-600 font-bold">${order.tableNumber ?? '-'}</span></div>
                        <div><span class="font-semibold">Thời gian tạo:</span> <span>${order.createdAt ?? '-'}</span></div>
                        <div><span class="font-semibold">Thời gian phục vụ:</span> <span>${order.serveTime ?? '-'}</span></div>
                        <div><span class="font-semibold">Trạng thái:</span> <span class="inline-block px-2 py-1 rounded ${statusColor}">${statusText}</span></div>
                    </div>
                </div>
                <div>
                    <div class="font-semibold mb-2 text-base">Danh sách món</div>
                    <div class="divide-y divide-gray-100">${renderDishes(order.dishes)}</div>
                </div>
                <div class="flex justify-end">
                    <span class="font-bold text-xl text-green-700">Tổng tiền: ${formatVND(order.totalAmount)}</span>
                </div>
                <div class="flex justify-end">
                    <button onclick="document.getElementById('order-detail-modal').remove()" class="px-5 py-2 bg-gray-200 hover:bg-gray-300 rounded text-gray-700 font-semibold">Đóng</button>
                    ${order.status === 'Pending' ? `<button id="start-payment-btn" class="ml-3 px-5 py-2 bg-blue-600 hover:bg-blue-700 rounded text-white font-semibold">Thanh toán</button>` : ''}
                </div>
            </div>
        `;
    }

    // Payment Panel Logic
    document.addEventListener('click', function (e) {
        if (e.target && e.target.id === 'start-payment-btn') {
            const order = window._lastOrderDetail;
            if (!order) return;
            renderPaymentPanel(order);
        }
    });

    function renderPaymentPanel(order) {
        const content = document.getElementById('order-detail-content');
        let subtotal = order.dishes.reduce((sum, d) => sum + (parseInt(d.subtotal)||0), 0);
        let vat = Math.round(subtotal * 0.08);
        let discountType = 'percent';
        let discountValue = 0;
        let paymentMethod = '';
        let discountAmount = 0;
        let total = subtotal + vat;

        content.innerHTML = `
            <div class="flex flex-col space-y-6">
                <div>
                    <div class="font-bold text-2xl text-gray-900 mb-1">Thanh toán đơn hàng - <span class="text-primary">${order.tableNumber ?? '-'}</span></div>
                    <div class="flex flex-col space-y-1 text-sm text-gray-600 mb-4">
                        <div><span class="font-semibold">Bàn:</span> <span class="text-blue-600 font-bold">${order.tableNumber ?? '-'}</span></div>
                        <div><span class="font-semibold">Thời gian tạo:</span> <span>${order.createdAt ?? '-'}</span></div>
                    </div>
                    <div class="overflow-y-auto max-h-64 mb-4">
                        <table class="w-full">
                            <thead class="border-b">
                                <tr>
                                    <th class="py-2 text-left">Món</th>
                                    <th class="py-2 text-right">SL</th>
                                    <th class="py-2 text-right">Đơn giá</th>
                                    <th class="py-2 text-right">Thành tiền</th>
                                </tr>
                            </thead>
                            <tbody>
                                ${order.dishes.map(d => `
                                    <tr>
                                        <td class="py-1">${d.name}</td>
                                        <td class="py-1 text-right">${d.quantity}</td>
                                        <td class="py-1 text-right">${formatVND(d.price)}</td>
                                        <td class="py-1 text-right">${formatVND(d.subtotal)}</td>
                                    </tr>
                                `).join('')}
                            </tbody>
                        </table>
                    </div>
                    <div class="space-y-2 border-t pt-3 mb-4">
                        <div class="flex justify-between">
                            <span>Tổng tiền hàng:</span>
                            <span id="payment-subtotal">${formatVND(subtotal)}</span>
                        </div>
                        <div class="flex justify-between">
                            <span>Thuế VAT (8%):</span>
                            <span id="payment-tax">${formatVND(vat)}</span>
                        </div>
                        <div class="flex justify-between items-center">
                            <div class="flex items-center">
                                <span>Giảm giá:</span>
                                <select id="discount-type" class="ml-2 border p-1 rounded">
                                    <option value="percent">%</option>
                                    <option value="amount">VND</option>
                                </select>
                            </div>
                            <input type="number" id="discount-value" class="w-24 border rounded p-1 text-right text-base" value="0" min="0">
                        </div>
                        <div class="flex justify-between font-bold text-lg pt-2 border-t">
                            <span>Tổng thanh toán:</span>
                            <span id="payment-total">${formatVND(total)}</span>
                        </div>
                    </div>
                    <div>
                        <h4 class="font-medium mb-2">Phương thức thanh toán</h4>
                        <div class="grid grid-cols-2 gap-2 mb-4">
                            <button type="button" class="payment-method border p-2 rounded flex items-center justify-center" data-method="cash">
                                <i class="fas fa-money-bill-wave mr-2"></i>
                                <span>Tiền mặt</span>
                            </button>
                            <button type="button" class="payment-method border p-2 rounded flex items-center justify-center" data-method="momo">
                                <i class="fas fa-mobile-alt mr-2"></i>
                                <span>Ví MoMo</span>
                            </button>
                            <button type="button" class="payment-method border p-2 rounded flex items-center justify-center" data-method="zalopay">
                                <i class="fas fa-wallet mr-2"></i>
                                <span>ZaloPay</span>
                            </button>
                            <button type="button" class="payment-method border p-2 rounded flex items-center justify-center" data-method="bank">
                                <i class="fas fa-credit-card mr-2"></i>
                                <span>Chuyển khoản</span>
                            </button>
                        </div>
                        <button id="complete-payment-btn" class="w-full bg-primary hover:bg-secondary text-white py-3 rounded font-medium" disabled>
                            Hoàn tất thanh toán
                        </button>
                    </div>
                    <div class="flex justify-end mt-6">
                        <button onclick="document.getElementById('order-detail-modal').remove()" class="px-5 py-2 bg-gray-200 hover:bg-gray-300 rounded text-gray-700 font-semibold">Đóng</button>
                    </div>
                </div>
            </div>
        `;

        // Discount logic
        const discountTypeEl = document.getElementById('discount-type');
        const discountValueEl = document.getElementById('discount-value');
        const subtotalEl = document.getElementById('payment-subtotal');
        const vatEl = document.getElementById('payment-tax');
        const totalEl = document.getElementById('payment-total');

        function updatePaymentSummary() {
            discountType = discountTypeEl.value;
            discountValue = parseInt(discountValueEl.value) || 0;
            if (discountType === 'percent') {
                discountAmount = Math.round((subtotal + vat) * discountValue / 100);
            } else {
                discountAmount = discountValue;
            }
            total = subtotal + vat - discountAmount;
            if (total < 0) total = 0;
            subtotalEl.textContent = formatVND(subtotal);
            vatEl.textContent = formatVND(vat);
            totalEl.textContent = formatVND(total);
        }
        discountTypeEl.addEventListener('change', updatePaymentSummary);
        discountValueEl.addEventListener('input', updatePaymentSummary);

        // Payment method selection
        let selectedMethodBtn = null;
        document.querySelectorAll('.payment-method').forEach(btn => {
            btn.addEventListener('click', function () {
                document.querySelectorAll('.payment-method').forEach(b => b.classList.remove('bg-blue-100', 'border-blue-500'));
                this.classList.add('bg-blue-100', 'border-blue-500');
                paymentMethod = this.dataset.method;
                document.getElementById('complete-payment-btn').disabled = false;
                selectedMethodBtn = this;
            });
        });

        // Complete payment
        document.getElementById('complete-payment-btn').addEventListener('click', function () {
            const btn = this;
            btn.disabled = true;
            btn.textContent = 'Đang xử lý...';
            fetch(`/Orders/CompletePayment?orderId=${order.id}&tableId=${order.tableId}`, {
                method: "POST"
            })
            .then(response => {
                // If server returns a redirect (default for ASP.NET MVC), treat as success
                if (response.redirected || response.ok) {
                    showToast('Thanh toán thành công!', 'success');
                    removeOrderModal();
                    setTimeout(() => window.location.reload(), 700);
                } else {
                    showToast('Lỗi khi thanh toán, vui lòng thử lại.', 'error');
                    btn.disabled = false;
                    btn.textContent = 'Hoàn tất thanh toán';
                }
            })
            .catch(() => {
                console.error('Lỗi fetch:', err);
                showToast('Có lỗi xảy ra, vui lòng thử lại.', 'error');
                btn.disabled = false;
                btn.textContent = 'Hoàn tất thanh toán';
            });
        });

        // Initial summary
        updatePaymentSummary();
    }

})();
