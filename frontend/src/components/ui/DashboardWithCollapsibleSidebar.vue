<script setup lang="ts">
import { computed, ref } from 'vue'

type ActivityTone = 'green' | 'blue' | 'orange' | 'red'

type ActivityItem = {
  title: string
  description: string
  time: string
  tone: ActivityTone
}

const props = defineProps<{
  roleLabel: string
  userName: string
  email: string
  isAdmin: boolean
}>()

const open = ref(true)
const selected = ref('Tổng quan')

const stats = computed(() => [
  { label: 'Vai trò hiện tại', value: props.roleLabel, note: props.isAdmin ? 'Đầy đủ quyền' : 'Giới hạn' },
  { label: 'Phiên đăng nhập', value: 'Đang hoạt động', note: 'Cập nhật vừa xong' },
  { label: 'Yêu cầu chờ xử lý', value: props.isAdmin ? '3' : '0', note: 'Trong hôm nay' },
  { label: 'Thông báo mới', value: '5', note: 'Đã lọc quan trọng' },
])

const activities = computed<ActivityItem[]>(() => [
  {
    title: 'Đã xác thực quyền truy cập',
    description: props.isAdmin ? 'Bạn đang ở vai trò quản trị viên' : 'Bạn đang ở vai trò người dùng',
    time: 'Vừa xong',
    tone: 'green',
  },
  {
    title: 'Cập nhật hồ sơ',
    description: 'Ảnh đại diện đã được đồng bộ',
    time: '12 phút trước',
    tone: 'blue',
  },
  {
    title: 'Lịch bảo trì hệ thống',
    description: 'Kiểm tra quyền vào lúc 22:00',
    time: '1 giờ trước',
    tone: 'orange',
  },
  {
    title: 'Cảnh báo đăng nhập',
    description: 'Không có hoạt động bất thường',
    time: '2 giờ trước',
    tone: 'red',
  },
])

const quickActions = [
  { title: 'Tổng quan', icon: 'pi pi-home' },
  { title: 'Người dùng', icon: 'pi pi-users' },
  { title: 'Đơn hàng', icon: 'pi pi-shopping-cart' },
  { title: 'Báo cáo', icon: 'pi pi-chart-line' },
  { title: 'Cấu hình', icon: 'pi pi-cog' },
]

const toneClass = (tone: ActivityTone) => {
  if (tone === 'green') return 'tone-green'
  if (tone === 'blue') return 'tone-blue'
  if (tone === 'orange') return 'tone-orange'
  return 'tone-red'
}
</script>

<template>
  <div class="role-dashboard">
    <aside class="rail" :class="{ collapsed: !open }">
      <div class="rail-header">
        <div class="logo">SF</div>
        <div v-if="open" class="brand">
          <p class="brand-name">Saigon Fast</p>
          <p class="brand-sub">Bảng điều khiển</p>
        </div>
      </div>
      <button v-if="open" class="account">
        <i class="pi pi-user"></i>
        <div>
          <p class="account-name">{{ props.userName }}</p>
          <p class="account-email">{{ props.email }}</p>
        </div>
      </button>
      <nav class="rail-nav">
        <button
          v-for="item in quickActions"
          :key="item.title"
          class="rail-item"
          :class="{ active: selected === item.title }"
          @click="selected = item.title"
        >
          <i :class="item.icon"></i>
          <span v-if="open">{{ item.title }}</span>
        </button>
      </nav>
      <button class="rail-toggle" @click="open = !open">
        <i class="pi pi-angle-double-right" :class="{ flipped: open }"></i>
        <span v-if="open">Thu gọn</span>
      </button>
    </aside>

    <section class="dashboard-content">
      <header class="dashboard-header">
        <div>
          <h1>Trang quản trị vai trò</h1>
          <p>Theo dõi quyền truy cập và hoạt động gần đây của tài khoản.</p>
        </div>
        <div class="header-actions">
          <button class="icon-button" aria-label="Thông báo">
            <i class="pi pi-bell"></i>
            <span class="ping"></span>
          </button>
          <button class="icon-button" aria-label="Tài khoản">
            <i class="pi pi-user"></i>
          </button>
        </div>
      </header>

      <div class="hero">
        <div class="hero-copy">
          <p class="pill">Vai trò hiện tại</p>
          <h2>{{ props.roleLabel }}</h2>
          <p>
            {{ props.isAdmin ? 'Bạn có quyền quản trị toàn hệ thống.' : 'Bạn đang ở chế độ người dùng thông thường.' }}
          </p>
          <div class="hero-badges">
            <span class="badge">Xác thực 2 lớp</span>
            <span class="badge">Hoạt động ổn định</span>
          </div>
        </div>
        <div class="hero-image" role="presentation"></div>
      </div>

      <div class="stats">
        <article v-for="stat in stats" :key="stat.label" class="stat-card">
          <p class="stat-label">{{ stat.label }}</p>
          <p class="stat-value">{{ stat.value }}</p>
          <p class="stat-note">{{ stat.note }}</p>
        </article>
      </div>

      <div class="grid">
        <section class="activity">
          <div class="section-title">
            <h3>Nhật ký hoạt động</h3>
            <button class="link">Xem tất cả</button>
          </div>
          <div class="activity-list">
            <div v-for="item in activities" :key="item.title" class="activity-item">
              <div class="activity-icon" :class="toneClass(item.tone)">
                <i class="pi pi-bolt"></i>
              </div>
              <div class="activity-info">
                <p class="activity-title">{{ item.title }}</p>
                <p class="activity-desc">{{ item.description }}</p>
              </div>
              <span class="activity-time">{{ item.time }}</span>
            </div>
          </div>
        </section>

        <section class="side-panels">
          <div class="panel">
            <h3>Kiểm tra quyền</h3>
            <p class="muted">Hệ thống xác định vai trò dựa trên đăng nhập hiện tại.</p>
            <ul class="checklist">
              <li>
                <i class="pi pi-check-circle"></i>
                <span>Token phiên hoạt động</span>
              </li>
              <li>
                <i class="pi pi-check-circle"></i>
                <span>Vai trò: {{ props.roleLabel }}</span>
              </li>
              <li>
                <i class="pi pi-check-circle"></i>
                <span>Bảo vệ API quản trị</span>
              </li>
            </ul>
          </div>
          <div class="panel">
            <h3>Thống kê nhanh</h3>
            <div class="meter">
              <div class="meter-row">
                <span>Độ tin cậy</span>
                <strong>92%</strong>
              </div>
              <div class="meter-bar">
                <div class="meter-fill" style="width: 92%"></div>
              </div>
            </div>
            <div class="meter">
              <div class="meter-row">
                <span>Mức độ truy cập</span>
                <strong>{{ props.isAdmin ? 'Cao' : 'Trung bình' }}</strong>
              </div>
              <div class="meter-bar">
                <div class="meter-fill alt" :style="{ width: props.isAdmin ? '86%' : '54%' }"></div>
              </div>
            </div>
          </div>
        </section>
      </div>
    </section>
  </div>
