(() => {
  if (!window.stockApi) {
    return;
  }

  const state = {
    productId: null,
    variants: [],
  };

  const getElement = (id) => document.getElementById(id);

  const modals = {
    product: getElement('productModal'),
    inventory: getElement('inventoryModal'),
    create: getElement('newProductModal'),
    alert: getElement('alertModal'),
  };

  const openModal = (modal) => {
    if (!modal) return;
    modal.classList.add('is-visible');
  };

  const closeModal = (modal) => {
    if (!modal) return;
    modal.classList.remove('is-visible');
  };

  document.querySelectorAll('[data-close]').forEach((btn) => {
    btn.addEventListener('click', () => {
      const target = btn.closest('.stock-modal');
      closeModal(target);
    });
  });

  const showAlert = (message) => {
    const node = getElement('alertMessage');
    if (node) {
      node.textContent = message;
    }
    openModal(modals.alert);
  };

  const populateVariants = () => {
    const sizeSelect = getElement('variantSizeSelect');
    const colorSelect = getElement('variantColorSelect');
    if (!sizeSelect || !colorSelect) {
      return;
    }

    const sizes = Array.from(
      new Set(state.variants.map((variant) => variant.size).filter(Boolean))
    );
    const colors = Array.from(
      new Set(state.variants.map((variant) => variant.color).filter(Boolean))
    );

    sizeSelect.innerHTML = '<option value="">เลือกไซส์</option>';
    sizes.forEach((size) => {
      const option = document.createElement('option');
      option.value = size;
      option.textContent = size;
      sizeSelect.appendChild(option);
    });

    colorSelect.innerHTML = '<option value="">เลือกสี</option>';
    colors.forEach((color) => {
      const option = document.createElement('option');
      option.value = color;
      option.textContent = color;
      colorSelect.appendChild(option);
    });
  };

  const getSelectedVariant = () => {
    const size = getElement('variantSizeSelect')?.value || '';
    const color = getElement('variantColorSelect')?.value || '';
    if (!size || !color) {
      return null;
    }
    return state.variants.find(
      (variant) => variant.size === size && variant.color === color
    );
  };

  const fetchProduct = async () => {
    const input = getElement('stockProductIdInput');
    const productId = Number(input?.value);
    if (!productId) {
      showAlert('กรุณากรอกรหัสสินค้าที่ต้องการค้นหา');
      return;
    }

    try {
      const response = await fetch(`${window.stockApi.getProduct}?id=${productId}`);
      const result = await response.json();
      if (!result.success) {
        showAlert(result.message || 'ไม่พบข้อมูลสินค้าที่ค้นหา');
        return;
      }

      const data = result.data;
      state.productId = data.id;
      state.variants = data.variants || [];

      getElement('productNameInput').value = data.name ?? '';
      getElement('productDescriptionInput').value = data.description ?? '';
      getElement('productPriceInput').value = data.price ?? 0;
      getElement('productDiscountInput').value = data.discountPercent ?? 0;
      getElement('productCategorySelect').value = data.categoryId ?? '';
      getElement('productImageInput').value = data.imageUrl ?? '';

      populateVariants();
      openModal(modals.product);
    } catch (error) {
      console.error(error);
      showAlert('ไม่สามารถดึงข้อมูลสินค้าได้ กรุณาลองอีกครั้ง');
    }
  };

  const updateProductInfo = async () => {
    if (!state.productId) {
      return;
    }

    const payload = {
      productId: state.productId,
      name: getElement('productNameInput').value.trim(),
      description: getElement('productDescriptionInput').value.trim(),
      price: Number(getElement('productPriceInput').value),
      discountPercent: Number(getElement('productDiscountInput').value),
      categoryId: Number(getElement('productCategorySelect').value),
      imageUrl: getElement('productImageInput').value.trim(),
    };

    if (!payload.name || Number.isNaN(payload.price) || Number.isNaN(payload.discountPercent) || !payload.categoryId) {
      showAlert('กรุณากรอกข้อมูลสินค้าให้ครบถ้วนก่อนบันทึก');
      return;
    }

    try {
      const response = await fetch(window.stockApi.updateProduct, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
      });
      const result = await response.json();
      if (!result.success) {
        showAlert(result.message || 'ไม่สามารถบันทึกข้อมูลสินค้าได้');
        return;
      }
      showAlert('บันทึกข้อมูลสินค้าเรียบร้อยแล้ว');
    } catch (error) {
      console.error(error);
      showAlert('เกิดข้อผิดพลาดในการบันทึกข้อมูล');
    }
  };

  const updateVariantStock = async () => {
    if (!state.productId) {
      return;
    }
    const variant = getSelectedVariant();
    if (!variant) {
      showAlert('กรุณาเลือกไซส์และสีที่ต้องการก่อนอัปเดตสต็อก');
      return;
    }
    const quantity = Number(getElement('variantQuantityInput').value);
    if (Number.isNaN(quantity) || quantity < 0) {
      showAlert('กรุณาระบุจำนวนสต็อกเป็นตัวเลขที่ถูกต้อง');
      return;
    }

    const payload = {
      productId: state.productId,
      size: variant.size,
      color: variant.color,
      quantity,
    };

    try {
      const response = await fetch(window.stockApi.updateVariant, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
      });
      const result = await response.json();
      if (!result.success) {
        showAlert(result.message || 'ไม่สามารถอัปเดตสต็อกได้');
        return;
      }
      showAlert('อัปเดตจำนวนสต็อกเรียบร้อยแล้ว');
      await fetchProduct();
    } catch (error) {
      console.error(error);
      showAlert('เกิดข้อผิดพลาดในการอัปเดตสต็อก');
    }
  };

  const addVariant = async () => {
    if (!state.productId) {
      return;
    }
    const size = getElement('newSizeInput').value.trim();
    const color = getElement('newColorInput').value.trim();
    if (!size || !color) {
      showAlert('กรุณากรอกไซส์และสีที่ต้องการเพิ่มให้ครบถ้วน');
      return;
    }

    const payload = {
      productId: state.productId,
      size,
      color,
    };

    try {
      const response = await fetch(window.stockApi.addVariant, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
      });
      const result = await response.json();
      if (!result.success) {
        showAlert(result.message || 'ไม่สามารถเพิ่มไซส์/สีใหม่ได้');
        return;
      }
      getElement('newSizeInput').value = '';
      getElement('newColorInput').value = '';
      await fetchProduct();
      showAlert('เพิ่มไซส์/สีใหม่สำเร็จ สามารถกำหนดสต็อกได้ทันที');
    } catch (error) {
      console.error(error);
      showAlert('เกิดข้อผิดพลาดในการเพิ่มไซส์/สีใหม่');
    }
  };

  const listInventory = async () => {
    try {
      const response = await fetch(window.stockApi.listInventory);
      const result = await response.json();
      if (!result.success) {
        showAlert(result.message || 'ไม่สามารถดึงรายการสินค้าได้');
        return;
      }

      const tbody = getElement('inventoryTable')?.querySelector('tbody');
      if (tbody) {
        tbody.innerHTML = '';
        const formatter = new Intl.NumberFormat('th-TH');
        result.data.forEach((item) => {
          const row = document.createElement('tr');
          row.innerHTML = `
            <td>${item.id}</td>
            <td>${item.name}</td>
            <td>${item.category}</td>
            <td>${formatter.format(Number(item.price) || 0)}</td>
            <td>${Number(item.discountPercent ?? 0).toFixed(1)}%</td>
            <td>${item.stockTotal ?? 0}</td>
            <td>${item.isLimited ? 'Yes' : '-'}</td>
          `;
          tbody.appendChild(row);
        });
      }

      openModal(modals.inventory);
    } catch (error) {
      console.error(error);
      showAlert('เกิดข้อผิดพลาดในการโหลดรายการสินค้า');
    }
  };

  const createProduct = async () => {
    const payload = {
      name: getElement('createNameInput').value.trim(),
      description: getElement('createDescriptionInput').value.trim(),
      price: Number(getElement('createPriceInput').value),
      discountPercent: Number(getElement('createDiscountInput').value),
      categoryId: Number(getElement('createCategorySelect').value),
      imageUrl: getElement('createImageInput').value.trim(),
    };

    if (
      !payload.name ||
      !payload.description ||
      Number.isNaN(payload.price) ||
      Number.isNaN(payload.discountPercent) ||
      !payload.categoryId
    ) {
      showAlert('กรุณากรอกข้อมูลสินค้าใหม่ให้ครบถ้วน');
      return;
    }

    try {
      const response = await fetch(window.stockApi.createProduct, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
      });
      const result = await response.json();
      if (!result.success) {
        showAlert(result.message || 'ไม่สามารถเพิ่มสินค้าใหม่ได้');
        return;
      }
      showAlert('เพิ่มสินค้าใหม่สำเร็จแล้ว');
      closeModal(modals.create);
      getElement('createNameInput').value = '';
      getElement('createDescriptionInput').value = '';
      getElement('createPriceInput').value = '';
      getElement('createDiscountInput').value = '';
      getElement('createImageInput').value = '';
      const categorySelect = getElement('createCategorySelect');
      if (categorySelect) {
        categorySelect.selectedIndex = 0;
      }
    } catch (error) {
      console.error(error);
      showAlert('เกิดข้อผิดพลาดในการเพิ่มสินค้าใหม่');
    }
  };

  getElement('searchProductBtn')?.addEventListener('click', fetchProduct);
  getElement('updateProductBtn')?.addEventListener('click', updateProductInfo);
  getElement('updateVariantBtn')?.addEventListener('click', updateVariantStock);
  getElement('addVariantBtn')?.addEventListener('click', addVariant);
  getElement('openInventoryBtn')?.addEventListener('click', listInventory);
  getElement('openNewProductBtn')?.addEventListener('click', () => openModal(modals.create));
  getElement('createProductBtn')?.addEventListener('click', createProduct);
})();
