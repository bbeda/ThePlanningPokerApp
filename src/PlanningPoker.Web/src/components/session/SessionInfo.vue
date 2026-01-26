<template>
  <div class="bg-white rounded-lg shadow-lg p-6">
    <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
      <div>
        <h1 class="text-2xl font-bold text-gray-800 mb-2">Planning Poker Session</h1>
        <div class="flex items-center gap-2 text-sm text-gray-600">
          <span class="font-medium">Owner:</span>
          <span>{{ session?.ownerName }}</span>
        </div>
        <div class="flex items-center gap-2 mt-1">
          <span
            :class="[
              'inline-flex items-center px-2 py-1 rounded-full text-xs font-medium',
              isConnected ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
            ]"
          >
            <span class="w-2 h-2 rounded-full mr-1" :class="isConnected ? 'bg-green-500' : 'bg-red-500'"></span>
            {{ isConnected ? 'Connected' : 'Disconnected' }}
          </span>
        </div>
      </div>

      <div class="flex flex-col sm:flex-row gap-3">
        <div class="bg-gray-100 rounded-lg p-4 text-center">
          <div class="text-xs text-gray-600 mb-1">Session Code</div>
          <div class="text-2xl font-mono font-bold text-gray-800">{{ session?.sessionCode }}</div>
        </div>

        <button
          @click="copyShareLink"
          class="bg-primary hover:bg-blue-600 text-white font-semibold py-3 px-6 rounded-lg transition duration-200 flex items-center justify-center gap-2"
        >
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8.684 13.342C8.886 12.938 9 12.482 9 12c0-.482-.114-.938-.316-1.342m0 2.684a3 3 0 110-2.684m0 2.684l6.632 3.316m-6.632-6l6.632-3.316m0 0a3 3 0 105.367-2.684 3 3 0 00-5.367 2.684zm0 9.316a3 3 0 105.368 2.684 3 3 0 00-5.368-2.684z" />
          </svg>
          {{ copied ? 'Copied!' : 'Share Link' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useSessionStore } from '@/stores/sessionStore'

const sessionStore = useSessionStore()

const session = computed(() => sessionStore.session)
const isConnected = computed(() => sessionStore.isConnected)

const copied = ref(false)

function copyShareLink() {
  const url = `${window.location.origin}/join/${session.value?.sessionCode}`
  navigator.clipboard.writeText(url)
  copied.value = true
  setTimeout(() => {
    copied.value = false
  }, 2000)
}
</script>
