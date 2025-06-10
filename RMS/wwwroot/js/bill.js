function showBillDetails(id) {
    fetch(`/Bills/GetBillDetails/${id}`)
        .then(response => response.json())
        .then(bill => {
            // Format date
            const createdAt = new Date(bill.createdAt).toLocaleString('vi-VN', {
                year: 'numeric',
                month: '2-digit',
                day: '2-digit',
                hour: '2-digit',
                minute: '2-digit'
            });

            // Update bill info
            document.getElementById('tableNumber').textContent = bill.tableNumber || 'N/A';
            document.getElementById('billId').textContent = bill.id;
            document.getElementById('orderId').textContent = bill.orderId;
            document.getElementById('createdAt').textContent = createdAt;

            // Update bill items
            document.getElementById('billItems').innerHTML = bill.items.map(item => `
                <tr>
                    <td>${item.name}</td>
                    <td class="text-center">${item.quantity}</td>
                    <td class="text-right">${item.price.toLocaleString()} ₫</td>
                    <td class="text-right">${item.total.toLocaleString()} ₫</td>
                </tr>
            `).join('');
            
            // Update bill summary
            document.getElementById('subtotal').textContent = bill.subtotal.toLocaleString() + ' ₫';
            document.getElementById('vatPercent').textContent = bill.vatPercent;
            document.getElementById('vatAmount').textContent = bill.vatAmount.toLocaleString() + ' ₫';
            document.getElementById('discountAmount').textContent = bill.discountAmount.toLocaleString() + ' ₫';
            document.getElementById('totalDue').textContent = bill.totalDue.toLocaleString() + ' ₫';
            
            document.getElementById('bill-modal').classList.remove('hidden');
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Không thể tải thông tin hóa đơn');
        });
}

function closeBillModal() {
    document.getElementById('bill-modal').classList.add('hidden');
}

function printBill() {
    // Clone the print content to avoid modifying the original
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
} 