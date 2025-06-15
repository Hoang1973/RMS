// order.js - Quản lý modal thanh toán, tối giản, không logic thừa

// Định dạng tiền VND
function formatVND(amount) {
    return parseInt(amount).toLocaleString('vi-VN') + ' ₫';
}

// Sử dụng biến bool isPaid thay cho kiểm tra status
function isUnpaidStatus(isPaid) {
    return !isPaid;
}

function getStatusDisplay(status) {
    switch (status) {
        case 'Pending':
            return { text: 'Chờ xử lý', color: 'bg-yellow-100 text-yellow-800' };
        case 'Processing':
            return { text: 'Đang chuẩn bị', color: 'bg-orange-100 text-orange-800' };
        case 'Ready':
            return { text: 'Sẵn sàng phục vụ', color: 'bg-blue-100 text-blue-800' };
        case 'Completed':
            return { text: 'Đã thanh toán', color: 'bg-green-100 text-green-800' };
        case 'Cancelled':
            return { text: 'Đã hủy', color: 'bg-red-100 text-red-800' };
        default:
            return { text: 'Không rõ', color: 'bg-gray-100 text-gray-600' };
    }
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
    const { text: statusText, color: statusColor } = getStatusDisplay(order.status);
    const content = document.getElementById('order-detail-content');

    // Store customerPhoneNumber in the order object
    order.customerPhoneNumber = order.customerPhoneNumber || '';

    content.innerHTML = `
        <div class="flex flex-col space-y-6">
            <div>
                <div class="font-bold text-2xl text-gray-900 mb-1">
                    Đơn hàng - <span class="text-primary">${order.tableNumber ?? '-'}</span>
                </div>
                <div class="flex flex-col space-y-1 text-sm text-gray-600">
                    <div><span class="font-semibold">Bàn:</span> <span class="text-blue-600 font-bold">${order.tableNumber ?? '-'}</span></div>
                    <div><span class="font-semibold">Thời gian tạo:</span> <span>${order.createdAt ?? '-'}</span></div>
                    <div><span class="font-semibold">Thời gian phục vụ:</span> <span>${order.Datetiem ?? '-'}</span></div>
                    <div><span class="font-semibold">Trạng thái:</span> <span class="inline-block px-2 py-1 rounded ${statusColor}">${statusText}</span></div>
                </div>
            </div>
            <div>           
                <div class="font-semibold mb-2 text-base">Danh sách món</div>
                <div class="overflow-x-auto">
                    <table class="w-full text-sm">
                        <thead class="bg-gray-50 border-b">
                            <tr>
                                <th class="py-2 text-left">Món</th>
                                <th class="py-2 text-right">SL</th>
                                <th class="py-2 text-right">Đơn giá</th>
                                <th class="py-2 text-right">Thành tiền</th>
                            </tr>
                        </thead>
                        <tbody>
                            ${order.dishes && order.dishes.length > 0
                                ? order.dishes.map(d => `
                                    <tr>
                                        <td class="py-1">${d.name}</td>
                                        <td class="py-1 text-right">${d.quantity}</td>
                                        <td class="py-1 text-right">${formatVND(d.price)}</td>
                                        <td class="py-1 text-right">${formatVND(d.subtotal)}</td>
                                    </tr>
                                `).join('')
                                : `<tr><td colspan="4" class="py-2 text-center text-gray-400">Không có món nào</td></tr>`
                            }
                        </tbody>
                    </table>
                </div>
            </div>
            <div class="flex justify-end">
                <span class="font-bold text-xl text-green-700">Tổng tiền: ${formatVND(order.totalAmount)}</span>
            </div>
            <div class="flex justify-end gap-2">
                <button onclick="document.getElementById('order-detail-modal').remove()" class="px-5 py-2 bg-gray-200 hover:bg-gray-300 rounded text-gray-700 font-semibold">Đóng</button>
                ${!order.isPaid ? `
                    <button onclick="editOrder(${order.id})" class="px-5 py-2 bg-green-600 hover:bg-green-700 rounded text-white font-semibold">
                        <i class="fas fa-edit mr-1"></i>Sửa Order
                    </button>
                    <button id="start-payment-btn" class="px-5 py-2 bg-blue-600 hover:bg-blue-700 rounded text-white font-semibold">Thanh toán</button>
                ` : ''}
            </div>
        </div>
    `;
}

