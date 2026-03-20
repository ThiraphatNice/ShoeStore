(() => {
  if (!window.managerApi) {
    return;
  }

  const state = {
    users: [],
    selectedUserId: null,
    deleteUserId: null,
  };

  const getElement = (id) => document.getElementById(id);

  const modals = {
    directory: getElement('userDirectoryModal'),
    edit: getElement('editUserModal'),
    delete: getElement('deleteUserModal'),
    alert: getElement('managerAlertModal'),
  };

  const openModal = (modal) => {
    if (modal) {
      modal.classList.add('is-visible');
    }
  };

  const closeModal = (modal) => {
    if (modal) {
      modal.classList.remove('is-visible');
    }
  };

  document.querySelectorAll('.stock-modal [data-close]').forEach((btn) => {
    btn.addEventListener('click', () => closeModal(btn.closest('.stock-modal')));
  });

  const showAlert = (message) => {
    const node = getElement('managerAlertMessage');
    if (node) {
      node.textContent = message;
    }
    openModal(modals.alert);
  };

  const tableBody = document.querySelector('#managedUsersTable tbody');

  const renderUserTable = () => {
    if (!tableBody) {
      return;
    }
    tableBody.innerHTML = '';
    state.users.forEach((user) => {
      const row = document.createElement('tr');
      row.innerHTML = `
        <td>${user.id}</td>
        <td>${user.roleId}</td>
        <td>${user.roleName}</td>
        <td>${user.fullName}</td>
        <td>${user.email}</td>
        <td>${user.password}</td>
        <td>${user.phone ?? '-'}</td>
        <td>${user.address ?? '-'}</td>
        <td>
          <div class="d-flex gap-2 flex-wrap">
            <button class="btn btn-sm btn-auth-primary" data-action="edit" data-user-id="${user.id}">แก้ไข</button>
            <button class="btn btn-sm btn-danger" data-action="delete" data-user-id="${user.id}">ลบ</button>
          </div>
        </td>
      `;
      tableBody.appendChild(row);
    });
  };

  const fetchUsers = async () => {
    try {
      const response = await fetch(window.managerApi.listUsers);
      const result = await response.json();
      if (!result.success) {
        showAlert(result.message || 'ไม่สามารถดึงรายชื่อผู้ใช้ได้');
        return;
      }
      state.users = result.data ?? [];
      renderUserTable();
    } catch (error) {
      console.error(error);
      showAlert('เกิดข้อผิดพลาดในการโหลดรายชื่อผู้ใช้');
    }
  };

  const populateEditForm = (user) => {
    state.selectedUserId = user.id;
    getElement('editUserId').value = user.id;
    getElement('editRoleSelect').value = user.roleId;
    getElement('editFullName').value = user.fullName ?? '';
    getElement('editEmail').value = user.email ?? '';
    getElement('editPassword').value = '';
    getElement('editPhone').value = user.phone ?? '';
    getElement('editAddress').value = user.address ?? '';
  };

  const handleRowAction = (event) => {
    const btn = event.target.closest('[data-action]');
    if (!btn) {
      return;
    }
    const userId = Number(btn.dataset.userId);
    const user = state.users.find((u) => u.id === userId);
    if (!user) {
      return;
    }

    if (btn.dataset.action === 'edit') {
      populateEditForm(user);
      openModal(modals.edit);
    } else if (btn.dataset.action === 'delete') {
      state.deleteUserId = user.id;
      const summary = getElement('deleteUserSummary');
      if (summary) {
        summary.textContent = `คุณกำลังจะลบบัญชีของ ${user.fullName} (${user.email})`;
      }
      getElement('managerPasswordInput').value = '';
      openModal(modals.delete);
    }
  };

  tableBody?.addEventListener('click', handleRowAction);

  const validateEditForm = () => {
    const roleId = Number(getElement('editRoleSelect').value);
    const fullName = getElement('editFullName').value.trim();
    const email = getElement('editEmail').value.trim();
    if (!state.selectedUserId || !roleId || !fullName || !email) {
      showAlert('กรุณากรอกข้อมูลให้ครบ');
      return null;
    }

    return {
      userId: state.selectedUserId,
      roleId,
      fullName,
      email,
      password: getElement('editPassword').value.trim(),
      phone: getElement('editPhone').value.trim(),
      address: getElement('editAddress').value.trim(),
    };
  };

  const saveUserChanges = async () => {
    const payload = validateEditForm();
    if (!payload) {
      return;
    }

    try {
      const response = await fetch(window.managerApi.updateUser, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
      });
      const result = await response.json();
      if (!result.success) {
        showAlert(result.message || 'ไม่สามารถบันทึกข้อมูลผู้ใช้ได้');
        return;
      }
      closeModal(modals.edit);
      await fetchUsers();
      showAlert('บันทึกข้อมูลผู้ใช้เรียบร้อยแล้ว');
    } catch (error) {
      console.error(error);
      showAlert('เกิดข้อผิดพลาดในการบันทึกข้อมูลผู้ใช้');
    }
  };

  const confirmDeleteUser = async () => {
    if (!state.deleteUserId) {
      return;
    }
    const password = getElement('managerPasswordInput').value.trim();
    if (!password) {
      showAlert('กรุณากรอกรหัสผ่านเพื่อยืนยัน');
      return;
    }

    const payload = {
      userId: state.deleteUserId,
      managerPassword: password,
    };

    try {
      const response = await fetch(window.managerApi.deleteUser, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
      });
      const result = await response.json();
      if (!result.success) {
        showAlert(result.message || 'ไม่สามารถลบบัญชีผู้ใช้ได้');
        return;
      }
      closeModal(modals.delete);
      await fetchUsers();
      showAlert('ลบบัญชีผู้ใช้เรียบร้อยแล้ว');
    } catch (error) {
      console.error(error);
      showAlert('เกิดข้อผิดพลาดในการลบบัญชีผู้ใช้');
    }
  };

  getElement('openUserDirectoryBtn')?.addEventListener('click', async () => {
    await fetchUsers();
    openModal(modals.directory);
  });

  getElement('refreshUserListBtn')?.addEventListener('click', fetchUsers);
  getElement('saveUserBtn')?.addEventListener('click', saveUserChanges);
  getElement('confirmDeleteBtn')?.addEventListener('click', confirmDeleteUser);
})();
