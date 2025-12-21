<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useAdminStore } from '../../stores/admin'
import { useAuthStore } from '../../stores/auth'

const admin = useAdminStore()
const auth = useAuthStore()

const roleLabel = computed(() => auth.roleName || (auth.isAdmin ? 'Quản trị viên' : 'Khách hàng'))
const userName = computed(() => auth.user?.FullName || 'Tài khoản ẩn danh')
const email = computed(() => auth.user?.Email || 'Không có email')
const roleNote = computed(() =>
  auth.isAdmin ? 'Bạn có quyền quản trị toàn bộ hệ thống.' : 'Bạn đang dùng quyền người dùng cơ bản.',
)

const permissions = computed(() => {
  if (auth.isAdmin) {
    return [
      'Quản lý người dùng và phân quyền',
      'Quản lý sản phẩm và combo',
      'Theo dõi đơn hàng và trạng thái thanh toán',
      'Xem thống kê và báo cáo',
    ]
  }
  return ['Xem menu và đặt món', 'Theo dõi đơn hàng cá nhân', 'Cập nhật hồ sơ cá nhân']
})

onMounted(admin.loadRoles)
</script>

<template>
  <section class="role-page">
    <div class="page-header">
      <div>
        <h2>Vai trò & quyền truy cập</h2>
        <p>Thông tin quyền hiện tại của tài khoản quản trị.</p>
      </div>
      <span class="pill">{{ auth.isAdmin ? 'Quyền đầy đủ' : 'Quyền giới hạn' }}</span>
    </div>

    <div class="role-grid">
      <div class="role-card">
        <div class="role-head">
          <div>
            <p class="label">Vai trò hiện tại</p>
            <h3>{{ roleLabel }}</h3>
            <p class="muted">{{ roleNote }}</p>
          </div>
          <div class="role-icon">
            <i class="pi pi-shield"></i>
          </div>
        </div>
        <div class="role-meta">
          <div>
            <span>Tài khoản</span>
            <strong>{{ userName }}</strong>
          </div>
          <div>
            <span>Email</span>
            <strong>{{ email }}</strong>
          </div>
        </div>
      </div>

      <div class="role-card">
        <h3>Quyền truy cập</h3>
        <ul class="perm-list">
          <li v-for="item in permissions" :key="item">
            <i class="pi pi-check-circle"></i>
            <span>{{ item }}</span>
          </li>
        </ul>
      </div>

      <div class="role-card">
        <h3>Danh sách vai trò</h3>
        <ul class="role-list">
          <li v-for="role in admin.roles" :key="role.RoleId">
            <div>
              <strong>{{ role.Name }}</strong>
              <span>{{ role.UserCount }} tài khoản</span>
              <div v-if="role.Users.length" class="role-users">
                <div v-for="user in role.Users" :key="user.Email" class="role-user">
                  <p>{{ user.FullName }}</p>
                  <small>{{ user.Email }}</small>
                </div>
              </div>
            </div>
            <small>#{{ role.RoleId }}</small>
          </li>
        </ul>
      </div>

      <div class="role-card">
        <h3>Khuyến nghị bảo mật</h3>
        <div class="note">
          <i class="pi pi-lock"></i>
          <div>
            <p>Luôn bảo vệ tài khoản quản trị bằng mật khẩu mạnh.</p>
            <span>Cập nhật mật khẩu định kỳ và không chia sẻ thông tin đăng nhập.</span>
          </div>
        </div>
        <div class="note">
          <i class="pi pi-clock"></i>
          <div>
            <p>Kiểm tra lịch sử truy cập thường xuyên.</p>
            <span>Phát hiện sớm các hoạt động bất thường để bảo vệ hệ thống.</span>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<style scoped>
.role-page {
  display: grid;
  gap: 18px;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  gap: 12px;
}

.page-header h2 {
  font-family: 'Fraunces', serif;
  font-size: clamp(1.6rem, 2.6vw, 2.2rem);
}

.page-header p {
  color: var(--muted);
}

.pill {
  padding: 6px 12px;
  border-radius: 999px;
  background: rgba(255, 107, 53, 0.12);
  color: var(--accent);
  font-weight: 600;
  font-size: 0.85rem;
}

.role-grid {
  display: grid;
  gap: 16px;
  grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
}

.role-card {
  background: var(--surface);
  border-radius: var(--radius);
  border: 1px solid rgba(31, 19, 11, 0.08);
  padding: 16px;
  box-shadow: var(--shadow);
  display: grid;
  gap: 12px;
}

.role-head {
  display: flex;
  justify-content: space-between;
  gap: 12px;
}

.role-head h3 {
  font-size: 1.3rem;
}

.label {
  color: var(--muted);
  font-size: 0.85rem;
}

.muted {
  color: var(--muted);
}

.role-icon {
  width: 44px;
  height: 44px;
  border-radius: 12px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: rgba(255, 107, 53, 0.12);
  color: var(--accent);
  font-size: 1.2rem;
}

.role-meta {
  display: grid;
  gap: 10px;
  background: var(--surface-2);
  padding: 12px;
  border-radius: var(--radius);
}

.role-meta span {
  color: var(--muted);
  font-size: 0.85rem;
}

.role-meta strong {
  display: block;
  margin-top: 4px;
}

.perm-list {
  list-style: none;
  display: grid;
  gap: 10px;
}

.perm-list li {
  display: flex;
  gap: 8px;
  align-items: center;
}

.perm-list i {
  color: var(--accent);
}

.note {
  display: flex;
  gap: 10px;
  align-items: flex-start;
  background: rgba(255, 107, 53, 0.08);
  border-radius: var(--radius);
  padding: 12px;
}

.note i {
  color: var(--accent);
  font-size: 1.1rem;
}

.note span {
  color: var(--muted);
  font-size: 0.85rem;
}

.role-list {
  list-style: none;
  display: grid;
  gap: 10px;
}

.role-list li {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: var(--surface-2);
  border-radius: var(--radius);
  padding: 10px 12px;
}

.role-list strong {
  display: block;
}

.role-list span {
  color: var(--muted);
  font-size: 0.85rem;
}

.role-users {
  margin-top: 8px;
  display: grid;
  gap: 6px;
}

.role-user p {
  font-size: 0.9rem;
}

.role-user small {
  color: var(--muted);
}

.role-list small {
  color: var(--muted);
  font-size: 0.8rem;
}
</style>