</template>

<style scoped>
.role-dashboard {
  display: grid;
  grid-template-columns: minmax(64px, 220px) 1fr;
  gap: 20px;
  background: #f6f4f0;
  border-radius: 20px;
  border: 1px solid rgba(31, 19, 11, 0.08);
  overflow: hidden;
  min-height: 70vh;
}

.rail {
  display: flex;
  flex-direction: column;
  gap: 16px;
  padding: 18px 12px;
  border-right: 1px solid rgba(31, 19, 11, 0.08);
  background: #fffaf5;
  transition: width 0.3s ease;
}

.rail.collapsed {
  width: 64px;
}

.rail-header {
  display: flex;
  align-items: center;
  gap: 10px;
}

.logo {
  width: 36px;
  height: 36px;
  border-radius: 12px;
  background: linear-gradient(140deg, var(--accent), var(--accent-2));
  color: #fff;
  font-weight: 700;
  display: grid;
  place-items: center;
}

.brand-name {
  font-weight: 700;
  margin: 0;
}

.brand-sub {
  margin: 0;
  font-size: 0.75rem;
  color: var(--muted);
}

.account {
  border: none;
  background: transparent;
  display: flex;
  gap: 10px;
  padding: 10px;
  border-radius: 12px;
  align-items: center;
  cursor: pointer;
  text-align: left;
}

.account:hover {
  background: rgba(255, 107, 53, 0.08);
}

.account-name {
  margin: 0;
  font-size: 0.85rem;
  font-weight: 600;
}

.account-email {
  margin: 0;
  font-size: 0.72rem;
  color: var(--muted);
}

.rail-nav {
  display: grid;
  gap: 6px;
}

.rail-item {
  border: none;
  background: transparent;
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px;
  border-radius: 12px;
  cursor: pointer;
  color: var(--text);
  font-weight: 600;
}

.rail-item.active {
  background: rgba(255, 107, 53, 0.15);
}

.rail-toggle {
  margin-top: auto;
  border: none;
  background: transparent;
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px;
  border-radius: 12px;
  cursor: pointer;
  color: var(--muted);
}

.rail-toggle:hover {
  background: rgba(255, 107, 53, 0.08);
}

.rail-toggle i {
  transition: transform 0.3s ease;
}

.rail-toggle i.flipped {
  transform: rotate(180deg);
}

.dashboard-content {
  padding: 24px 24px 32px;
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.dashboard-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 20px;
}

.dashboard-header h1 {
  margin: 0;
  font-size: 1.8rem;
}

.dashboard-header p {
  margin: 4px 0 0;
  color: var(--muted);
}

.header-actions {
  display: flex;
  gap: 10px;
}

