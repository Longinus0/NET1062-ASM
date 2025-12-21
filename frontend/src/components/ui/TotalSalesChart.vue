<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  total: number
  series: { Date: string; Total: number }[]
  days: number
  selectedDays: number
}>()

const emit = defineEmits<{
  (event: 'change', value: number): void
}>()

const periods = [
  { label: '7N', value: 7 },
  { label: '30N', value: 30 },
  { label: '90N', value: 90 },
  { label: '365N', value: 365 },
]

const currency = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })

const chartPath = computed(() => {
  if (!props.series.length) return { line: '', area: '' }
  const values = props.series.map((point) => point.Total)
  const max = Math.max(...values, 1)
  const min = Math.min(...values, 0)
  const range = max - min || 1
  const width = 260
  const height = 120
  const step = props.series.length > 1 ? width / (props.series.length - 1) : width
  const points = values.map((value, index) => {
    const x = index * step
    const y = height - ((value - min) / range) * height
    return { x, y }
  })
  const line = points.map((point) => `${point.x},${point.y}`).join(' ')
  const area = `M0,${height} L${line} L${width},${height} Z`
  return { line, area }
})

const lastPoint = computed(() => props.series[props.series.length - 1]?.Total ?? 0)
</script>

<template>
  <section class="sales-card">
    <header class="sales-header">
      <div>
        <p class="caption">Doanh thu đã ghi nhận</p>
        <h3>{{ currency.format(total) }}</h3>
        <p class="sub">Chu kỳ {{ days }} ngày · Điểm cuối {{ currency.format(lastPoint) }}</p>
      </div>
    </header>

    <div class="periods">
      <button
        v-for="period in periods"
        :key="period.value"
        class="period"
        :class="{ active: selectedDays === period.value }"
        @click="emit('change', period.value)"
      >
        {{ period.label }}
      </button>
    </div>

    <div class="chart-shell" role="img" aria-label="Biểu đồ doanh thu">
      <svg viewBox="0 0 260 120">
        <defs>
          <linearGradient id="areaGradient" x1="0" x2="0" y1="0" y2="1">
            <stop offset="0%" stop-color="rgba(255, 107, 53, 0.4)" />
            <stop offset="100%" stop-color="rgba(255, 189, 89, 0.05)" />
          </linearGradient>
        </defs>
        <path v-if="chartPath.area" :d="chartPath.area" class="area" />
        <polyline v-if="chartPath.line" :points="chartPath.line" class="line" />
      </svg>
      <div class="gradient"></div>
    </div>
  </section>
</template>

<style scoped>
.sales-card {
  border-radius: 20px;
  border: 1px solid rgba(31, 19, 11, 0.08);
  padding: 18px 20px 20px;
  background: #fff;
  display: flex;
  flex-direction: column;
  gap: 16px;
  height: 100%;
}

.sales-header {
  display: flex;
  justify-content: space-between;
  gap: 16px;
}

.caption {
  margin: 0;
  font-size: 0.8rem;
  color: var(--muted);
}

.sales-header h3 {
  margin: 6px 0;
  font-size: 1.6rem;
}

.sub {
  margin: 0;
  font-size: 0.78rem;
  color: var(--muted);
}

.ghost {
  border: 1px solid rgba(31, 19, 11, 0.1);
  background: transparent;
  border-radius: 999px;
  padding: 6px 12px;
  font-weight: 600;
  cursor: pointer;
  color: var(--muted);
}

.periods {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.period {
  border: 1px solid rgba(31, 19, 11, 0.08);
  background: #fffaf5;
  border-radius: 999px;
  padding: 6px 12px;
  font-weight: 600;
  color: var(--muted);
  cursor: pointer;
}

.period.active {
  color: #1f1f1f;
  background: rgba(255, 107, 53, 0.12);
  border-color: rgba(255, 107, 53, 0.3);
}

.chart-shell {
  position: relative;
  flex: 1;
  min-height: 180px;
  border-radius: 16px;
  background: linear-gradient(120deg, rgba(255, 238, 226, 0.6), rgba(255, 250, 245, 0.9));
  padding: 10px 12px;
  overflow: hidden;
}

.chart-shell svg {
  width: 100%;
  height: 100%;
}

.line {
  fill: none;
  stroke: #ff6b35;
  stroke-width: 2.2;
  stroke-linecap: round;
  stroke-linejoin: round;
}

.area {
  fill: url(#areaGradient);
}

.gradient {
  position: absolute;
  inset: 0;
  background: radial-gradient(circle at 70% 20%, rgba(255, 189, 89, 0.3), transparent 55%);
  pointer-events: none;
}
</style>
