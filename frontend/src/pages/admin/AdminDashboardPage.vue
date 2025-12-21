<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useAdminStore } from '../../stores/admin'
import TotalSalesChart from '../../components/ui/TotalSalesChart.vue'

const admin = useAdminStore()

const loadData = async () => {
  await Promise.all([admin.loadDashboard(), admin.loadRevenue(admin.revenueDays)])
}

onMounted(loadData)

const currency = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })

const summaryCards = computed(() => [
  { label: 'Tổng người dùng', value: admin.users.length, accent: 'accent' },
  { label: 'Tổng đơn hàng', value: admin.orders.length, accent: 'accent-2' },
  { label: 'Tổng sản phẩm', value: admin.products.length, accent: 'accent' },
  { label: 'Doanh thu', value: currency.format(admin.revenueTotal), accent: 'accent-2' },
])

const trendingProducts = computed(() =>
  [...admin.products]
    .sort((a, b) => b.Price - a.Price)
    .slice(0, 5),
)

const recentOrders = computed(() =>
  [...admin.orders]
    .sort((a, b) => new Date(b.CreatedAt).getTime() - new Date(a.CreatedAt).getTime())
    .slice(0, 6),
)

const recentCustomers = computed(() =>
  [...admin.users]
    .sort((a, b) => new Date(b.CreatedAt).getTime() - new Date(a.CreatedAt).getTime())
    .slice(0, 6),
)

const timeAgo = (isoDate: string) => {
  const date = new Date(isoDate)
  const diffMs = Date.now() - date.getTime()
  const minutes = Math.max(1, Math.floor(diffMs / 60000))
  if (minutes < 60) return `${minutes} phút trước`
  const hours = Math.floor(minutes / 60)
  if (hours < 24) return `${hours} giờ trước`
  const days = Math.floor(hours / 24)
  return `${days} ngày trước`
}

const statusClass = (status: string) => {
  if (status === 'Đã giao') return 'status-success'
  if (status === 'Đang giao') return 'status-info'
  if (status === 'Đang chuẩn bị') return 'status-warn'
  if (status === 'Đã hủy') return 'status-danger'
  return 'status-neutral'
}

const paymentClass = (status: string) => {
  if (status === 'Đã thanh toán') return 'status-success'
  if (status === 'Đang xử lý') return 'status-warn'
  if (status === 'Đã hoàn tiền') return 'status-danger'
  if (status === 'Thất bại') return 'status-danger'
  return 'status-neutral'
}
</script>

