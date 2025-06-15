// Bill-related functionality
document.addEventListener('DOMContentLoaded', function() {
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
    window.showBillDetails = function(id) {
        fetch(`/Bills/GetBillDetails/${id}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                return response.json();
            })
            .then(bill => {
                if (!bill) {
                    throw new Error('No data received');
                }

                currentBillData = bill; // Store the data for printing

                // Update bill info
                document.getElementById('tableNumber').textContent = bill.tableNumber || 'N/A';
                document.getElementById('billId').textContent = bill.id;
                document.getElementById('orderId').textContent = bill.orderId;
                document.getElementById('createdAt').textContent = bill.createdAt;

                // Update bill items
                document.getElementById('billItems').innerHTML = bill.items.map(item => `
                    <tr>
                        <td>${item.name}</td>
                        <td class="text-center">${item.quantity}</td>
                        <td class="text-right">${formatCurrency(item.price)}</td>
                        <td class="text-right">${formatCurrency(item.total)}</td>
                    </tr>
                `).join('');
                
                // Update bill summary
                document.getElementById('subtotal').textContent = formatCurrency(bill.subtotal);
                document.getElementById('vatPercent').textContent = bill.vatPercent;
                document.getElementById('vatAmount').textContent = formatCurrency(bill.vatAmount);
                document.getElementById('discountAmount').textContent = formatCurrency(bill.discountAmount);
                document.getElementById('totalDue').textContent = formatCurrency(bill.totalDue);
                
                document.getElementById('bill-modal').classList.remove('hidden');
            })
            .catch(error => {
                console.error('Error:', error);
                alert('Không thể tải thông tin hóa đơn');
            });
    };

    // Close bill modal
    window.closeBillModal = function() {
        document.getElementById('bill-modal').classList.add('hidden');
    };

    // Print bill function
    window.printBill = function() {
        if (!currentBillData) {
            alert("Không có dữ liệu hóa đơn để in");
            return;
        }

        const printContent = document.getElementById('print-content').cloneNode(true);
        const printWindow = window.open('', '_blank');
        
        // Get all styles from the current document
        const styles = Array.from(document.styleSheets)
            .map(styleSheet => {
                try {
                    return Array.from(styleSheet.cssRules)
                        .map(rule => rule.cssText)
                        .join('\n');
                } catch (e) {
                    return '';
                }
            })
            .join('\n');

        // Create the print document with embedded styles
        printWindow.document.write(`
            <!DOCTYPE html>
            <html>
            <head>
                <title>In hóa đơn</title>
                <style>
                    ${styles}
                    
                    /* Additional print-specific styles */
                    body {
                        margin: 0;
                        padding: 0;
                        font-family: Arial, sans-serif;
                        font-size: 12pt;
                        color: #1f2937;
                    }
                    
                    .bill-container {
                        width: 100%;
                        max-width: none;
                        padding: 10mm;
                        box-sizing: border-box;
                    }
                    
                    .bill-header {
                        text-align: center;
                        margin-bottom: 1rem;
                    }
                    
                    .bill-header .restaurant-name {
                        font-size: 20pt;
                        font-weight: bold;
                        color: #1e40af;
                        margin-bottom: 4mm;
                    }
                    
                    .bill-header .restaurant-info {
                        font-size: 10pt;
                        color: #4b5563;
                        line-height: 1.5;
                    }
                    
                    .bill-info {
                        border-top: 1px solid #e5e7eb;
                        border-bottom: 1px solid #e5e7eb;
                        margin: 6mm 0;
                        padding: 4mm 0;
                    }
                    
                    .bill-info-grid {
                        display: grid;
                        grid-template-columns: 1fr 1fr;
                        gap: 4mm;
                    }
                    
                    .bill-info-item {
                        display: flex;
                        justify-content: space-between;
                    }
                    
                    .bill-info-label {
                        font-weight: 600;
                        color: #374151;
                    }
                    
                    .bill-table {
                        width: 100%;
                        border-collapse: collapse;
                        margin: 6mm 0;
                    }
                    
                    .bill-table th {
                        background-color: #f3f4f6;
                        color: #1e40af;
                        font-weight: 600;
                        text-transform: uppercase;
                        font-size: 9pt;
                        padding: 3mm;
                        border: 1px solid #e5e7eb;
                    }
                    
                    .bill-table td {
                        padding: 2mm 3mm;
                        border: 1px solid #e5e7eb;
                        font-size: 10pt;
                    }
                    
                    .bill-table .text-center {
                        text-align: center;
                    }
                    
                    .bill-table .text-right {
                        text-align: right;
                    }
                    
                    .bill-summary {
                        border-top: 1px solid #e5e7eb;
                        padding-top: 0.75rem;
                        margin-top: 0.5rem;
                    }
                    
                    .bill-summary-item {
                        display: flex;
                        justify-content: space-between;
                        margin: 2mm 0;
                        font-size: 10pt;
                    }
                    
                    .bill-total {
                        display: flex;
                        justify-content: space-between;
                        font-size: 14pt;
                        font-weight: bold;
                        margin-top: 2mm;
                        padding-top: 2mm;
                        border-top: 1px solid #e5e7eb;
                    }
                    
                    .bill-total-amount {
                        color: #16a34a;
                    }
                    
                    .bill-footer {
                        text-align: center;
                        font-size: 8pt;
                        color: #6b7280;
                        line-height: 1.5;
                        margin-top: 12mm;
                    }
                    
                    @page {
                        size: auto;
                        margin: 0;
                    }
                </style>
            </head>
            <body>
                ${printContent.outerHTML}
            </body>
            </html>
        `);
        
        // Wait for styles to be applied
        printWindow.document.close();
        
        // Add a small delay to ensure styles are applied
        setTimeout(() => {
            printWindow.focus();
            printWindow.print();
            printWindow.close();
        }, 100);
    };

    // Format currency helper function
    function formatCurrency(amount) {
        return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
    }

    // Search functionality
    const searchInput = document.getElementById('bill-search');
    if (searchInput) {
        searchInput.addEventListener('input', function(e) {
            const search = e.target.value.toLowerCase();
            document.querySelectorAll('tbody tr').forEach(function(row) {
                const id = row.cells[0].textContent.toLowerCase();
                const order = row.cells[1].textContent.toLowerCase();
                const show = id.includes(search) || order.includes(search);
                row.style.display = show ? '' : 'none';
            });
        });
    }

    // Close modal when clicking outside
    const modal = document.getElementById('bill-modal');
    if (modal) {
        modal.addEventListener('click', function(e) {
            if (e.target === this) {
                this.classList.add('hidden');
            }
        });
    }
}); 