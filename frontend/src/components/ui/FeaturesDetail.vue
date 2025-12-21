<script setup lang="ts">
import { computed, ref } from 'vue'
import type { AdminOrderSummary, AdminUser } from '../../services/admin'
import type { ProductDTO } from '../../types/dtos'

const props = defineProps<{
  users: AdminUser[]
  products: ProductDTO[]
  orders: AdminOrderSummary[]
}>()

const tabs = [
  { id: 'products', label: 'Sản phẩm nổi bật' },
  { id: 'orders', label: 'Đơn hàng gần đây' },
  { id: 'users', label: 'Người dùng mới' },
]

const activeTab = ref(tabs[0].id)

const featuredProducts = computed(() => props.products.slice(0, 4))
const recentOrders = computed(() => props.orders.slice(0, 4))
const newestUsers = computed(() => props.users.slice(0, 4))

const totals = computed(() => [
  { label: 'Tổng người dùng', value: props.users.length },
  { label: 'Tổng sản phẩm', value: props.products.length },
  { label: 'Đơn hàng hôm nay', value: props.orders.length },
])
</script>

<template>
  <section class="admin-highlight">
    <header class="hero">
      <div class="hero-copy">
        <p class="eyebrow">Bảng điều khiển quản trị</p>
        <h1>Kiểm soát hoạt động theo thời gian thực</h1>
        <p class="lede">
          Theo dõi dữ liệu từ hệ thống và cập nhật nhanh các điểm nóng của cửa hàng.
        </p>
        <div class="hero-stats">
          <div v-for="item in totals" :key="item.label" class="stat-card">
            <p>{{ item.label }}</p>
            <strong>{{ item.value }}</strong>
          </div>
        </div>
      </div>
      <div class="hero-visual">
        <div class="hero-image" :style="{ backgroundImage: `url(${featuredProducts[0]?.ImageUrl || ''})` }">
          <div class="hero-overlay"></div>
        </div>
      </div>
    </header>

    <div class="tabs">
      <button
        v-for="tab in tabs"
        :key="tab.id"
        :class="['tab', { active: activeTab === tab.id }]"
        @click="activeTab = tab.id"
      >
        {{ tab.label }}
      </button>
    </div>

    <div class="panel">
      <div v-if="activeTab === 'products'" class="panel-grid">
        <article v-for="product in featuredProducts" :key="product.ProductId" class="panel-card">
          <div class="card-media" :style="{ backgroundImage: `url(${product.ImageUrl})` }"></div>
          <div class="card-body">
            <h3>{{ product.Name }}</h3>
            <p>{{ product.Description }}</p>
            <span>{{ product.Price.toLocaleString('vi-VN') }} đ</span>
          </div>
        </article>
      </div>

      <div v-else-if="activeTab === 'orders'" class="panel-grid list">
        <article v-for="order in recentOrders" :key="order.OrderId" class="panel-row">
          <div>
            <h3>#{{ order.OrderCode }}</h3>
            <p>{{ order.Status }} • {{ order.PaymentStatus }}</p>
          </div>
          <span>{{ order.GrandTotal.toLocaleString('vi-VN') }} đ</span>
        </article>
      </div>

      <div v-else class="panel-grid list">
        <article v-for="user in newestUsers" :key="user.UserId" class="panel-row">
          <div>
            <h3>{{ user.FullName }}</h3>
            <p>{{ user.Email }}</p>
          </div>
          <span>{{ user.IsActive ? 'Đang hoạt động' : 'Tạm khóa' }}</span>
        </article>
      </div>
    </div>
  </section>
</template>

<style scoped>
.admin-highlight {
  display: grid;
  gap: 24px;
}

.hero {
  display: grid;
  grid-template-columns: minmax(0, 1.1fr) minmax(0, 0.9fr);
  gap: 24px;
  padding: 24px;
  border-radius: 20px;
  border: 1px solid rgba(31, 19, 11, 0.08);
  background: linear-gradient(120deg, rgba(255, 236, 225, 0.8), rgba(255, 250, 245, 0.9));
}

.hero-copy h1 {
  margin: 8px 0 10px;
  font-size: clamp(2rem, 3vw, 2.6rem);
  font-family: 'Fraunces', serif;
}

.eyebrow {
  font-size: 0.85rem;
  font-weight: 600;
  color: var(--muted);
  margin: 0;
}

.lede {
  margin: 0;
  color: var(--muted);
}

.hero-stats {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(140px, 1fr));
  gap: 12px;
  margin-top: 16px;
}

.stat-card {
  border-radius: 14px;
  background: #fff;
  padding: 12px 14px;
  border: 1px solid rgba(31, 19, 11, 0.08);
}

.stat-card p {
  margin: 0;
  color: var(--muted);
  font-size: 0.8rem;
}

.stat-card strong {
  display: block;
  margin-top: 6px;
  font-size: 1.2rem;
}

.hero-visual {
  display: grid;
}

.hero-image {
  border-radius: 18px;
  min-height: 240px;
  background-size: cover;
  background-position: center;
  position: relative;
  overflow: hidden;
}

.hero-overlay {
  position: absolute;
  inset: 0;
  background: linear-gradient(120deg, rgba(0, 0, 0, 0.15), rgba(255, 107, 53, 0.15));
}

.tabs {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
}

.tab {
  border: 1px solid rgba(31, 19, 11, 0.1);
  border-radius: 999px;
  padding: 8px 14px;
  background: #fff;
  color: var(--muted);
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
}

.tab.active {
  color: #1f1f1f;
  border-color: rgba(255, 107, 53, 0.4);
  box-shadow: var(--shadow);
}

.panel {
  border: 1px solid rgba(31, 19, 11, 0.08);
  border-radius: 20px;
  padding: 20px;
  background: #fff;
}

.panel-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 16px;
}

.panel-grid.list {
  grid-template-columns: 1fr;
}

.panel-card {
  border-radius: 16px;
  border: 1px solid rgba(31, 19, 11, 0.08);
  overflow: hidden;
  background: #fffaf5;
  display: grid;
}

.card-media {
  aspect-ratio: 4 / 3;
  background-size: cover;
  background-position: center;
}

.card-body {
  padding: 12px 14px;
  display: grid;
  gap: 6px;
}

.card-body h3 {
  margin: 0;
  font-size: 1rem;
}

.card-body p {
  margin: 0;
  font-size: 0.8rem;
  color: var(--muted);
}

.card-body span {
  font-weight: 600;
  color: #1f1f1f;
}

.panel-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 14px;
  border-radius: 14px;
  background: #fffaf5;
}

.panel-row h3 {
  margin: 0 0 4px;
  font-size: 0.95rem;
}

.panel-row p {
  margin: 0;
  font-size: 0.8rem;
  color: var(--muted);
}

.panel-row span {
  font-weight: 600;
}

@media (max-width: 960px) {
  .hero {
    grid-template-columns: 1fr;
  }
}
</style>