<template>
  <section class="dashboard">
    <header class="dash-header">
      <div>
        <p class="eyebrow">Bảng điều khiển</p>
        <h1>Quản trị tổng quan</h1>
        <p class="muted">Theo dõi nhanh sức khỏe vận hành và doanh thu.</p>
      </div>
    </header>

    <div class="summary-row">
      <article v-for="card in summaryCards" :key="card.label" class="summary-card">
        <div class="summary-icon" :class="card.accent">
          <i class="pi pi-chart-line"></i>
        </div>
        <div>
          <p>{{ card.label }}</p>
          <strong>{{ card.value }}</strong>
        </div>
      </article>
    </div>

    <div class="content-grid">
      <section class="panel">
        <div class="panel-header">
          <h2>Xu hướng món bán chạy</h2>
          <span class="muted">Top theo giá niêm yết</span>
        </div>
        <div class="list">
          <div v-for="(product, index) in trendingProducts" :key="product.ProductId" class="list-row">
            <div class="rank">{{ String(index + 1).padStart(2, '0') }}</div>
            <div class="list-info">
              <h3>{{ product.Name }}</h3>
              <p>{{ product.Description }}</p>
            </div>
            <div class="list-meta">
              <span>{{ currency.format(product.Price) }}</span>
              <small>Tồn: {{ product.StockQty }}</small>
            </div>
          </div>
        </div>
      </section>

      <section id="doanh-thu" class="revenue-panel">
        <TotalSalesChart
          :total="admin.revenueTotal"
          :series="admin.revenueSeries"
          :days="admin.revenueDays"
          :selected-days="admin.revenueDays"
          @change="admin.loadRevenue"
        />
      </section>

      <aside class="panel side split">
        <div class="panel-header">
          <h2>Tóm tắt nhanh</h2>
          <span class="muted">Thông tin theo ngày</span>
        </div>
        <div class="split-grid">
          <div class="mini-cards">
            <div class="mini-card">
              <p>Đơn đã thanh toán</p>
              <strong>{{ admin.orders.filter((o) => o.PaymentStatus === 'Đã thanh toán').length }}</strong>
            </div>
            <div class="mini-card">
              <p>Đơn đang xử lý</p>
              <strong>{{ admin.orders.filter((o) => o.PaymentStatus === 'Đang xử lý').length }}</strong>
            </div>
            <div class="mini-card">
              <p>Doanh thu TB/đơn</p>
              <strong>{{
                admin.orders.length
                  ? currency.format(
                      admin.orders.reduce((sum, order) => sum + order.GrandTotal, 0) / admin.orders.length,
                    )
                  : currency.format(0)
              }}</strong>
            </div>
          </div>
          <div class="recent-customers">
            <div class="panel-header sub">
              <h3>Khách hàng gần đây</h3>
              <span class="muted">Cập nhật mới nhất</span>
            </div>
            <div class="customer-list">
              <div v-for="user in recentCustomers" :key="user.UserId" class="customer-row">
                <div>
                  <strong>{{ user.FullName }}</strong>
                  <p>{{ user.Email }}</p>
                </div>
                <span>{{ timeAgo(user.CreatedAt) }}</span>
              </div>
            </div>
          </div>
        </div>
      </aside>

      <aside class="panel side">
        <div class="panel-header">
          <h2>Hoạt động gần đây</h2>
          <span class="muted">6 đơn mới nhất</span>
        </div>
        <div class="activity-list">
          <div v-for="order in recentOrders" :key="order.OrderId" class="activity-row">
            <div>
              <strong>{{ order.OrderCode }}</strong>
              <div class="status-row">
                <span class="status-pill" :class="statusClass(order.Status)">{{ order.Status }}</span>
                <span class="status-pill" :class="paymentClass(order.PaymentStatus)">{{ order.PaymentStatus }}</span>
              </div>
            </div>
            <span>{{ currency.format(order.GrandTotal) }}</span>
          </div>
        </div>
      </aside>
    </div>
  </section>
</template>

<style scoped>
.dashboard {
  display: grid;
  gap: 20px;
}

.dash-header {
  display: flex;
  justify-content: space-between;
  gap: 20px;
  align-items: center;
}

.dash-header h1 {
  margin: 6px 0 4px;
  font-family: 'Fraunces', serif;
  font-size: clamp(2rem, 3vw, 2.6rem);
}

.eyebrow {
  margin: 0;
  font-size: 0.8rem;
  color: var(--muted);
}

.muted {
  color: var(--muted);
  margin: 0;
}

.summary-row {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: 16px;
}

.summary-card {
  display: flex;
  align-items: center;
  gap: 12px;
  border-radius: 16px;
  padding: 14px 16px;
  border: 1px solid rgba(31, 19, 11, 0.08);
  background: #fff;
}

.summary-card p {
  margin: 0;
  font-size: 0.8rem;
  color: var(--muted);
}

.summary-card strong {
  font-size: 1.2rem;
}

.summary-icon {
  width: 42px;
  height: 42px;
  border-radius: 14px;
  display: grid;
  place-items: center;
  color: #fff;
}

.summary-icon.accent {
  background: linear-gradient(140deg, var(--accent), var(--accent-2));
}

