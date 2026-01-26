<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 flex items-center justify-center p-4">
    <div class="max-w-md w-full">
      <div class="text-center mb-8">
        <h1 class="text-5xl font-bold text-gray-800 mb-2">Planning Poker</h1>
        <p class="text-gray-600">Collaborative story point estimation for your team</p>
      </div>

      <div class="bg-white rounded-lg shadow-xl p-8 space-y-4">
        <router-link
          to="/create"
          class="block w-full bg-primary hover:bg-blue-600 text-white font-semibold py-4 px-6 rounded-lg transition duration-200 text-center"
        >
          Create New Session
        </router-link>

        <div class="relative">
          <div class="absolute inset-0 flex items-center">
            <div class="w-full border-t border-gray-300"></div>
          </div>
          <div class="relative flex justify-center text-sm">
            <span class="px-2 bg-white text-gray-500">or</span>
          </div>
        </div>

        <router-link
          to="/join"
          class="block w-full bg-gray-100 hover:bg-gray-200 text-gray-800 font-semibold py-4 px-6 rounded-lg transition duration-200 text-center"
        >
          Join Existing Session
        </router-link>
      </div>

      <div class="mt-8 text-center text-sm text-gray-600">
        <p>Fibonacci sequence: 1, 2, 3, 5, 8, 13, 21</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
</script>
