(() => {
  const body = document.body;
  const modal = document.getElementById('productModal');
  if (!modal) {
    return;
  }

  const formatPrice = (value) =>
    `${new Intl.NumberFormat('th-TH').format(value)}.-`;

  const modalElements = {
    name: document.getElementById('modalProductName'),
    sku: document.getElementById('modalSku'),
    color: document.getElementById('modalColor'),
    stock: document.getElementById('modalStock'),
    img: document.getElementById('modalProductImage'),
    sizeSelect: document.getElementById('modalSize'),
    quantity: document.getElementById('modalQuantity'),
    originalPrice: document.getElementById('modalOriginalPrice'),
    currentPrice: document.getElementById('modalCurrentPrice'),
  };

  const openModal = (card) => {
    const data = card.dataset;
    modalElements.name.textContent = data.productName ?? '';
    modalElements.sku.textContent = data.productSku ?? '';
    modalElements.color.textContent = data.productColor ?? '';
    modalElements.stock.textContent = data.productStock ?? '-';
    modalElements.img.src = data.productImage ?? '';
    modalElements.img.alt = data.productName ?? 'Selected shoe';

    // Populate sizes
    modalElements.sizeSelect.innerHTML = '';
    const sizes = (data.productSizes || '')
      .split('|')
      .map((s) => s.trim())
      .filter(Boolean);
    sizes.forEach((size) => {
      const option = document.createElement('option');
      option.value = size;
      option.textContent = size;
      modalElements.sizeSelect.appendChild(option);
    });

    modalElements.quantity.value = '1';

    const basePrice = Number(data.productPrice ?? 0);
    const salePriceRaw = data.productSale;
    const salePrice =
      salePriceRaw && !Number.isNaN(Number(salePriceRaw))
        ? Number(salePriceRaw)
        : null;

    modalElements.currentPrice.textContent = formatPrice(
      salePrice ?? basePrice
    );
    modalElements.originalPrice.textContent = salePrice
      ? formatPrice(basePrice)
      : '';

    modal.classList.add('is-visible');
    body.classList.add('modal-open');
    modal.setAttribute('aria-hidden', 'false');
  };

  const closeModal = () => {
    modal.classList.remove('is-visible');
    body.classList.remove('modal-open');
    modal.setAttribute('aria-hidden', 'true');
  };

  document
    .querySelectorAll('.product-card')
    .forEach((card) => {
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

  // Quantity control
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

  // Scroll arrows
  document.querySelectorAll('.grid-arrow').forEach((button) => {
    button.addEventListener('click', () => {
      const grid = button.closest('.product-rail')?.querySelector('.product-grid');
      if (grid) {
        grid.scrollBy({
          left: grid.clientWidth * 0.75,
          behavior: 'smooth',
        });
      }
    });
  });

  // Mobile "More" toggle logic
  const mobileMedia = window.matchMedia('(max-width: 768px)');
  document.querySelectorAll('[data-grid-collapse]').forEach((section) => {
    const cards = Array.from(section.querySelectorAll('.product-card'));
    const moreBtn = section.querySelector('.mobile-more-btn');
    if (!moreBtn) {
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
})();
