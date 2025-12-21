<script setup lang="ts">
const props = withDefaults(
  defineProps<{
    open: boolean
    duration?: number
    strokeWidth?: number
    strokeLinecap?: 'round' | 'butt' | 'square'
    strokeLinejoin?: 'round' | 'miter' | 'bevel'
  }>(),
  {
    duration: 500,
    strokeWidth: 2.5,
    strokeLinecap: 'round',
    strokeLinejoin: 'round',
  },
)
</script>

<template>
  <svg
    :stroke-width="props.strokeWidth"
    fill="none"
    stroke="currentColor"
    viewBox="0 0 32 32"
    :stroke-linecap="props.strokeLinecap"
    :stroke-linejoin="props.strokeLinejoin"
    class="menu-icon"
    :class="{ '-rotate-45': props.open }"
    :style="{ transitionDuration: `${props.duration}ms` }"
  >
    <path
      class="menu-path"
      :class="props.open ? 'path-open' : 'path-closed'"
      :style="{ transitionDuration: `${props.duration}ms` }"
      d="M27 10 13 10C10.8 10 9 8.2 9 6 9 3.5 10.8 2 13 2 15.2 2 17 3.8 17 6L17 26C17 28.2 18.8 30 21 30 23.2 30 25 28.2 25 26 25 23.8 23.2 22 21 22L7 22"
    />
    <path d="M7 16 27 16" />
  </svg>
</template>

<style scoped>
.menu-icon {
  transition: transform ease-in-out;
}

.menu-path {
  transition: all ease-in-out;
}

.path-closed {
  stroke-dasharray: 12 63;
}

.path-open {
  stroke-dasharray: 20 300;
  stroke-dashoffset: -32.42px;
}
</style>
