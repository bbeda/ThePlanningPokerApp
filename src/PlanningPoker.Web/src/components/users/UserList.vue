<template>
  <div class="bg-white rounded-lg shadow-lg p-6">
    <h2 class="text-xl font-bold text-gray-800 mb-4">
      Participants ({{ users.length }})
    </h2>

    <div
      class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-3"
    >
      <div
        v-for="user in users"
        :key="user.id"
        :class="[
          'flex items-center gap-3 p-3 rounded-lg',
          isCurrentUser(user.id)
            ? 'bg-primary/10 ring-2 ring-primary/30'
            : 'bg-gray-50',
        ]"
      >
        <div class="relative flex-shrink-0">
          <div
            :class="[
              'w-10 h-10 rounded-full flex items-center justify-center text-white font-bold',
              isCurrentUser(user.id)
                ? 'bg-primary ring-2 ring-primary/50'
                : 'bg-primary',
            ]"
          >
            {{ user.name.charAt(0).toUpperCase() }}
          </div>
          <!-- Connection status dot -->
          <div
            :class="[
              'absolute -bottom-0.5 -right-0.5 w-3.5 h-3.5 rounded-full border-2 border-white',
              user.isConnected ? 'bg-green-500' : 'bg-red-500',
            ]"
            :title="user.isConnected ? 'Connected' : 'Disconnected'"
          ></div>
        </div>
        <div class="flex-1 min-w-0">
          <div class="font-medium text-gray-800 truncate">
            {{ user.name }}
            <span
              v-if="isCurrentUser(user.id)"
              class="text-primary font-semibold"
              >(You)</span
            >
          </div>
          <div v-if="user.isOwner" class="text-xs text-gray-500">Admin</div>
          <div
            v-if="hasVoted(user.id)"
            class="text-xs text-secondary font-medium"
          >
            ✓ Voted
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useSessionStore } from "@/stores/sessionStore";

const sessionStore = useSessionStore();

const users = computed(() => sessionStore.session?.users || []);
const currentRound = computed(() => sessionStore.currentRound);
const currentUserId = computed(() => sessionStore.currentUser?.id);

function isCurrentUser(userId: string): boolean {
  return userId === currentUserId.value;
}

function hasVoted(userId: string): boolean {
  if (!currentRound.value || currentRound.value.status !== "InProgress") {
    return false;
  }
  return currentRound.value.votes.some((v) => v.userId === userId);
}
</script>
