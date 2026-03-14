(() => {
  const body = document.body;
  const modal = document.getElementById('productModal');

  const initGridControls = () => {
    const arrowButtons = document.querySelectorAll('.grid-arrow');

    const updateArrowVisibility = () => {
      arrowButtons.forEach((button) => {
        const grid = button.closest('.product-rail')?.querySelector('.product-grid');
        if (!grid) {
          button.classList.add('is-hidden');
          return;
        }
        const shouldShow = grid.scrollWidth - grid.clientWidth > 12;
        button.classList.toggle('is-hidden', !shouldShow);
      });
    };

    arrowButtons.forEach((button) => {
      button.addEventListener('click', () => {
        const grid = button.closest('.product-rail')?.querySelector('.product-grid');
        if (!grid) {
          return;
        }
        grid.scrollBy({
          left: grid.clientWidth * 0.85,
          behavior: 'smooth',
        });
      });
    });

    window.addEventListener('resize', updateArrowVisibility);
    updateArrowVisibility();

    const mobileMedia = window.matchMedia('(max-width: 768px)');
    document.querySelectorAll('[data-grid-collapse]').forEach((section) => {
      const cards = Array.from(section.querySelectorAll('.product-card'));
      const moreBtn = section.querySelector('.mobile-more-btn');
      if (!moreBtn || cards.length === 0) {
        return;
      }

      const chunk = Number(section.dataset.gridCollapse) || 4;
      let visibleCount = Math.min(chunk, cards.length);

      const applyVisibility = () => {
        if (!mobileMedia.matches) {
          cards.forEach((card) => card.classList.remove('is-hidden-mobile'));
          moreBtn.hidden = true;
          return;
        }

        moreBtn.hidden = cards.length <= chunk;
        cards.forEach((card, index) => {
          card.classList.toggle('is-hidden-mobile', index >= visibleCount);
        });
        moreBtn.textContent = visibleCount >= cards.length ? 'Hide' : 'More';
      };

      moreBtn.addEventListener('click', () => {
        if (!mobileMedia.matches) {
          return;
        }
        if (visibleCount >= cards.length) {
          visibleCount = chunk;
        } else {
          visibleCount = Math.min(visibleCount + chunk, cards.length);
        }
        applyVisibility();
      });

      mobileMedia.addEventListener('change', () => {
        if (!mobileMedia.matches) {
          visibleCount = cards.length;
        } else {
          visibleCount = Math.min(Math.max(visibleCount, chunk), cards.length);
        }
        applyVisibility();
      });

      applyVisibility();
    });
  };

  if (!modal) {
    initGridControls();
    return;
  }

  const defaultModalImage =
    'https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?auto=format&fit=crop&w=800&q=80';
  const formatPrice = (value) =>
    `${new Intl.NumberFormat('th-TH').format(value)}.-`;

  const modalElements = {
    name: document.getElementById('modalProductName'),
    sku: document.getElementById('modalSku'),
    color: document.getElementById('modalColor'),
    stock: document.getElementById('modalStock'),
    category: document.getElementById('modalCategory'),
    description: document.getElementById('modalDescription'),
    img: document.getElementById('modalProductImage'),
    sizeSelect: document.getElementById('modalSize'),
    quantity: document.getElementById('modalQuantity'),
    originalPrice: document.getElementById('modalOriginalPrice'),
    currentPrice: document.getElementById('modalCurrentPrice'),
  };

  let activeVariants = [];
  let currentProductData = null;

  const safeParseVariants = (value) => {
    try {
      const parsed = JSON.parse(value ?? '[]');
      return Array.isArray(parsed) ? parsed : [];
    } catch (error) {
      return [];
    }
  };

  const buildSizeOptions = (variants, fallbackSizes) => {
    if (!modalElements.sizeSelect) {
      return [];
    }

    const normalizedSizes = (fallbackSizes || '')
      .split('|')
      .map((size) => size.trim())
      .filter(Boolean);

    const variantSizes = variants
      .map((variant) => variant.size)
      .filter(Boolean);

    const combined = Array.from(new Set([...variantSizes, ...normalizedSizes]));
    modalElements.sizeSelect.innerHTML = '';

    if (combined.length === 0) {
      const option = document.createElement('option');
      option.value = '';
      option.textContent = 'Free Size';
      modalElements.sizeSelect.appendChild(option);
      modalElements.sizeSelect.disabled = true;
    } else {
      modalElements.sizeSelect.disabled = false;
      combined.forEach((size, index) => {
        const option = document.createElement('option');
        option.value = size;
        option.textContent = size;
        if (index === 0) {
          option.selected = true;
        }
        modalElements.sizeSelect.appendChild(option);
      });
    }

    return combined;
  };

  const updateVariantDetails = () => {
    if (!currentProductData) {
      return;
    }
    const selectedSize = modalElements.sizeSelect?.value;
    const candidate = selectedSize
      ? activeVariants.find((variant) => variant.size === selectedSize)
      : activeVariants[0];

    modalElements.color.textContent = candidate?.color || currentProductData.productColor || '-';
    modalElements.stock.textContent =
      typeof candidate?.stockQuantity === 'number'
        ? candidate.stockQuantity
        : currentProductData.productStock || '-';
  };

  const openModal = (card) => {
    const data = { ...card.dataset };
    currentProductData = data;

    modalElements.name.textContent = data.productName ?? '';
    modalElements.sku.textContent = data.productSku ?? '';
    modalElements.category.textContent = data.productCategory ?? '-';
    modalElements.description.textContent = data.productDescription || '';

    modalElements.img.src = data.productImage || defaultModalImage;
    modalElements.img.alt = data.productName ?? 'Selected shoe';

    modalElements.quantity.value = '1';

    activeVariants = safeParseVariants(data.productVariants);
    buildSizeOptions(activeVariants, data.productSizes);
    updateVariantDetails();

    const basePrice = Number(data.productPrice ?? 0);
    const salePrice = data.productSale ? Number(data.productSale) : null;

    modalElements.currentPrice.textContent = formatPrice(salePrice ?? basePrice);
    modalElements.originalPrice.textContent = salePrice ? formatPrice(basePrice) : '';

    modal.classList.add('is-visible');
    body.classList.add('modal-open');
    modal.setAttribute('aria-hidden', 'false');
  };

  const closeModal = () => {
    modal.classList.remove('is-visible');
    body.classList.remove('modal-open');
    modal.setAttribute('aria-hidden', 'true');
    currentProductData = null;
    activeVariants = [];
  };

  document.querySelectorAll('.product-card').forEach((card) => {
    const trigger = () => openModal(card);
    card.addEventListener('click', trigger);
    card.addEventListener('keydown', (event) => {
      if (event.key === 'Enter' || event.key === ' ') {
        event.preventDefault();
        trigger();
      }
    });
  });

  modal.querySelectorAll('[data-modal-close]').forEach((btn) => {
    btn.addEventListener('click', closeModal);
  });

  document.addEventListener('keydown', (event) => {
    if (event.key === 'Escape' && modal.classList.contains('is-visible')) {
      closeModal();
    }
  });

  modalElements.sizeSelect?.addEventListener('change', updateVariantDetails);

  modal.querySelectorAll('.qty-btn').forEach((btn) => {
    btn.addEventListener('click', () => {
      const action = btn.dataset.qtyAction;
      let current = Number(modalElements.quantity.value) || 1;
      if (action === 'minus') {
        current = Math.max(1, current - 1);
      } else {
        current += 1;
      }
      modalElements.quantity.value = String(current);
    });
  });

  initGridControls();
})();