.icon-button {
  border: 1px solid rgba(31, 19, 11, 0.08);
  background: #fff;
  width: 42px;
  height: 42px;
  border-radius: 12px;
  display: grid;
  place-items: center;
  position: relative;
  cursor: pointer;
}

.icon-button .ping {
  position: absolute;
  top: 8px;
  right: 8px;
  width: 8px;
  height: 8px;
  border-radius: 999px;
  background: #ff6b35;
}

.hero {
  display: grid;
  grid-template-columns: minmax(0, 1fr) 220px;
  gap: 18px;
  padding: 18px;
  border-radius: 18px;
  background: linear-gradient(120deg, rgba(255, 107, 53, 0.12), rgba(255, 189, 89, 0.2));
  border: 1px solid rgba(31, 19, 11, 0.08);
}

.hero-copy h2 {
  margin: 8px 0;
  font-size: 1.6rem;
}

.pill {
  display: inline-flex;
  padding: 4px 10px;
  border-radius: 999px;
  background: #fff;
  font-size: 0.75rem;
  font-weight: 600;
}

.hero-badges {
  display: flex;
  gap: 10px;
  margin-top: 14px;
}

.badge {
  padding: 6px 12px;
  border-radius: 999px;
  background: #fff;
  font-size: 0.75rem;
}

.hero-image {
  border-radius: 16px;
  background: url('https://images.unsplash.com/photo-1518770660439-4636190af475?auto=format&fit=crop&w=600&q=80')
    center / cover;
  min-height: 160px;
}

.stats {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(160px, 1fr));
  gap: 16px;
}

.stat-card {
  padding: 16px;
  border-radius: 16px;
  background: #fff;
  border: 1px solid rgba(31, 19, 11, 0.08);
  display: grid;
  gap: 6px;
}

.stat-label {
  margin: 0;
  font-size: 0.8rem;
  color: var(--muted);
}

.stat-value {
  margin: 0;
  font-size: 1.1rem;
  font-weight: 700;
}

.stat-note {
  margin: 0;
  font-size: 0.75rem;
  color: var(--muted);
}

.grid {
  display: grid;
  grid-template-columns: minmax(0, 2fr) minmax(0, 1fr);
  gap: 20px;
}

.activity {
  background: #fff;
  border-radius: 18px;
  border: 1px solid rgba(31, 19, 11, 0.08);
  padding: 18px;
}

.section-title {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.section-title h3 {
  margin: 0;
}

.link {
  border: none;
  background: none;
  color: #ff6b35;
  font-weight: 600;
  cursor: pointer;
}

.activity-list {
  margin-top: 16px;
  display: grid;
  gap: 12px;
}

.activity-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px;
  border-radius: 12px;
  background: #fffaf5;
}

.activity-icon {
  width: 36px;
  height: 36px;
  border-radius: 12px;
  display: grid;
  place-items: center;
}

.activity-info {
  flex: 1;
}

.activity-title {
  margin: 0;
  font-weight: 600;
}

.activity-desc {
  margin: 2px 0 0;
  font-size: 0.78rem;
  color: var(--muted);
}

.activity-time {
  font-size: 0.75rem;
  color: var(--muted);
}

.tone-green {
  background: rgba(46, 204, 113, 0.15);
  color: #2ecc71;
}

.tone-blue {
  background: rgba(52, 152, 219, 0.15);
  color: #3498db;
}

.tone-orange {
  background: rgba(255, 159, 67, 0.15);
  color: #ff9f43;
}

.tone-red {
  background: rgba(231, 76, 60, 0.15);
  color: #e74c3c;
}

.side-panels {
  display: grid;
  gap: 16px;
}

.panel {
  background: #fff;
  border-radius: 18px;
  border: 1px solid rgba(31, 19, 11, 0.08);
  padding: 18px;
  display: grid;
  gap: 12px;
}

.muted {
  color: var(--muted);
  margin: 0;
}

.checklist {
  list-style: none;
  padding: 0;
  margin: 0;
  display: grid;
  gap: 8px;
}

.checklist li {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 0.85rem;
}

.checklist i {
  color: #2ecc71;
}

.meter {
  display: grid;
  gap: 8px;
}

.meter-row {
  display: flex;
  justify-content: space-between;
  font-size: 0.85rem;
}

.meter-bar {
  height: 6px;
  background: #f0ebe4;
  border-radius: 999px;
  overflow: hidden;
}

.meter-fill {
  height: 100%;
  background: #ff6b35;
  border-radius: 999px;
}

.meter-fill.alt {
  background: #1f1f1f;
}

@media (max-width: 1100px) {
  .role-dashboard {
    grid-template-columns: 1fr;
  }

  .rail {
    flex-direction: row;
    align-items: center;
    overflow-x: auto;
  }

  .grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 720px) {
  .hero {
    grid-template-columns: 1fr;
  }

  .hero-image {
    min-height: 140px;
  }
}
</style>
