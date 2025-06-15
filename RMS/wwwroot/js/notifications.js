// Khởi tạo các âm thanh thông báo
const notificationSounds = {
    // order: new Audio('/sounds/order-notification.mp3'),
    // payment: new Audio('/sounds/payment-notification.mp3'),
    // table: new Audio('/sounds/table-notification.mp3'),
    // dish: new Audio('/sounds/dish-notification.mp3'),
    ingredient: new Audio('/sounds/ingredient-notification.mp3'),
    // default: new Audio('/sounds/default-notification.mp3')
};

// Kết nối tới SignalR Hub
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .build();

// Xử lý sự kiện nhận thông báo
connection.on("DataChanged", async function (data) {
    try {
        // Phát âm thanh thông báo ngay lập tức
        const sound = notificationSounds[data.notificationType] || notificationSounds.ingredient;
        sound.currentTime = 0; // Reset âm thanh về đầu
        await sound.play();

        // Sau khi âm thanh bắt đầu phát, thực hiện các hành động khác
        showNotification(data);

        // Cập nhật dữ liệu (nếu cần)
        if (typeof updateData === 'function') {
            updateData(data);
        }
    } catch (error) {
        console.log('Error playing sound:', error);
        // Nếu không phát được âm thanh, vẫn thực hiện các hành động khác
        showNotification(data);
        if (typeof updateData === 'function') {
            updateData(data);
        }
    }
});

// Hàm hiển thị thông báo
function showNotification(data) {
    // Kiểm tra xem trình duyệt có hỗ trợ thông báo không
    if (!("Notification" in window)) {
        return;
    }

    // Yêu cầu quyền hiển thị thông báo nếu chưa có
    if (Notification.permission !== "granted") {
        Notification.requestPermission();
    }

    // Hiển thị thông báo
    if (Notification.permission === "granted") {
        const notification = new Notification("RMS Notification", {
            body: `Có thay đổi mới: ${data.event}`,
            icon: '/images/logo.png' // Thay đổi đường dẫn logo của bạn
        });

        // Tự động đóng thông báo sau 5 giây
        setTimeout(() => notification.close(), 5000);
    }
}

// Bắt đầu kết nối
connection.start().catch(function (err) {
    console.error(err.toString());
}); 