// Hàm render panel thanh toán
function renderPaymentPanel(order) {
    const modal = document.getElementById('order-detail-modal');
    if (!modal) return;
    
    // Chỉ cập nhật nội dung, không tạo lại modal
    const content = document.getElementById('order-detail-content');
    if (!content) return;

    // Tính toán subtotal từ dishes
    const subtotal = order.dishes.reduce((sum, item) => sum + (item.price * item.quantity), 0);
    const vat = subtotal * 0.08; // 8% VAT
    const total = subtotal + vat;
    
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
                                    <td class="py-1 text-right">${formatVND(d.price * d.quantity)}</td>
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
                        <button type="button" class="payment-method border p-2 rounded flex items-center justify-center" data-method="card">
                            <i class="fas fa-university mr-2"></i>
                            <span>Quét QR (Card)</span>
                        </button>
                        <div id="card-qr" class="w-full flex flex-col items-center mt-4 hidden">
                            <img src="/images/myqr.png" alt="QR chuyển khoản" class="w-48 h-48 object-contain border rounded mb-2" />
                            <div class="text-sm text-gray-700">Quét mã QR để thanh toán</div>
                        </div>
                    </div>
                    <button id="complete-payment-btn" class="w-full py-2 bg-green-500 hover:bg-green-600 text-white font-bold rounded disabled:opacity-60" disabled>Hoàn tất thanh toán</button>
                    <button onclick="document.getElementById('order-detail-modal').remove()" class="w-full mt-2 py-2 bg-gray-200 hover:bg-gray-300 rounded text-gray-700 font-semibold">Đóng</button>
                </div>
            </div>
        </div>
    `;
    
    // Khởi tạo các event listeners cho form thanh toán
    initializePaymentForm(order);
}

// Lắng nghe click nút "Thanh toán" để chuyển sang panel form chi tiết thanh toán duy nhất
if (!window._orderDetailPaymentListener) {
    document.addEventListener('click', function (e) {
        if (e.target && e.target.id === 'start-payment-btn') {
            const order = window._lastOrderDetail;
            if (!order) return;
            // Khi nhấn Thanh toán, chuyển luôn sang panel form thanh toán chi tiết
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
                        <button type="button" class="payment-method border p-2 rounded flex items-center justify-center" data-method="card">
                            <i class="fas fa-university mr-2"></i>
                            <span>Quét QR (Card)</span>
                        </button>
                        <div id="card-qr" class="w-full flex flex-col items-center mt-4 hidden">
                            <img src="/images/myqr.png" alt="QR chuyển khoản" class="w-48 h-48 object-contain border rounded mb-2" />
                            <div class="text-sm text-gray-700">Quét mã QR để thanh toán</div>
                        </div>
                    </div>
                    <button id="complete-payment-btn" class="w-full py-2 bg-green-500 hover:bg-green-600 text-white font-bold rounded disabled:opacity-60" disabled>Hoàn tất thanh toán</button>
                    <button onclick="document.getElementById('order-detail-modal').remove()" class="w-full mt-2 py-2 bg-gray-200 hover:bg-gray-300 rounded text-gray-700 font-semibold">Đóng</button>
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
            // Hiển thị QR cố định khi chọn Card
            const qrDiv = document.getElementById('card-qr');
            const qrImg = qrDiv.querySelector('img');
            if (paymentMethod === 'card') {
                qrImg.src = '/images/myqr.png'; // Đường dẫn QR cố định
                qrDiv.classList.remove('hidden');
            } else {
                qrDiv.classList.add('hidden');
            }

            selectedMethodBtn = this;
        });
    });

    // Complete payment
    document.getElementById('complete-payment-btn').addEventListener('click', function () {
        const btn = this;
        btn.disabled = true;
        btn.textContent = 'Đang xử lý...';
        fetch('/Payments/Order', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
            },
            body: JSON.stringify({
                orderId: order.id,
                tableId: order.tableId,
                subtotal: subtotal,
                discountValue: parseInt(discountValueEl.value) || 0,
                discountType: discountTypeEl.value,
                vatPercent: 8, // hoặc cho phép chọn nếu cần
                paymentMethod: paymentMethod,
                tableNumber: order.tableNumber,
                customerPhoneNumber: order.customerPhoneNumber || ''
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

// Hàm cập nhật tổng tiền
function updateTotalAmount() {
    const dishList = document.getElementById('dish-list');
    if (!dishList) return;

    let total = 0;
    const dishItems = dishList.querySelectorAll('.dish-item');
    
    dishItems.forEach(item => {
        const select = item.querySelector('select');
        const quantityInput = item.querySelector('input[type="number"]');
        const priceElement = item.querySelector('.dish-price');
        
        if (select && quantityInput && priceElement) {
            const quantity = parseInt(quantityInput.value) || 0;
            const price = parseInt(priceElement.dataset.price) || 0;
            total += quantity * price;
        }
    });

    // Cập nhật hiển thị tổng tiền
    const totalElement = document.getElementById('total-amount');
    if (totalElement) {
        totalElement.textContent = formatVND(total);
    }
}

// Hàm thêm món mới vào form
function addDish() {
    const dishList = document.getElementById('dish-list');
    if (!dishList) return;

    const index = dishList.querySelectorAll('.dish-item').length;
    const dishItem = document.createElement('div');
    dishItem.className = 'dish-item flex items-center gap-2 mb-2';
    dishItem.innerHTML = `
        <select name="Dishes[${index}].DishId" class="flex-1 border rounded p-2 dish-select">
            <option value="">Chọn món</option>
            ${window._dishes ? window._dishes.map(d =>
        `<option value="${d.id}" data-price="${d.price}">${d.name} - ${formatVND(d.price)}</option>`
    ).join('') : ''}
        </select>
        <input type="number" name="Dishes[${index}].Quantity" value="1" min="1" class="w-20 border rounded p-2 dish-quantity" onchange="updateTotalAmount()">
        <span class="dish-price" data-price="0">0 ₫</span>
        <button type="button" class="text-red-500 hover:text-red-700" onclick="removeDish(this)">
            <i class="fas fa-times"></i>
        </button>
    `;
    dishList.appendChild(dishItem);
}

// Hàm cập nhật giá món khi chọn
function updateDishPrice(select) {
    const option = select.options[select.selectedIndex];
    const price = option.dataset.price || 0;
    const dishItem = select.closest('.dish-item');
    const priceElement = dishItem.querySelector('.dish-price');
    if (priceElement) {
        priceElement.textContent = formatVND(price);
        priceElement.dataset.price = price;
    }
    updateTotalAmount();
}

// Hàm xóa món
function removeDish(button) {
    const dishItem = button.closest('.dish-item');
    if (dishItem) {
        dishItem.remove();
        updateDishIndexes();
        updateTotalAmount();
    }
}

function updateDishIndexes() {
    const dishItems = document.querySelectorAll('#dish-list .dish-item');
    dishItems.forEach((item, index) => {
        const select = item.querySelector('select');
        const input = item.querySelector('input[type="number"]');
        if (select) select.name = `Dishes[${index}].DishId`;
        if (input) input.name = `Dishes[${index}].Quantity`;
    });
}

// Hàm khởi tạo form đơn hàng
function initOrderForm(order = null) {
    // Show order form modal
    const modal = document.getElementById('orderModal');
    if (!modal) {
        console.error('Không tìm thấy modal form');
        return;
    }
    
    // Hiển thị modal
    modal.style.display = 'flex';
    
    // Set form title
    const titleEl = modal.querySelector('h2');
    if (titleEl) titleEl.textContent = order ? 'Sửa đơn hàng' : 'Tạo đơn hàng mới';
    
    // Fill form data
    const form = document.getElementById('orderCreateForm');
    if (!form) {
        console.error('Không tìm thấy form');
        return;
    }

    // Reset form
    form.reset();
    
    // Clear existing dishes
    const dishList = document.getElementById('dish-list');
    if (!dishList) {
        console.error('Không tìm thấy danh sách món');
        return;
    }
    dishList.innerHTML = '';

    // Nếu là sửa đơn hàng
    if (order) {
        // Điền thông tin cơ bản
        const customerPhoneInput = form.querySelector('input[name="CustomerPhoneNumber"]');
        const tableIdInput = form.querySelector('input[name="TableId"]');
        const noteInput = form.querySelector('textarea[name="Note"]');

        if (customerPhoneInput) customerPhoneInput.value = order.customerPhoneNumber || '';
        if (tableIdInput) tableIdInput.value = order.tableId || '';
        if (noteInput) noteInput.value = order.note || '';
        
        // Add existing dishes
        if (order.dishes && order.dishes.length > 0) {
            console.log('Adding dishes:', order.dishes);
            
            // Lấy danh sách món từ select đầu tiên
            const firstSelect = document.querySelector('.dish-select');
            if (firstSelect) {
                window._dishes = Array.from(firstSelect.options).map(opt => ({
                    id: opt.value,
                    name: opt.text
                })).filter(d => d.id !== ''); // Loại bỏ option "Chọn món ăn"
                
                // Thêm món vào form
                addDishesToForm(order.dishes);
            }
        }
        
        // Update total amount
        updateTotalAmount();
        
        // Change form action to edit
        form.onsubmit = function(e) {
            e.preventDefault();
            var formData = $(this).serialize();
            $.ajax({
                url: `/Orders/Edit/${order.id}`,
                type: 'POST',
                data: formData,
                success: function(res) {
                    if (res.success) {
                        $('#orderModal').hide();
                        toastr.success('Cập nhật đơn thành công!');
                        // Refresh the page or update the UI
                        setTimeout(() => location.reload(), 1000);
                    } else {
                        let errorMsg = res.message || 'Có lỗi xảy ra!';
                        if (res.errors) errorMsg += '\n' + res.errors.join('\n');
                        if (res.inner) errorMsg += '\n' + res.inner;
                        toastr.error(errorMsg);
                    }
                },
                error: function(xhr) {
                    toastr.error('Lỗi server!\n' + xhr.responseText);
                }
            });
        };
    } else {
        // Nếu là tạo đơn mới
        form.onsubmit = function(e) {
            e.preventDefault();
            var formData = $(this).serialize();
            $.ajax({
                url: '/Orders/Create',
                type: 'POST',
                data: formData,
                success: function(res) {
                    if (res.success) {
                        $('#orderModal').hide();
                        toastr.success('Tạo đơn thành công!');
                        // Refresh the page or update the UI
                        setTimeout(() => location.reload(), 1000);
                    } else {
                        let errorMsg = res.message || 'Có lỗi xảy ra!';
                        if (res.errors) errorMsg += '\n' + res.errors.join('\n');
                        if (res.inner) errorMsg += '\n' + res.inner;
                        toastr.error(errorMsg);
                    }
                },
                error: function(xhr) {
                    toastr.error('Lỗi server!\n' + xhr.responseText);
                }
            });
        };
    }
}

// Hàm thêm món vào form
function addDishesToForm(dishes) {
    const dishList = document.getElementById('dish-list');
    if (!dishList) return;

    dishes.forEach((item, index) => {
        // Tạo món mới
        addDish();
        
        // Lấy phần tử món vừa thêm
        const dishItem = dishList.lastElementChild;
        if (dishItem) {
            // Tìm select và input trong món mới
            const select = dishItem.querySelector('select');
            const input = dishItem.querySelector('input[type="number"]');
            
            if (select && window._dishes) {
                // Tìm món trong danh sách
                const dish = window._dishes.find(d => d.name === item.name);
                if (dish) {
                    select.value = dish.id;
                    // Trigger change event để cập nhật giá
                    const event = new Event('change');
                    select.dispatchEvent(event);
                }
            }
            
            if (input) {
                input.value = item.quantity;
            }
        }
    });
}

// Hàm sửa order
window.editOrder = function(orderId) {
    // Đóng modal chi tiết trước
    const detailModal = document.getElementById('order-detail-modal');
    if (detailModal) detailModal.remove();

    // Fetch order details
    fetch(`/Orders/DetailsJson/${orderId}`)
        .then(res => {
            if (!res.ok) throw new Error('Not found');
            return res.json();
        })
        .then(order => {
            console.log('Order data:', order);

            // Kiểm tra trạng thái đơn hàng
            if (order.status === 'Ready' || order.status === 'Completed' || order.status === 'Cancelled') {
                toastr.error('Không thể chỉnh sửa đơn hàng đã hoàn thành, đã thanh toán hoặc đã hủy');
                return;
            }

            // Show order form modal
            const modal = document.getElementById('orderModal');
            if (!modal) {
                console.error('Không tìm thấy modal form');
                return;
            }
            
            // Hiển thị modal
            modal.style.display = 'flex';
            
            // Set form title
            const titleEl = modal.querySelector('h2');
            if (titleEl) titleEl.textContent = 'Sửa đơn hàng';
            
            // Fill form data
            const form = document.getElementById('orderCreateForm');
            if (!form) {
                console.error('Không tìm thấy form');
                return;
            }

            // Reset form
            form.reset();
            
            // Clear existing dishes
            const dishList = document.getElementById('dish-list');
            if (!dishList) {
                console.error('Không tìm thấy danh sách món');
                return;
            }
            dishList.innerHTML = '';

            // Điền thông tin cơ bản
            const customerPhoneInput = form.querySelector('input[name="CustomerPhoneNumber"]');
            const tableIdInput = form.querySelector('input[name="TableId"]');
            const noteInput = form.querySelector('textarea[name="Note"]');

            if (customerPhoneInput) customerPhoneInput.value = order.customerPhoneNumber || '';
            if (tableIdInput) tableIdInput.value = order.tableId || '';
            if (noteInput) noteInput.value = order.note || '';
            
            // Add existing dishes
            if (order.dishes && order.dishes.length > 0) {
                console.log('Adding dishes:', order.dishes);
                
                // Lấy danh sách món từ select đầu tiên
                const firstSelect = document.querySelector('.dish-select');
                if (firstSelect) {
                    // Tạo danh sách món từ options
                    const dishes = Array.from(firstSelect.options).map(opt => ({
                        id: opt.value,
                        name: opt.text
                    })).filter(d => d.id !== ''); // Loại bỏ option "Chọn món ăn"
                    
                    // Thêm từng món vào form
                    order.dishes.forEach((item, index) => {
                        // Tạo món mới
                        addDish();
                        
                        // Lấy phần tử món vừa thêm
                        const dishItem = dishList.lastElementChild;
                        if (dishItem) {
                            // Tìm select và input trong món mới
                            const select = dishItem.querySelector('select');
                            const input = dishItem.querySelector('input[type="number"]');
                            
                            if (select) {
                                // Tìm món trong danh sách
                                const dish = dishes.find(d => d.name === item.name);
                                if (dish) {
                                    select.value = dish.id;
                                    // Trigger change event để cập nhật giá
                                    const event = new Event('change');
                                    select.dispatchEvent(event);
                                }
                            }
                            
                            if (input) {
                                input.value = item.quantity;
                            }
                        }
                    });
                }
            }
            
            // Update total amount
            updateTotalAmount();
            
            // Change form action to edit
            form.onsubmit = function(e) {
                e.preventDefault();
                var formData = $(this).serialize();
                $.ajax({
                    url: `/Orders/Edit/${orderId}`,
                    type: 'POST',
                    data: formData,
                    success: function(res) {
                        if (res.success) {
                            $('#orderModal').hide();
                            toastr.success('Cập nhật đơn thành công!');
                            // Refresh the page or update the UI
                            setTimeout(() => location.reload(), 1000);
                        } else {
                            let errorMsg = res.message || 'Có lỗi xảy ra!';
                            if (res.errors) errorMsg += '\n' + res.errors.join('\n');
                            if (res.inner) errorMsg += '\n' + res.inner;
                            toastr.error(errorMsg);
                        }
                    },
                    error: function(xhr) {
                        toastr.error('Lỗi server!\n' + xhr.responseText);
                    }
                });
            };
        })
        .catch(error => {
            console.error('Error:', error);
            toastr.error('Không tìm thấy đơn hàng hoặc lỗi server.');
        });
};

// Hàm tạo đơn mới
window.createOrder = function() {
    initOrderForm();
};

// Hàm khởi tạo form thanh toán
function initializePaymentForm(order) {
    // Xử lý sự kiện chọn phương thức thanh toán
    const paymentMethods = document.querySelectorAll('.payment-method');
    const cardQR = document.getElementById('card-qr');
    const completeBtn = document.getElementById('complete-payment-btn');
    
    paymentMethods.forEach(method => {
        method.addEventListener('click', function() {
            // Bỏ active tất cả các phương thức
            paymentMethods.forEach(m => m.classList.remove('bg-blue-50', 'border-blue-500'));
            
            // Active phương thức được chọn
            this.classList.add('bg-blue-50', 'border-blue-500');
            
            // Hiển thị/ẩn QR code
            if (this.dataset.method === 'card') {
                cardQR.classList.remove('hidden');
            } else {
                cardQR.classList.add('hidden');
            }
            
            // Enable nút hoàn tất
            completeBtn.disabled = false;
        });
    });
    
    // Xử lý sự kiện nhập giảm giá
    const discountType = document.getElementById('discount-type');
    const discountValue = document.getElementById('discount-value');
    const subtotalEl = document.getElementById('payment-subtotal');
    const taxEl = document.getElementById('payment-tax');
    const totalEl = document.getElementById('payment-total');
    
    function updateTotal() {
        const subtotal = parseFloat(subtotalEl.textContent.replace(/[^\d]/g, ''));
        const vat = parseFloat(taxEl.textContent.replace(/[^\d]/g, ''));
        let discount = parseFloat(discountValue.value) || 0;
        
        // Tính giảm giá theo % hoặc số tiền
        if (discountType.value === 'percent') {
            discount = (subtotal * discount) / 100;
        }
        
        const total = subtotal + vat - discount;
        totalEl.textContent = formatVND(total);
    }
    
    discountType.addEventListener('change', updateTotal);
    discountValue.addEventListener('input', updateTotal);
    
    // Xử lý sự kiện hoàn tất thanh toán
    completeBtn.addEventListener('click', function() {
        const selectedMethod = document.querySelector('.payment-method.bg-blue-50');
        if (!selectedMethod) {
            toastr.error('Vui lòng chọn phương thức thanh toán');
            return;
        }
        
        const paymentMethod = selectedMethod.dataset.method;
        const discount = parseFloat(discountValue.value) || 0;
        const discountType = document.getElementById('discount-type').value;
        const total = parseFloat(totalEl.textContent.replace(/[^\d]/g, ''));
        const subtotal = parseFloat(subtotalEl.textContent.replace(/[^\d]/g, ''));
        
        // Lấy anti-forgery token
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (!token) {
            toastr.error('Không tìm thấy token xác thực');
            return;
        }
        
        // Gọi API thanh toán
        $.ajax({
            url: '/Payments/Order',
            type: 'POST',
            contentType: 'application/json',
            headers: {
                'RequestVerificationToken': token
            },
            data: JSON.stringify({
                orderId: order.id,
                tableId: order.tableId,
                subtotal: subtotal,
                discountValue: discount,
                discountType: discountType,
                vatPercent: 8,
                paymentMethod: paymentMethod,
                tableNumber: order.tableNumber,
                customerPhoneNumber: order.customerPhoneNumber || ''
            }),
            success: function(res) {
                if (res.success) {
                    // Đóng modal thanh toán
                    document.getElementById('order-detail-modal').remove();
                    
                    // Hiển thị modal thành công
                    const successModal = document.createElement('div');
                    successModal.id = 'payment-success-modal';
                    successModal.className = 'fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50';
                    successModal.innerHTML = `
                        <div class="bg-white rounded-lg p-8 max-w-md w-full mx-4">
                            <div class="text-center">
                                <div class="text-green-500 text-6xl mb-4">
                                    <i class="fas fa-check-circle"></i>
                                </div>
                                <h3 class="text-2xl font-bold text-gray-900 mb-2">Thanh toán thành công!</h3>
                                <p class="text-gray-600 mb-6">Đơn hàng đã được thanh toán.</p>
                                <div class="space-y-4">
                                    <div class="flex justify-between text-sm">
                                        <span class="text-gray-600">Mã đơn:</span>
                                        <span class="font-semibold">#${order.id.toString().padStart(6, '0')}</span>
                                    </div>
                                    <div class="flex justify-between text-sm">
                                        <span class="text-gray-600">Bàn:</span>
                                        <span class="font-semibold">${order.tableNumber}</span>
                                    </div>
                                    <div class="flex justify-between text-sm">
                                        <span class="text-gray-600">Tổng tiền:</span>
                                        <span class="font-semibold">${formatVND(total)}</span>
                                    </div>
                                    <div class="flex justify-between text-sm">
                                        <span class="text-gray-600">Phương thức:</span>
                                        <span class="font-semibold">${paymentMethod === 'cash' ? 'Tiền mặt' : 'Quét QR'}</span>
                                    </div>
                                </div>
                                <div class="mt-8 flex justify-center">
                                    <button onclick="document.getElementById('payment-success-modal').remove(); location.reload();" class="bg-gray-200 text-gray-700 px-6 py-2 rounded hover:bg-gray-300">
                                        Đóng
                                    </button>
                                </div>
                            </div>
                        </div>
                    `;
                    document.body.appendChild(successModal);
                } else {
                    toastr.error(res.message || 'Có lỗi xảy ra!');
                }
            },
            error: function(xhr) {
                console.error('Payment error:', xhr.responseText);
                toastr.error('Lỗi server!\n' + xhr.responseText);
            }
        });
    });
}

// Hàm in hóa đơn
function printBill(orderId) {
    window.open(`/Bills/Print/${orderId}`, '_blank');
}

// Hàm hủy đơn hàng
function cancelOrder(orderId) {
    if (!confirm('Bạn có chắc chắn muốn hủy đơn hàng này?')) return;
    
    // Lấy anti-forgery token
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    if (!token) {
        toastr.error('Không tìm thấy token xác thực');
        return;
    }
    
    $.ajax({
        url: `/Orders/Cancel/${orderId}`,
        type: 'POST',
        headers: {
            'RequestVerificationToken': token
        },
        success: function(res) {
            if (res.success) {
                toastr.success('Hủy đơn hàng thành công!');
                setTimeout(() => location.reload(), 1000);
            } else {
                toastr.error(res.message || 'Có lỗi xảy ra!');
            }
        },
        error: function(xhr) {
            console.error('Cancel error:', xhr.responseText);
            toastr.error('Lỗi server!\n' + xhr.responseText);
        }
    });
}
