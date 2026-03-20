# Staff Manager Feature Plan

## Overview
Build a dedicated "Staff Manager" workspace (accessible to Admin + Staff Manag) mirroring the Stock Control UX but focused on user accounts. Provide a modal table listing every non-admin user, plus edit/delete flows that respect permissions and require password confirmation before destructive actions.

## Backend
1. **ViewModels** (`ViewModels/Staff/ManagerViewModels.cs`)
   - `StaffUserAdminPageViewModel`: exposes non-admin role dropdown for edit modal.
   - `ManagedUserRowViewModel`: id, roleId, roleName, fullname, email, password, phone, address.
   - `UpdateManagedUserRequest` & `DeleteManagedUserRequest` with validation attributes.
2. **Controller changes** (`Controllers/StaffController.cs`)
   - Update `StaffSections["Staff Manag"]` to call new async `ManageUsers` action.
   - `ManageUsers` loads selectable roles (exclude Admin) and returns the new view model.
   - API endpoints (Authorize Admin/Staff Manag):
     * `GetManagedUsers`  JSON list excluding Admin (ordered by id).
     * `UpdateManagedUser`  validates payload, rejects admin mutations, enforces valid target role, updates fields, saves.
     * `DeleteManagedUser`  verifies manager identity via claim, matches password, blocks admin/dependent users, deletes when safe.
   - Shared helpers: `IsAdminRole(string roleName)`, `Task<bool> HasDependencies(int userId)` for orders/cart.

## Frontend View (`Views/Staff/ManageUsers.cshtml`)
- Base layout mimics stock page: hero card + action cards ("ดูรายชื่อผู้ใช้งาน", "สิทธิ์การจัดการ").
- Hidden modals:
  1. **User directory modal** – table columns per requirement with trailing action buttons.
  2. **Edit modal** – form with dropdown (roles), text inputs for fullname/email/password/phone, textarea for address, save button.
  3. **Delete confirm modal** – shows target summary + password field, confirm + cancel buttons.
  4. **Alert modal** reused from stock for status messages.
- Emit `window.managerApi` endpoints + serialized role options for JS.

## Client Script (`wwwroot/js/staff-manager.js`)
1. Wire up open/close modal behaviors (reuse pattern from stock file).
2. Fetch users (`managerApi.listUsers`), populate table rows + attach data attributes to buttons.
3. Edit flow: load row data into form, validate required fields (non-empty, email format, password optional but allowed), submit via `managerApi.updateUser`, refresh table.
4. Delete flow: open confirm, display user summary, require password input, POST to `managerApi.deleteUser`, clear form + refresh.
5. Shared utility: `showAlert`, `resetForms`, simple loading state (disable buttons while awaiting response).

## Permissions & Validation
- All fetch endpoints require Admin or Staff Manag role.
- JS prevents editing/deleting admin by never including them; server double-checks.
- Delete requires manager password; mismatch returns error without deletion.
- Block role changes to Admin (role dropdown excludes and backend guards).

## Testing
1. Create/login as Staff Manager.
2. Open Staff Dashboard  pick manager section  ensure page loads with role dropdown.
3. Click "ดูรายชื่อผู้ใช้งาน"  verify modal shows all non-admin users with accurate fields.
4. Edit a user (change role/email/phone)  observe success toast + updated table + DB verification.
5. Attempt to delete user with wrong password  expect error; with correct password + no dependencies  entry disappears from modal + DB.
6. Confirm admin accounts never appear and cannot be targeted even via crafted requests.
7. Sanity-check as Admin to ensure they also can access page if desired.
