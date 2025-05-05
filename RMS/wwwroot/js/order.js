// order.js - Quản lý modal thanh toán, tối giản, không logic thừa

// Discount logic
const discountTypeEl = document.getElementById('discount-type');
const discountValueEl = document.getElementById('discount-value');
const subtotalEl = document.getElementById('payment-subtotal');
const vatEl = document.getElementById('payment-tax');
const totalEl = document.getElementById('payment-total');

// Định dạng tiền VND
function formatVND(amount) {
    return parseInt(amount).toLocaleString('vi-VN') + ' ₫';
}
function printOrderBill(orderId) {
    var billContent = document.getElementById('order-bill-' + orderId).innerHTML;
    var printWindow = window.open('', '', 'width=800,height=600');
    printWindow.document.write('<html><head><title>In hóa đơn</title>');
    printWindow.document.write('<link href="https://cdn.tailwindcss.com" rel="stylesheet" />');
    printWindow.document.write('<style>body{background:white;}</style>');
    printWindow.document.write('</head><body >');
    printWindow.document.write(billContent);
    printWindow.document.write('</body></html>');
    printWindow.document.close();
    printWindow.focus();
    setTimeout(function () { printWindow.print(); printWindow.close(); }, 500);
}
function showOrderDetail(orderId) {
    // Xóa modal cũ nếu có
    let oldModal = document.getElementById('order-detail-modal');
    if (oldModal) oldModal.remove();
    // Tạo modal mới
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
    // Fetch chi tiết đơn hàng
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
}

