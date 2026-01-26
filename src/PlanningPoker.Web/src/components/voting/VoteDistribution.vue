<template>
  <div>
    <h3 class="text-lg font-semibold text-gray-800 mb-4">Vote Distribution</h3>

    <div class="space-y-3">
      <div
        v-for="(count, value) in sortedDistribution"
        :key="value"
        class="flex items-center gap-3"
      >
        <div class="w-12 text-right font-bold text-gray-700">{{ value }}</div>

        <div class="flex-1 bg-gray-200 rounded-full h-10 relative overflow-hidden">
          <div
            class="bg-gradient-to-r from-primary to-blue-600 h-full rounded-full flex items-center justify-end px-3 transition-all duration-500"
            :style="{ width: `${getPercentage(count)}%` }"
          >
            <span class="text-white font-bold text-sm">{{ count }}</span>
          </div>
        </div>

        <div class="w-12 text-left text-sm text-gray-600">
          {{ getPercentage(count).toFixed(0) }}%
        </div>
      </div>
    </div>

    <div class="mt-4 text-sm text-gray-600 text-center">
      Total votes: {{ total }}
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  distribution: Record<number, number>
  total: number
}

const props = defineProps<Props>()

const sortedDistribution = computed(() => {
  const entries = Object.entries(props.distribution)
    .map(([value, count]) => [Number(value), count] as [number, number])
    .sort((a, b) => a[0] - b[0])

  return Object.fromEntries(entries)
})

function getPercentage(count: number): number {
  if (props.total === 0) return 0
  return (count / props.total) * 100
}
</script>
