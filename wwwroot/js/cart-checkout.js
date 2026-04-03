(() => {
    const paymentSelect = document.getElementById('paymentMethod');
    const payButton = document.querySelector('.btn-pay');
    const checkoutModalEl = document.getElementById('checkoutModal');
    const paymentStatusModalEl = document.getElementById('paymentStatusModal');
    if (!paymentSelect || !payButton || !checkoutModalEl || !paymentStatusModalEl) {
        return;
    }

    const checkoutModal = new bootstrap.Modal(checkoutModalEl);
    const statusModal = new bootstrap.Modal(paymentStatusModalEl);
    const paymentTabs = document.querySelectorAll('.payment-tabs button');
    const panels = {
        'Credit Card': document.getElementById('creditPanel'),
        'PromptPay': document.getElementById('promptpayPanel')
    };

    const cardNumber = document.getElementById('cardNumber');
    const cardHolder = document.getElementById('cardHolder');
    const cardExpiryMonth = document.getElementById('cardExpiryMonth');
    const cardExpiryYear = document.getElementById('cardExpiryYear');
    const cardCvv = document.getElementById('cardCvv');
    const couponInput = document.getElementById('couponInput');
    const couponStatusText = document.getElementById('couponStatusText');
    const subtotalLabel = document.getElementById('checkoutSubtotal');
    const discountLabel = document.getElementById('checkoutDiscount');
    const finalLabel = document.getElementById('checkoutFinal');
    const submitButton = document.getElementById('checkoutSubmit');
    const statusIcon = document.getElementById('paymentStatusIcon');
    const statusTitle = document.getElementById('paymentStatusTitle');
    const statusMessage = document.getElementById('paymentStatusMessage');
    const statusHistoryBtn = document.getElementById('paymentStatusHistory');

    let currentMethod = 'Credit Card';
    let couponTimer;
    let reloadAfterStatus = false;
    const initialSubtotal = Number(window.checkoutDefaults?.subtotal ?? 0);
    let currentTotals = {
        subtotal: initialSubtotal,
        finalAmount: initialSubtotal,
        discountAmount: 0,
        couponCode: null
    };

    const formatCurrency = (value) => `${new Intl.NumberFormat('th-TH').format(value)} บาท`;

    const setPaymentTab = (method) => {
        currentMethod = method;
        paymentTabs.forEach((btn) => {
            const isActive = btn.dataset.paymentTab === method;
            btn.classList.toggle('active', isActive);
        });
        Object.entries(panels).forEach(([key, panel]) => {
            if (!panel) return;
            panel.classList.toggle('is-active', key === method);
        });
    };

    const showStatus = (isSuccess, title, message) => {
        reloadAfterStatus = isSuccess;
        statusIcon.textContent = isSuccess ? '✅' : '⚠️';
        statusTitle.textContent = title;
        statusMessage.textContent = message;
        if (statusHistoryBtn) {
            statusHistoryBtn.hidden = !isSuccess;
            statusHistoryBtn.textContent = 'ดู History';
            statusHistoryBtn.onclick = () => window.location.href = window.cartApi.orderHistory;
        }
        statusModal.show();
    };

    const ensureProfileComplete = async () => {
        try {
            const response = await fetch(window.cartApi.profileStatus);
            if (!response.ok) {
                throw new Error('profile request failed');
            }
            const result = await response.json();
            if (result.isComplete) {
                return true;
            }
            const fields = (result.missingFields || []).join(', ');
            showStatus(false, 'โปรดกรอกข้อมูลให้ครบ', `ต้องมีข้อมูล: ${fields}`);
            if (statusHistoryBtn && result.profileUrl) {
                statusHistoryBtn.hidden = false;
                statusHistoryBtn.textContent = 'แก้ไขโปรไฟล์';
                statusHistoryBtn.onclick = () => window.location.href = result.profileUrl;
            }
            return false;
        } catch (error) {
            console.error(error);
            alert('ไม่สามารถตรวจสอบข้อมูลผู้ใช้ได้');
            return false;
        }
    };

    const validateCoupon = async (code) => {
        try {
            const url = new URL(window.cartApi.validateCoupon, window.location.origin);
            if (code) {
                url.searchParams.set('code', code);
            }
            const response = await fetch(url);
            if (!response.ok) {
                throw new Error('coupon error');
            }
            const data = await response.json();
            currentTotals.subtotal = data.subtotal ?? currentTotals.subtotal;
            currentTotals.finalAmount = data.finalAmount ?? currentTotals.subtotal;
            currentTotals.discountAmount = data.discountAmount ?? 0;
            currentTotals.couponCode = data.isValid && data.couponId ? data.couponCode : null;
            subtotalLabel && (subtotalLabel.textContent = data.subtotalDisplay ?? formatCurrency(currentTotals.subtotal));
            discountLabel && (discountLabel.textContent = data.discountDisplay ?? `-${new Intl.NumberFormat('th-TH').format(currentTotals.discountAmount)} บาท`);
            finalLabel && (finalLabel.textContent = data.finalAmountDisplay ?? formatCurrency(currentTotals.finalAmount));
            if (couponStatusText) {
                couponStatusText.textContent = data.message || '';
                couponStatusText.classList.toggle('text-danger', !!data.hasCoupon && !data.isValid);
                couponStatusText.classList.toggle('text-success', !!data.hasCoupon && data.isValid);
            }
            return data;
        } catch (error) {
            console.error(error);
            if (couponStatusText) {
                couponStatusText.textContent = 'ไม่สามารถตรวจสอบคูปองได้';
                couponStatusText.classList.add('text-danger');
            }
            return null;
        }
    };

    const debouncedCoupon = (value) => {
        clearTimeout(couponTimer);
        couponTimer = setTimeout(() => validateCoupon(value.trim()), 400);
    };

    if (couponInput) {
        couponInput.addEventListener('input', (event) => debouncedCoupon(event.target.value));
    }

    paymentTabs.forEach((btn) => {
        btn.addEventListener('click', () => setPaymentTab(btn.dataset.paymentTab));
    });

    paymentSelect.addEventListener('change', () => {
        payButton.disabled = !paymentSelect.value;
    });
    payButton.disabled = !paymentSelect.value;

    payButton.addEventListener('click', async () => {
        if (!paymentSelect.value) {
            return;
        }
        const ok = await ensureProfileComplete();
        if (!ok) {
            return;
        }
        setPaymentTab(paymentSelect.value);
        couponInput && validateCoupon(couponInput.value.trim());
        checkoutModal.show();
    });

    const validateCardFields = () => {
        if (!cardNumber || !cardHolder || !cardExpiryMonth || !cardExpiryYear || !cardCvv) {
            return { success: false, message: 'กรอกข้อมูลบัตรให้ครบถ้วน' };
        }
        const digits = cardNumber.value.replace(/\D/g, '');
        if (digits.length !== 16) {
            return { success: false, message: 'หมายเลขบัตรต้องมี 16 หลัก' };
        }
        if (!cardHolder.value.trim()) {
            return { success: false, message: 'กรุณากรอกชื่อ-สกุลตามหน้าบัตร' };
        }
        if (!cardExpiryMonth.value) {
            return { success: false, message: 'กรุณาเลือกเดือนหมดอายุ' };
        }
        if (!(cardExpiryYear.value && cardExpiryYear.value.length === 4)) {
            return { success: false, message: 'กรุณากรอกปี ค.ศ. 4 หลัก' };
        }
        const cvvDigits = cardCvv.value.replace(/\D/g, '');
        if (cvvDigits.length !== 3) {
            return { success: false, message: 'รหัสหลังบัตรต้องมี 3 หลัก' };
        }
        return { success: true };
    };

    submitButton?.addEventListener('click', async () => {
        const typedCode = couponInput?.value?.trim();
        if (typedCode) {
            const latest = await validateCoupon(typedCode);
            if (!latest || !latest.isValid || !latest.couponId) {
                showStatus(false, 'คูปองไม่ถูกต้อง', latest?.message || 'กรุณาใช้คูปองที่ยังไม่หมดอายุ');
                return;
            }
        }

        if (currentMethod === 'Credit Card') {
            const validationResult = validateCardFields();
            if (!validationResult.success) {
                showStatus(false, 'ชำระเงินไม่สำเร็จ', validationResult.message);
                return;
            }
        }

        submitButton.disabled = true;
        submitButton.textContent = 'กำลังชำระเงิน...';

        try {
            const payload = {
                paymentMethod: currentMethod,
                couponCode: currentTotals.couponCode,
                promptPayConfirmed: currentMethod === 'PromptPay'
            };

            if (currentMethod === 'Credit Card') {
                payload.creditCard = {
                    cardNumber: cardNumber.value.replace(/\D/g, ''),
                    cardholderName: cardHolder.value.trim(),
                    expiryMonth: cardExpiryMonth.value,
                    expiryYear: cardExpiryYear.value,
                    cvv: cardCvv.value
                };
            }

            const response = await fetch(window.cartApi.submitPayment, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });
            const result = await response.json();
            if (!result.success) {
                showStatus(false, 'ชำระเงินไม่สำเร็จ', result.message || 'กรุณาลองใหม่อีกครั้ง');
                return;
            }
            checkoutModal.hide();
            showStatus(true, 'ชำระเงินสำเร็จ', `คำสั่งซื้อ #${result.orderId} ยอดชำระ ${result.finalAmountDisplay}`);
            if (statusHistoryBtn) {
                statusHistoryBtn.hidden = false;
                statusHistoryBtn.textContent = 'ไปหน้า History';
                statusHistoryBtn.onclick = () => window.location.href = result.historyUrl || window.cartApi.orderHistory;
            }
        } catch (error) {
            console.error(error);
            showStatus(false, 'เกิดข้อผิดพลาด', 'ไม่สามารถชำระเงินได้ในขณะนี้');
        } finally {
            submitButton.disabled = false;
            submitButton.textContent = 'ยืนยันการชำระเงิน';
        }
    });

    paymentStatusModalEl.addEventListener('hidden.bs.modal', () => {
        if (reloadAfterStatus) {
            window.location.href = window.cartApi.orderHistory;
        }
        reloadAfterStatus = false;
    });
})();