// Hàm render chi tiết đơn hàng (panel đầu tiên trong modal)
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
                <div class="divide-y divide-gray-100">${typeof renderDishes === 'function' ? renderDishes(order.dishes) : ''}</div>
            </div>
            <div class="flex justify-end">
                <span class="font-bold text-xl text-green-700">Tổng tiền: ${formatVND(order.totalAmount)}</span>
            </div>
            <div class="flex justify-end">
                <button onclick="document.getElementById('order-detail-modal').remove()" class="px-5 py-2 bg-gray-200 hover:bg-gray-300 rounded text-gray-700 font-semibold">Đóng</button>
                ${order.status === 'Pending' ? `<button id=\"start-payment-btn\" class=\"ml-3 px-5 py-2 bg-blue-600 hover:bg-blue-700 rounded text-white font-semibold\">Thanh toán</button>` : ''}
            </div>
        </div>
    `;
}

// Hàm render panel thanh toán
function renderPaymentPanel(order) {
    const modal = document.getElementById('order-detail-modal');
    if (!modal) return;
    modal.innerHTML = `
        <div class="bg-white rounded-lg shadow-lg w-full max-w-xl min-w-[350px] p-6 sm:p-8 relative animate-fadein flex flex-col">
            <button onclick="document.getElementById('order-detail-modal').remove()" class="absolute top-3 right-4 text-gray-400 hover:text-gray-600 text-3xl">&times;</button>
            <div id="payment-detail-panel"></div>
        </div>
    `;
    renderPaymentDetailPanel(order);
}

// Lắng nghe click nút "Thanh toán" để chuyển sang panel thanh toán
if (!window._orderDetailPaymentListener) {
    document.addEventListener('click', function (e) {
        if (e.target && e.target.id === 'start-payment-btn') {
            const order = window._lastOrderDetail;
            if (!order) return;
            renderPaymentPanel(order);
        }
    });
    window._orderDetailPaymentListener = true;
}


// Render nội dung chi tiết thanh toán
function renderPaymentDetailPanel(order) {
    const content = document.getElementById('payment-detail-panel');
    const subtotal = order.totalAmount;
    const vat = Math.round(subtotal * 0.08);
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
                            <i class="fas fa-qrcode mr-2"></i>
                            <span>ZaloPay</span>
                        </button>
                        <button type="button" class="payment-method border p-2 rounded flex items-center justify-center" data-method="bank">
                            <i class="fas fa-university mr-2"></i>
                            <span>Chuyển khoản</span>
                        </button>
                    </div>
                    <button id="complete-payment-btn" class="w-full py-2 bg-green-500 hover:bg-green-600 text-white font-bold rounded disabled:opacity-60" disabled>Hoàn tất thanh toán</button>
                    <button onclick="document.getElementById('order-detail-modal').remove()" class="w-full mt-2 py-2 bg-gray-200 hover:bg-gray-300 rounded text-gray-700 font-semibold">Đóng</button>
                </div>
            </div>
        </div>
    `;

    function updatePaymentSummary() {
        let discountType = discountTypeEl.value;
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
        fetch('/Payment/Order', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
            },
            body: JSON.stringify({
                OrderId: order.id,
                Subtotal: subtotal,
                Discount: discountAmount,
                TotalAmount: total,
                TotalDue: total,
                AmountPaid: total,
                PaymentMethod: paymentMethod
            })
        })
        .then(response => response.json())
        .then(data => {
            if (data.success && data.bill) {
                // Hiển thị hóa đơn ở modal
                const bill = data.bill;
                const billId = data.billId;
                let html = `<div class='text-center mb-3'>
                    <div class='font-bold text-xl mb-1'>HÓA ĐƠN THANH TOÁN</div>
                    <div class='text-gray-500 mb-1'>Bàn: <b>${bill.TableNumber}</b> | Thời gian: <b>${bill.CreatedAt}</b></div>
                </div>
                <table class='w-full mb-2 border'>
                    <thead><tr class='bg-gray-100'><th class='p-1'>Món</th><th class='p-1'>SL</th><th class='p-1'>Đơn giá</th><th class='p-1'>Thành tiền</th></tr></thead>
                    <tbody>`;
                for (const item of bill.Dishes) {
                    html += `<tr><td class='p-1'>${item.Name}</td><td class='p-1 text-center'>${item.Quantity}</td><td class='p-1 text-right'>${item.Price.toLocaleString()} ₫</td><td class='p-1 text-right'>${(item.Price*item.Quantity).toLocaleString()} ₫</td></tr>`;
                }
                html += `</tbody></table>
                <div class='text-right'>
                    <div>Tạm tính: <b>${bill.Subtotal.toLocaleString()} ₫</b></div>
                    <div>VAT: <b>${bill.Vat.toLocaleString()} ₫</b></div>
                    <div>Giảm giá: <b>${bill.Discount.toLocaleString()} ₫</b></div>
                    <div>Phương thức: <b>${bill.PaymentMethod}</b></div>
                    <div>Khách trả: <b>${bill.AmountPaid.toLocaleString()} ₫</b></div>
                    <div class='text-lg font-bold'>Tổng cộng: <span class='text-red-600'>${bill.Total.toLocaleString()} ₫</span></div>
                </div>`;
                // Thêm nút In và link mở trang in hóa đơn
                html += `<div class='mt-4 flex flex-col gap-2'>
                    <button onclick="window.print()" class="w-full bg-blue-600 hover:bg-blue-700 text-white rounded px-4 py-2">In hóa đơn</button>
                    <a href="/Bill/Print/${billId}" target="_blank" class="w-full bg-green-600 hover:bg-green-700 text-white rounded px-4 py-2 text-center">Xem/In lại hóa đơn</a>
                    <button onclick="closeInvoiceModalAndReload()" class="w-full bg-gray-300 hover:bg-gray-400 text-gray-800 rounded px-4 py-2 mt-2">Đóng</button>
                </div>`;
                // Tạo modal nếu chưa có
                let modal = document.getElementById('invoiceModal');
                if (!modal) {
                    modal = document.createElement('div');
                    modal.id = 'invoiceModal';
                    modal.className = 'fixed inset-0 bg-black bg-opacity-40 flex items-center justify-center z-50';
                    modal.innerHTML = `<div class="bg-white rounded-lg shadow-lg w-full max-w-md p-6 relative print:w-full print:max-w-full">
                        <button type="button" class="absolute top-2 right-2 text-gray-400 hover:text-gray-700" onclick="closeInvoiceModalAndReload()">&times;</button>
                        <div id="invoiceContent"></div>
                    </div>`;
                    document.body.appendChild(modal);
                }
                document.getElementById('invoiceContent').innerHTML = html;
                modal.classList.remove('hidden');
                // Đóng modal chi tiết order gốc
                document.getElementById('order-detail-modal')?.remove();
                // Reset nút thanh toán
                btn.disabled = false;
                btn.textContent = 'Hoàn tất thanh toán';
            } else if(data.success) {
                alert(data.message || "Thanh toán thành công!");
                document.getElementById('order-detail-modal')?.remove();
                location.reload();
            } else {
                alert(data.message || "Thanh toán thất bại!");
                btn.disabled = false;
                btn.textContent = 'Hoàn tất thanh toán';
            }
        })
        .catch(() => {
            alert("Có lỗi xảy ra khi thanh toán!");
            btn.disabled = false;
            btn.textContent = 'Hoàn tất thanh toán';
        });
    });

    // Hàm đóng modal hóa đơn và reload trang
    function closeInvoiceModalAndReload() {
        document.getElementById('invoiceModal')?.remove();
        location.reload();
    }

    // Initial summary
    updatePaymentSummary();
}
