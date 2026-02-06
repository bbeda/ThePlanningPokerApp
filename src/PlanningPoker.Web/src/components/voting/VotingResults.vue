<template>
  <div v-if="results" class="space-y-6">
    <!-- Summary Card -->
    <div class="bg-white rounded-lg shadow-lg p-6">
      <h2 class="text-2xl font-bold text-gray-800 mb-6">Voting Results</h2>

      <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
        <div class="bg-green-50 rounded-lg p-4 text-center">
          <div class="text-sm text-gray-600 mb-1">Average</div>
          <div class="text-3xl font-bold text-secondary">{{ results.actualAverage.toFixed(1) }}</div>
          <div class="text-xs text-gray-500 mt-1">Mean of all votes</div>
        </div>

        <div class="bg-blue-50 rounded-lg p-4 text-center">
          <div class="text-sm text-gray-600 mb-1">Majority</div>
          <div class="text-3xl font-bold text-primary">{{ results.majority }}</div>
          <div class="text-xs text-gray-500 mt-1">Most common vote</div>
        </div>

        <div class="bg-emerald-50 rounded-lg p-4 text-center">
          <div class="text-sm text-gray-600 mb-1">Optimistic</div>
          <div class="text-3xl font-bold text-emerald-600">{{ results.optimistic }}</div>
          <div class="text-xs text-gray-500 mt-1">25th percentile</div>
        </div>

        <div class="bg-purple-50 rounded-lg p-4 text-center">
          <div class="text-sm text-gray-600 mb-1">Pessimistic</div>
          <div class="text-3xl font-bold text-purple-600">{{ results.pessimistic }}</div>
          <div class="text-xs text-gray-500 mt-1">75th percentile</div>
        </div>
      </div>

      <!-- Vote Distribution -->
      <VoteDistribution :distribution="results.distribution" :total="results.totalVotes" />
    </div>

    <!-- Individual Votes -->
    <div class="bg-white rounded-lg shadow-lg p-6">
      <h3 class="text-xl font-bold text-gray-800 mb-4">All Votes</h3>

      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
        <div
          v-for="vote in sortedVotes"
          :key="vote.userId"
          class="flex items-center justify-between p-3 bg-gray-50 rounded-lg"
        >
          <div class="flex items-center gap-3">
            <div class="flex-shrink-0 w-10 h-10 bg-primary rounded-full flex items-center justify-center text-white font-bold">
              {{ vote.userName.charAt(0).toUpperCase() }}
            </div>
            <div class="font-medium text-gray-800">{{ vote.userName }}</div>
          </div>
          <div class="text-2xl font-bold text-primary">{{ vote.value }}</div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useSessionStore } from '@/stores/sessionStore'
import VoteDistribution from './VoteDistribution.vue'

const sessionStore = useSessionStore()

const currentRound = computed(() => sessionStore.currentRound)
const results = computed(() => currentRound.value?.results)

const sortedVotes = computed(() => {
  if (!currentRound.value?.votes) return []
  return [...currentRound.value.votes].sort((a, b) => {
    if (a.value === null) return 1
    if (b.value === null) return -1
    return a.value - b.value
  })
})
</script>