.summary-icon.accent-2 {
  background: linear-gradient(140deg, #1f1f1f, #3c3c3c);
}

.content-grid {
  display: grid;
  grid-template-columns: minmax(0, 1.6fr) minmax(0, 1fr);
  gap: 20px;
}

.panel {
  border-radius: 20px;
  border: 1px solid rgba(31, 19, 11, 0.08);
  background: #fff;
  padding: 18px;
  display: grid;
  gap: 16px;
}

.revenue-panel {
  border-radius: 20px;
  border: 1px solid rgba(31, 19, 11, 0.08);
  background: #fff;
  padding: 18px;
  display: grid;
  gap: 16px;
  min-height: 100%;
}

.revenue-panel :deep(.sales-card) {
  height: 100%;
}

.panel-header {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
}

.panel-header h2 {
  margin: 0;
  font-size: 1.1rem;
}

.list {
  display: grid;
  gap: 12px;
}

.list-row {
  display: grid;
  grid-template-columns: 34px minmax(0, 1fr) auto;
  gap: 12px;
  align-items: center;
  padding: 10px;
  border-radius: 14px;
  background: #fffaf5;
}

.rank {
  font-weight: 700;
  color: var(--muted);
}

.list-info h3 {
  margin: 0 0 4px;
  font-size: 0.95rem;
}

.list-info p {
  margin: 0;
  font-size: 0.75rem;
  color: var(--muted);
}

.list-meta {
  text-align: right;
}

.list-meta span {
  display: block;
  font-weight: 600;
}

.list-meta small {
  color: var(--muted);
}

.side {
  align-content: start;
}

.split {
  gap: 12px;
}

.split-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 16px;
}

.mini-cards {
  display: grid;
  gap: 12px;
}

.mini-card {
  padding: 12px;
  border-radius: 14px;
  background: #fffaf5;
  border: 1px solid rgba(31, 19, 11, 0.06);
}

.mini-card p {
  margin: 0 0 6px;
  font-size: 0.8rem;
  color: var(--muted);
}

.recent-customers .panel-header.sub {
  align-items: flex-start;
}

.recent-customers h3 {
  margin: 0;
  font-size: 0.95rem;
}

.customer-list {
  display: grid;
  gap: 10px;
  margin-top: 10px;
}

.customer-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: #fffaf5;
  border-radius: 14px;
  padding: 10px 12px;
  gap: 10px;
}

.customer-row p {
  margin: 4px 0 0;
  font-size: 0.75rem;
  color: var(--muted);
}

.customer-row span {
  font-size: 0.75rem;
  color: var(--muted);
}

.activity-list {
  display: grid;
  gap: 10px;
}

.activity-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: #fffaf5;
  border-radius: 14px;
  padding: 10px 12px;
}

.status-row {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  margin-top: 6px;
}

.status-pill {
  padding: 4px 10px;
  border-radius: 999px;
  font-size: 0.7rem;
  font-weight: 600;
  border: 1px solid transparent;
}

.status-success {
  background: rgba(46, 204, 113, 0.16);
  color: #1e8e5a;
  border-color: rgba(46, 204, 113, 0.35);
}

.status-info {
  background: rgba(52, 152, 219, 0.16);
  color: #1f6fb2;
  border-color: rgba(52, 152, 219, 0.35);
}

.status-warn {
  background: rgba(255, 159, 67, 0.18);
  color: #c96d17;
  border-color: rgba(255, 159, 67, 0.4);
}

.status-danger {
  background: rgba(231, 76, 60, 0.16);
  color: #c0392b;
  border-color: rgba(231, 76, 60, 0.35);
}

.status-neutral {
  background: rgba(149, 165, 166, 0.2);
  color: #566;
  border-color: rgba(149, 165, 166, 0.4);
}

@media (max-width: 1024px) {
  .content-grid {
    grid-template-columns: 1fr;
  }

  .dash-header {
    flex-direction: column;
    align-items: flex-start;
  }
}

@media (max-width: 860px) {
  .split-grid {
    grid-template-columns: 1fr;
  }
}
</style>
