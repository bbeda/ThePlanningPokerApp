<template>
  <div class="bg-white rounded-lg shadow-lg p-6">
    <h2 class="text-xl font-bold text-gray-800 mb-4">Select Your Estimate</h2>

    <div class="grid grid-cols-3 sm:grid-cols-4 md:grid-cols-7 gap-4">
      <button
        v-for="value in fibonacci.values"
        :key="value"
        @click="handleVote(value)"
        :disabled="loading"
        :class="[
          'aspect-[3/4] rounded-lg font-bold text-2xl transition duration-200 transform hover:scale-105 disabled:opacity-50 disabled:cursor-not-allowed',
          isSelected(value)
            ? 'bg-primary text-white ring-4 ring-blue-300'
            : 'bg-gray-100 hover:bg-gray-200 text-gray-800'
        ]"
      >
        {{ value }}
      </button>
    </div>

    <div v-if="currentVote" class="mt-4 text-center text-sm text-gray-600">
      Your current vote: <span class="font-bold text-primary">{{ currentVote.value }}</span>
      <span class="text-gray-500 ml-2">(You can change it anytime)</span>
    </div>

    <div v-if="error" class="mt-4 bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg">
      {{ error }}
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useApi } from '@/composables/useApi'
import { useFibonacci } from '@/composables/useFibonacci'
import { useSessionStore } from '@/stores/sessionStore'

const api = useApi()
const fibonacci = useFibonacci()
const sessionStore = useSessionStore()

const loading = ref(false)
const error = ref<string | null>(null)

const currentVote = computed(() => sessionStore.currentVote)

function isSelected(value: number): boolean {
  return currentVote.value?.value === value
}

async function handleVote(value: number) {
  if (!sessionStore.session || !sessionStore.currentUser) return

  loading.value = true
  error.value = null

  try {
    await api.submitVote(sessionStore.session.sessionCode, sessionStore.currentUser.id, value)
    // Update local state immediately for better UX
    sessionStore.updateVote({
      userId: sessionStore.currentUser.id,
      userName: sessionStore.currentUser.name,
      value: value,
      submittedAt: new Date()
    })
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Failed to submit vote'
  } finally {
    loading.value = false
  }
}
</script>
