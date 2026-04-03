(() => {
    const detailButtons = document.querySelectorAll('[data-order-detail]');
    const modalEl = document.getElementById('orderDetailModal');
    if (!detailButtons.length || !modalEl) {
        return;
    }

    const modal = new bootstrap.Modal(modalEl);
    const orderNumber = document.getElementById('detailOrderNumber');
    const createdAt = document.getElementById('detailCreatedAt');
    const paymentMethod = document.getElementById('detailPaymentMethod');
    const finalAmount = document.getElementById('detailFinalAmount');
    const couponInfo = document.getElementById('detailCouponInfo');
    const customer = document.getElementById('detailCustomer');
    const contact = document.getElementById('detailContact');
    const address = document.getElementById('detailAddress');
    const itemsContainer = document.getElementById('detailItems');
    const timelineContainer = document.getElementById('detailTimeline');

    const baseUrl = (window.orderDetailApi?.detailUrl || '/Order/DetailsData').replace(/\/$/, '');

    const buildTimeline = (timeline) => {
        if (!timelineContainer) return;
        timelineContainer.innerHTML = '';
        (timeline || []).forEach((step) => {
            const wrapper = document.createElement('div');
            wrapper.className = step.isActive ? 'timeline-step active' : 'timeline-step';
            wrapper.innerHTML = `
                <div class="icon"><i class="bi ${step.icon || 'bi-circle'}"></i></div>
                <span>${step.label || ''}</span>
                <small class="text-muted">${step.timestampDisplay || ''}</small>
            `;
            timelineContainer.appendChild(wrapper);
        });
    };

    const buildItems = (items) => {
        if (!itemsContainer) return;
        itemsContainer.innerHTML = '';
        (items || []).forEach((item) => {
            const row = document.createElement('div');
            row.className = 'd-flex justify-content-between border-bottom py-1';
            row.innerHTML = `
                <div>
                    <strong>${item.productName}</strong>
                    <div class="small text-muted">สี: ${item.color || '-'} • ไซซ์: ${item.size || '-'}</div>
                </div>
                <div class="text-end">
                    <div>x${item.quantity}</div>
                    <small>${item.lineTotalDisplay || ''}</small>
                </div>
            `;
            itemsContainer.appendChild(row);
        });
    };

    const openDetail = async (orderId) => {
        try {
            const response = await fetch(`${baseUrl}/${orderId}`);
            if (!response.ok) {
                throw new Error('detail error');
            }
            const data = await response.json();
            orderNumber.textContent = data.orderNumber || `คำสั่งซื้อ #${data.orderId}`;
            createdAt.textContent = data.createdAtDisplay || '';
            paymentMethod.textContent = data.paymentMethod || '-';
            finalAmount.textContent = data.finalAmountDisplay || '';
            couponInfo.textContent = data.couponCode ? `ใช้คูปอง: ${data.couponCode}` : 'ไม่ได้ใช้คูปอง';
            customer.textContent = data.customerName || '-';
            contact.textContent = `โทร: ${data.customerPhone || '-'} | อีเมล: ${data.customerEmail || '-'}`;
            address.textContent = data.customerAddress || '-';
            buildItems(data.items);
            buildTimeline(data.timeline);
            modal.show();
        } catch (error) {
            console.error(error);
            alert('ไม่สามารถโหลดรายละเอียดคำสั่งซื้อได้');
        }
    };

    detailButtons.forEach((button) => {
        button.addEventListener('click', () => {
            const id = button.getAttribute('data-order-id');
            if (!id) {
                return;
            }
            openDetail(id);
        });
    });
})();
