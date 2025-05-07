// Bill-related functionality
(function() {
    let currentBillData = null;

    // Inject invoice styles
    const style = document.createElement('style');
    style.innerHTML = `
        body.invoice-page { font-family: Arial, sans-serif; background: #f7fafc; }
        .invoice-container { max-width: 480px; margin: 32px auto; background: #fff; border-radius: 12px; box-shadow: 0 2px 12px #0001; padding: 32px 24px; }
        .logo { width: 60px; margin-bottom: 8px; }
        .header { text-align: center; margin-bottom: 20px; }
        .title { font-size: 1.5rem; font-weight: bold; margin-bottom: 2px; color: #1a202c; }
        .address { font-size: 0.95rem; color: #718096; margin-bottom: 12px; }
        .meta { font-size: 1rem; color: #4a5568; margin-bottom: 8px; }
        .invoice-details { width: 100%; border-collapse: collapse; margin-bottom: 18px; }
        .invoice-details th, .invoice-details td { border: 1px solid #e2e8f0; padding: 8px 6px; text-align: left; font-size: 1rem; }
        .invoice-details th { background: #f1f5f9; }
        .totals-row td { border: none; }
        .totals-row .label { text-align: right; color: #4a5568; }
        .totals-row .value { text-align: right; font-weight: bold; color: #1a202c; font-size: 1.1rem; }
        .print-btn { display: block; margin: 18px auto 0; background: #3182ce; color: #fff; border: none; border-radius: 6px; padding: 10px 28px; font-size: 1rem; font-weight: 600; cursor: pointer; transition: background 0.2s; }
        .print-btn:hover { background: #2563eb; }
        @media print { .print-btn { display: none; } .invoice-container { box-shadow: none; border: none; } }
    `;
    document.head.appendChild(style);

    // Show bill details in modal
    window.showBillDetails = function(billId) {
        fetch(`/Bills/GetBillDetails/${billId}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                return response.json();
            })
            .then(data => {
                if (!data) {
                    throw new Error('No data received');
                }

                currentBillData = data; // Store the data for printing

                // Update modal content
                const elements = {
                    billId: document.getElementById('billId'),
                    orderId: document.getElementById('orderId'),
                    tableNumber: document.getElementById('tableNumber'),
                    createdAt: document.getElementById('createdAt'),
                    subtotal: document.getElementById('subtotal'),
                    vatPercent: document.getElementById('vatPercent'),
                    vatAmount: document.getElementById('vatAmount'),
                    discountAmount: document.getElementById('discountAmount'),
                    totalDue: document.getElementById('totalDue'),
                    billItems: document.getElementById('billItems')
                };

                // Check if all elements exist
                for (const [key, element] of Object.entries(elements)) {
                    if (!element) {
                        console.error(`Element with id '${key}' not found`);
                        return;
                    }
                }

                // Update content
                elements.billId.textContent = '#' + data.id;
                elements.orderId.textContent = '#' + data.orderId;
                elements.tableNumber.textContent = data.tableNumber;
                elements.createdAt.textContent = new Date(data.createdAt).toLocaleString('vi-VN');
                elements.subtotal.textContent = formatCurrency(data.subtotal);
                elements.vatPercent.textContent = data.vatPercent;
                elements.vatAmount.textContent = formatCurrency(data.vatAmount);
                elements.discountAmount.textContent = formatCurrency(data.discountAmount);
                elements.totalDue.textContent = formatCurrency(data.totalDue);

                // Update items table
                elements.billItems.innerHTML = '';
                if (data.items && Array.isArray(data.items)) {
                    data.items.forEach(item => {
                        const row = document.createElement('tr');
                        row.innerHTML = `
                            <td class="border px-2 py-1">${item.name}</td>
                            <td class="border px-2 py-1 text-center">${item.quantity}</td>
                            <td class="border px-2 py-1 text-right">${formatCurrency(item.price)}</td>
                            <td class="border px-2 py-1 text-right">${formatCurrency(item.total)}</td>
                        `;
                        elements.billItems.appendChild(row);
                    });
                }

                // Show modal
                document.getElementById('billDetailsModal').classList.remove('hidden');
            })
            .catch(error => {
                console.error('Error:', error);
                alert('Có lỗi xảy ra khi tải thông tin hóa đơn');
            });
    };

    // Print bill function
    window.printBill = function(billId = null) {
        // Nếu có billId được truyền vào, thì fetch dữ liệu và sau đó in
        if (billId) {
            fetch(`/Bills/GetBillDetails/${billId}`)
                .then(response => response.json())
                .then(data => {
                    if (!data) throw new Error("Không tìm thấy hóa đơn");
                    currentBillData = data;
                    renderAndPrint();
                })
                .catch(error => {
                    console.error(error);
                    alert("Không thể tải dữ liệu hóa đơn để in");
                });
        } 
        // Nếu đã có dữ liệu sẵn, thì in luôn
        else if (currentBillData) {
            renderAndPrint();
        } 
        // Ngược lại, báo lỗi
        else {
            alert("Không có dữ liệu hóa đơn để in");
        }
    };
    
    function renderAndPrint() {
        const content = document.querySelector('#billDetailsModal .max-w-md').innerHTML;
        const printWindow = window.open('', '', 'width=800,height=600');
    
        printWindow.document.write(`
            <!DOCTYPE html>
            <html>
            <head>
                <title>In hóa đơn</title>
                <link href="https://cdn.tailwindcss.com" rel="stylesheet" />
                <style>
                    body { background: white; font-family: Arial, sans-serif; }
                    @media print {
                        body { padding: 20px; }
                        .no-print { display: none !important; }
                    }
                </style>
            </head>
            <body class="invoice-page">
                <div class="invoice-container">${content}</div>
                <div class="no-print text-center mt-4">
                    <button onclick="window.print()" class="print-btn">In hóa đơn</button>
                </div>
            </body>
            </html>
        `);
    
        printWindow.document.close();
        printWindow.focus();
    }
    

    // Format currency helper function
    function formatCurrency(amount) {
        return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
    }

    // Initialize event listeners when DOM is loaded
    document.addEventListener('DOMContentLoaded', function() {
        // Close modal when clicking the close button
        const closeButton = document.getElementById('closeModal');
        if (closeButton) {
            closeButton.addEventListener('click', function() {
                document.getElementById('billDetailsModal').classList.add('hidden');
            });
        }

        // Close modal when clicking outside
        const modal = document.getElementById('billDetailsModal');
        if (modal) {
            modal.addEventListener('click', function(e) {
                if (e.target === this) {
                    this.classList.add('hidden');
                }
            });
        }
    });
})(); 