<script setup lang="ts">
import { useRouter } from 'vue-router'

const router = useRouter()
</script>

<template>
  <section class="hero">
    <div class="container hero-grid">
      <div class="hero-text">
        <p class="pill">Combo trưa giảm 20% · Từ 11:00 - 14:00</p>
        <h1>
          Đậm vị <span>thức ăn nhanh</span> theo phong cách Việt
        </h1>
        <p class="lead">
          Burger, gà rán, đồ uống mát lạnh và combo siêu lợi. Chọn món, thanh toán và theo dõi
          đơn hàng ngay lập tức.
        </p>
        <div class="actions">
          <PButton
            label="Khám phá Menu"
            class="p-button-rounded p-button-warning"
            @click="router.push('/menu')"
          />
          <PButton
            label="Tìm món ngay"
            class="p-button-rounded p-button-text"
            @click="router.push('/search')"
          />
        </div>
        <div class="hero-stats">
          <div>
            <p class="stat">4.9★</p>
            <span>Đánh giá khách hàng</span>
          </div>
          <div>
            <p class="stat">20 phút</p>
            <span>Giao siêu tốc</span>
          </div>
          <div>
            <p class="stat">120+</p>
            <span>Món mới mỗi mùa</span>
          </div>
        </div>
      </div>
      <div class="hero-card">
        <div class="hero-card-inner">
          <div class="hero-burger">
            <img
              src="https://images.unsplash.com/photo-1550547660-d9450f859349?auto=format&fit=crop&w=800&q=80"
              alt="Burger with lettuce and cheese"
            />
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<style scoped>
.hero {
  padding: 48px 0 32px;
  position: relative;
  overflow: hidden;
}

.hero::before {
  content: '';
  position: absolute;
  inset: -120px -80px auto;
  height: 260px;
  background: radial-gradient(circle at 20% 20%, rgba(255, 107, 53, 0.18), transparent 65%);
  z-index: 0;
  pointer-events: none;
}

.hero::after {
  content: '';
  position: absolute;
  inset: auto -120px -140px;
  height: 320px;
  background: radial-gradient(circle at 80% 60%, rgba(0, 166, 166, 0.16), transparent 70%);
  z-index: 0;
  pointer-events: none;
}

.hero-grid {
  display: grid;
  gap: 32px;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  align-items: center;
  position: relative;
  z-index: 1;
}

.pill {
  display: inline-flex;
  padding: 6px 14px;
  border-radius: var(--radius);
  background: rgba(255, 209, 102, 0.15);
  color: var(--accent-2);
  font-size: 0.85rem;
  letter-spacing: 0.04em;
}

h1 {
  font-family: 'Fraunces', serif;
  font-size: clamp(2.4rem, 4vw, 3.6rem);
  margin: 16px 0;
}

h1 span {
  color: var(--accent);
}

.lead {
  color: var(--muted);
  font-size: 1rem;
  margin-bottom: 20px;
}

.actions {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
  margin-bottom: 20px;
}

.hero-stats {
  display: flex;
  gap: 24px;
  flex-wrap: wrap;
  color: var(--muted);
}

.stat {
  font-weight: 600;
  color: var(--text);
}

.hero-card {
  display: flex;
  justify-content: center;
}

.hero-card-inner {
  background: linear-gradient(140deg, rgba(255, 107, 53, 0.2), rgba(0, 166, 166, 0.2));
  border-radius: var(--radius);
  padding: 16px;
  width: min(360px, 100%);
  box-shadow: var(--shadow);
  border: 0.5px solid rgba(31, 19, 11, 0.08);
}

.hero-burger {
  display: flex;
  justify-content: center;
  margin-bottom: 16px;
}

.hero-burger img {
  width: min(260px, 100%);
  height: auto;
  border-radius: var(--radius);
  object-fit: cover;
  filter: drop-shadow(0 14px 18px rgba(31, 19, 11, 0.18));
}

.card-title {
  font-weight: 600;
  margin-bottom: 12px;
  color: var(--accent);
}

ul {
  list-style: none;
  display: grid;
  gap: 8px;
  margin-bottom: 18px;
  color: var(--muted);
}
</style>
