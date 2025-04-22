// Inject invoice styles dynamically
(function(){
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
    document.body.classList.add('invoice-page');
})();
