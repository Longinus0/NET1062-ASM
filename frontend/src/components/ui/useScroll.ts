import { onMounted, onUnmounted, ref } from 'vue'

export function useScroll(threshold = 10) {
  const scrolled = ref(false)

  const onScroll = () => {
    scrolled.value = window.scrollY > threshold
  }

  onMounted(() => {
    window.addEventListener('scroll', onScroll)
    onScroll()
  })

  onUnmounted(() => {
    window.removeEventListener('scroll', onScroll)
  })

  return scrolled
}
