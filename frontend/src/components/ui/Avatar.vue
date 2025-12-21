<script setup lang="ts">
import { computed, ref } from 'vue'

const props = withDefaults(
  defineProps<{
    src?: string | null
    alt?: string
    fallback?: string
    size?: number
  }>(),
  {
    size: 40,
    alt: 'User avatar',
  },
)

const hasError = ref(false)
const showImage = computed(() => !!props.src && !hasError.value)

const onError = () => {
  hasError.value = true
}
</script>

<template>
  <div class="avatar" :style="{ width: `${props.size}px`, height: `${props.size}px` }">
    <img v-if="showImage" :src="props.src" :alt="props.alt" @error="onError" />
    <span v-else class="avatar-fallback">{{ props.fallback || 'U' }}</span>
  </div>
</template>

<style scoped>
.avatar {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
  border-radius: var(--radius);
  background: var(--surface);
  border: 1px solid rgba(31, 19, 11, 0.12);
  color: var(--text);
  font-weight: 600;
  font-size: 0.75rem;
}

.avatar img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

.avatar-fallback {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 100%;
  height: 100%;
}
</style>
