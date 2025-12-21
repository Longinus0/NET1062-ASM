<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  history: Array<{ FromStatus: string; ToStatus: string; ChangedAt: string; Note?: string | null }>
}>()

const timelineEvents = computed(() =>
  props.history.map((entry) => ({
    status: entry.ToStatus,
    date: entry.ChangedAt,
    note: entry.Note,
  })),
)
</script>

<template>
  <div class="timeline">
    <PTimeline :value="timelineEvents">
      <template #content="{ item }">
        <div class="timeline-card">
          <h4>{{ item.status }}</h4>
          <span>{{ item.date }}</span>
          <p v-if="item.note">{{ item.note }}</p>
        </div>
      </template>
    </PTimeline>
  </div>
</template>

<style scoped>
.timeline :deep(.p-timeline-event-content) {
  padding: 0 0 1rem 0.5rem;
}

.timeline-card {
  background: var(--surface-2);
  border-radius: var(--radius);
  padding: 12px;
  border: 1px solid rgba(31, 19, 11, 0.08);
}

.timeline-card h4 {
  margin-bottom: 4px;
}

.timeline-card span {
  color: var(--muted);
  font-size: 0.85rem;
}
</style>
